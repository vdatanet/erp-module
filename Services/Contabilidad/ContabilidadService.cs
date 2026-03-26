using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.Helpers.Contactos;

namespace erp.Module.Services.Contabilidad;

public static class ContabilidadService
{
    public static Asiento? ContabilizarFactura(FacturaBase factura)
    {
        if (factura == null) return null;
        if (factura.AsientoContable != null) return factura.AsientoContable;

        var session = factura.Session;
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(session);
        if (companyInfo == null) return null;

        var fechaAsiento = factura.Fecha;
        var ejercicio = session.FindObject<Ejercicio>(
            CriteriaOperator.Parse("FechaInicio <= ? AND FechaFin >= ?", fechaAsiento, fechaAsiento));

        if (ejercicio == null)
        {
            throw new UserFriendlyException($"No se ha encontrado un ejercicio contable para la fecha {fechaAsiento:d}.");
        }

        if (ejercicio.Estado != EstadoEjercicio.Abierto)
        {
            throw new UserFriendlyException($"El ejercicio contable {ejercicio.Anio} no está abierto.");
        }

        var diario = (factura.Cliente as Cliente)?.DiarioVentas ?? companyInfo.DiarioVentasPorDefecto;
        if (diario == null)
        {
            throw new UserFriendlyException("No se ha definido el Diario de Ventas por defecto en la configuración de la empresa ni en el cliente.");
        }

        var asiento = new Asiento(session)
        {
            Fecha = fechaAsiento,
            Ejercicio = ejercicio,
            Diario = diario,
            Serie = companyInfo.PrefijoAsientosPorDefecto ?? "GEN",
            Concepto = $"Factura {factura.Secuencia} - {factura.Cliente?.Nombre ?? factura.NombreCliente}",
            Estado = EstadoAsiento.Borrador
        };

        // 1. Apunte de Cliente (Debe)
        var cuentaCliente = (factura.Cliente as Cliente)?.CuentaCobro ?? companyInfo.CuentaClientesPorDefecto;
        if (cuentaCliente == null)
        {
            throw new UserFriendlyException("No se ha definido la cuenta contable del cliente ni una por defecto.");
        }

        var apunteCliente = new Apunte(session)
        {
            Asiento = asiento,
            CuentaContable = cuentaCliente,
            Tercero = factura.Cliente,
            Concepto = asiento.Concepto,
            Debe = factura.ImporteTotal,
            Haber = 0
        };

        // 2. Apuntes de Ventas (Haber) - Agrupados por cuenta contable
        var basesPorCuenta = factura.Lineas
            .GroupBy(l => l.CuentaContable ?? companyInfo.CuentaVentasPorDefecto)
            .Where(g => g.Key != null);

        foreach (var grupo in basesPorCuenta)
        {
            var apunteVenta = new Apunte(session)
            {
                Asiento = asiento,
                CuentaContable = grupo.Key,
                Concepto = asiento.Concepto,
                Debe = 0,
                Haber = grupo.Sum(l => l.BaseImponible)
            };
        }

        // 3. Apuntes de IVA (Haber) - Agrupados por cuenta de impuesto
        var impuestosAgrupados = factura.Lineas
            .SelectMany(l => l.Impuestos)
            .GroupBy(i => i.CuentaContable)
            .Where(g => g.Key != null);

        foreach (var grupo in impuestosAgrupados)
        {
            var apunteIva = new Apunte(session)
            {
                Asiento = asiento,
                CuentaContable = grupo.Key,
                Concepto = asiento.Concepto,
                Debe = 0,
                Haber = grupo.Sum(i => i.ImporteImpuestos)
            };
        }

        // Validar descuadre (por redondeos u otros)
        asiento.UpdateSums();
        var descuadre = asiento.SumaDebe - asiento.SumaHaber;
        if (Math.Abs(descuadre) > 0)
        {
            // Si el descuadre es mínimo (redondeo), lo ajustamos en el último apunte de venta
            if (Math.Abs(descuadre) <= 0.05m) 
            {
                var ultimoApunte = asiento.Apuntes.LastOrDefault(a => a.Haber > 0);
                if (ultimoApunte != null)
                {
                    ultimoApunte.Haber += descuadre;
                    asiento.UpdateSums();
                }
            }
            else
            {
                throw new UserFriendlyException($"La factura presenta un descuadre contable de {descuadre:N2} que supera el margen de redondeo permitido.");
            }
        }

        factura.AsientoContable = asiento;
        return asiento;
    }
}
