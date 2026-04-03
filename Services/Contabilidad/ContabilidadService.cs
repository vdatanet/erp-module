using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.Helpers.Contactos;

using erp.Module.Models.Contabilidad;

namespace erp.Module.Services.Contabilidad;

public static class ContabilidadService
{
    public static List<BalanceSumasSaldosItem> GetBalanceSumasSaldos(IObjectSpace objectSpace, BalanceSumasSaldosParameters parameters)
    {
        var criteria = CriteriaOperator.Parse("Asiento.Estado = ?", EstadoAsiento.Publicado);

        if (parameters.Ejercicio != null)
        {
            criteria = GroupOperator.And(criteria, CriteriaOperator.Parse("Asiento.Ejercicio.Oid = ?", parameters.Ejercicio.Oid));
        }

        if (parameters.FechaInicio.HasValue)
        {
            criteria = GroupOperator.And(criteria, CriteriaOperator.Parse("Asiento.Fecha >= ?", parameters.FechaInicio.Value));
        }

        if (parameters.FechaFin.HasValue)
        {
            criteria = GroupOperator.And(criteria, CriteriaOperator.Parse("Asiento.Fecha <= ?", parameters.FechaFin.Value));
        }

        var apuntes = objectSpace.GetObjects<Apunte>(criteria);

        var balance = apuntes
            .GroupBy(a => a.CuentaContable)
            .Select(g => new BalanceSumasSaldosItem
            {
                Codigo = g.Key?.Codigo,
                Nombre = g.Key?.Nombre,
                SumaDebe = g.Sum(a => a.Debe),
                SumaHaber = g.Sum(a => a.Haber)
            })
            .OrderBy(b => b.Codigo)
            .ToList();

        return balance;
    }

    public static List<ExtractoCuentaItem> GetExtractoCuenta(IObjectSpace objectSpace, ExtractoCuentaParameters parameters)
    {
        if (parameters.CuentaContable == null) return new List<ExtractoCuentaItem>();

        var criteria = CriteriaOperator.Parse("CuentaContable.Oid = ? AND Asiento.Estado = ?", 
            parameters.CuentaContable.Oid, EstadoAsiento.Publicado);

        if (parameters.FechaInicio.HasValue)
        {
            criteria = GroupOperator.And(criteria, CriteriaOperator.Parse("Asiento.Fecha >= ?", parameters.FechaInicio.Value));
        }

        if (parameters.FechaFin.HasValue)
        {
            criteria = GroupOperator.And(criteria, CriteriaOperator.Parse("Asiento.Fecha <= ?", parameters.FechaFin.Value));
        }

        var apuntes = objectSpace.GetObjects<Apunte>(criteria)
            .OrderBy(a => a.Asiento?.Fecha)
            .ThenBy(a => a.Asiento?.Numero)
            .ThenBy(a => a.Oid)
            .ToList();

        decimal saldoAcumulado = 0;
        var extracto = new List<ExtractoCuentaItem>();

        foreach (var apunte in apuntes)
        {
            saldoAcumulado += apunte.Debe - apunte.Haber;
            extracto.Add(new ExtractoCuentaItem
            {
                Fecha = apunte.Asiento?.Fecha ?? DateTime.MinValue,
                Asiento = apunte.Asiento?.Codigo,
                Concepto = apunte.Concepto,
                Debe = apunte.Debe,
                Haber = apunte.Haber,
                Saldo = saldoAcumulado
            });
        }

        return extracto;
    }

    public static Asiento? ContabilizarFactura(FacturaBase factura)
    {
        if (factura == null) return null;
        if (factura.AsientoContable != null) return factura.AsientoContable;

        if (string.IsNullOrEmpty(factura.Secuencia))
        {
            factura.AsignarNumero();
        }

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

        var diario = (factura.Cliente as Cliente)?.DiarioVentas;
        if (diario == null && factura.EsFacturaSimplificada)
        {
            diario = companyInfo.DiarioVentasSimplificadasPorDefecto;
        }
        diario ??= companyInfo.DiarioVentasPorDefecto;

        if (diario == null)
        {
            throw new UserFriendlyException("No se ha definido el Diario de Ventas por defecto en la configuración de la empresa ni en el cliente.");
        }

        var conceptoAsiento = $"N/Factura nº {factura.Secuencia}";
        if (factura.Cliente != null)
        {
            conceptoAsiento += $" a {factura.Cliente.Nombre}";
        }
        else if (!string.IsNullOrEmpty(factura.NombreCliente))
        {
            conceptoAsiento += $" a {factura.NombreCliente}";
        }
        else if (factura.EsFacturaSimplificada && factura.MedioPago != null)
        {
            conceptoAsiento += $" ({factura.MedioPago.Nombre})";
        }

        var asiento = new Asiento(session)
        {
            Fecha = fechaAsiento,
            Ejercicio = ejercicio,
            Diario = diario,
            Serie = companyInfo.PrefijoAsientosPorDefecto ?? "GEN",
            Concepto = conceptoAsiento,
            Estado = EstadoAsiento.Borrador
        };

        // 1. Apunte de Cliente (Debe)
        var cuentaCliente = (factura.Cliente as Cliente)?.CuentaContable;

        if (cuentaCliente == null && factura.EsFacturaSimplificada)
        {
            cuentaCliente = factura.MedioPago?.CuentaContableCobros 
                            ?? factura.CuentaCaja 
                            ?? factura.CuentaBanco 
                            ?? companyInfo.CuentaCobrosPorDefecto;
        }

        cuentaCliente ??= companyInfo.CuentaClientesPorDefecto;

        if (cuentaCliente == null)
        {
            throw new UserFriendlyException("No se ha definido la cuenta contable del cliente, la del medio de pago ni una cuenta por defecto (Caja/Banco/Cobros) para clientes.");
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
        
        if (apunteCliente.Tercero == null && factura.EsFacturaSimplificada && factura.MedioPago != null)
        {
             apunteCliente.Notas = $"Cobro mediante {factura.MedioPago.Nombre}";
        }
        
        asiento.Apuntes.Add(apunteCliente);

        // 2. Apuntes de Impuestos que son retenciones (Debe)
        var lineasConImpuestos = factura.Lineas.Where(l => l.Impuestos.Any());
        foreach (var linea in lineasConImpuestos)
        {
            foreach (var impuesto in linea.Impuestos.Where(i => i.CuentaContable != null && i.EsRetencion))
            {
                var apunteRetencion = new Apunte(session)
                {
                    Asiento = asiento,
                    CuentaContable = impuesto.CuentaContable,
                    Concepto = asiento.Concepto,
                    Debe = Math.Abs(impuesto.ImporteImpuestos),
                    Haber = 0
                };
                asiento.Apuntes.Add(apunteRetencion);
            }
        }

        // 3. Apuntes de Ventas (Haber) - Uno por cada línea de factura sin agrupar
        foreach (var linea in factura.Lineas.Where(l => (l.CuentaContable ?? companyInfo.CuentaVentasPorDefecto) != null))
        {
            var apunteVenta = new Apunte(session)
            {
                Asiento = asiento,
                CuentaContable = linea.CuentaContable ?? companyInfo.CuentaVentasPorDefecto,
                Concepto = asiento.Concepto,
                Debe = 0,
                Haber = linea.BaseImponible
            };
            asiento.Apuntes.Add(apunteVenta);
        }

        // 4. Apuntes de Impuestos que NO son retenciones (Haber)
        foreach (var linea in lineasConImpuestos)
        {
            foreach (var impuesto in linea.Impuestos.Where(i => i.CuentaContable != null && !i.EsRetencion))
            {
                var apunteImpuesto = new Apunte(session)
                {
                    Asiento = asiento,
                    CuentaContable = impuesto.CuentaContable,
                    Concepto = asiento.Concepto,
                    Debe = 0,
                    Haber = impuesto.ImporteImpuestos
                };
                asiento.Apuntes.Add(apunteImpuesto);
            }
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
