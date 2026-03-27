using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Compras;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Compras;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.BusinessObjects.Tpv;
using erp.Module.BusinessObjects.Servicios.PartesTrabajo;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.Factories;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.BusinessObjects.Impuestos;

namespace erp.Module.Services.Setup;

public class InformacionEmpresaSetupService(IObjectSpace objectSpace)
{
    private IObjectSpace? _os;
    private IObjectSpace OS => _os ??= GetWorkingObjectSpace();

    private IObjectSpace GetWorkingObjectSpace()
    {
        if (objectSpace is CompositeObjectSpace compositeOS)
        {
            return compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os.IsKnownType(typeof(InformacionEmpresa))) ?? objectSpace;
        }

        return objectSpace;
    }

    public void CreateInitialInformacionEmpresa()
    {
        if (!OS.IsKnownType(typeof(InformacionEmpresa))) return;

        var informacionEmpresa = OS.FirstOrDefault<InformacionEmpresa>(i => true);
        if (informacionEmpresa == null)
        {
            informacionEmpresa = OS.CreateObject<InformacionEmpresa>();
        }

        // Siempre establecemos estos valores o nos aseguramos de que existan
        
        if (string.IsNullOrEmpty(informacionEmpresa.Nombre)) informacionEmpresa.Nombre = "Empresa por Defecto";
        if (string.IsNullOrEmpty(informacionEmpresa.Nif)) informacionEmpresa.Nif = "B00000000";
        
        if (string.IsNullOrEmpty(informacionEmpresa.NombreReporteTicket)) informacionEmpresa.NombreReporteTicket = "Ticket Factura Simplificada";
        
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoAsientosPorDefecto)) informacionEmpresa.PrefijoAsientosPorDefecto = "AS";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoOfertasCompraPorDefecto)) informacionEmpresa.PrefijoOfertasCompraPorDefecto = "CO";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoPedidosCompraPorDefecto)) informacionEmpresa.PrefijoPedidosCompraPorDefecto = "CP";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoAlbaranesCompraPorDefecto)) informacionEmpresa.PrefijoAlbaranesCompraPorDefecto = "CA";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoFacturasCompraPorDefecto)) informacionEmpresa.PrefijoFacturasCompraPorDefecto = "CF";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoOfertasVentaPorDefecto)) informacionEmpresa.PrefijoOfertasVentaPorDefecto = "VO";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoPedidosPorDefecto)) informacionEmpresa.PrefijoPedidosPorDefecto = "VP";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoAlbaranesVentaPorDefecto)) informacionEmpresa.PrefijoAlbaranesVentaPorDefecto = "VA";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoFacturasVentaPorDefecto)) informacionEmpresa.PrefijoFacturasVentaPorDefecto = "VF";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoFacturasSimplificadasPorDefecto)) informacionEmpresa.PrefijoFacturasSimplificadasPorDefecto = "VS";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoSesionTpvPorDefecto)) informacionEmpresa.PrefijoSesionTpvPorDefecto = "TS";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoVentaTpvPorDefecto)) informacionEmpresa.PrefijoVentaTpvPorDefecto = "TV";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoParteTrabajoPorDefecto)) informacionEmpresa.PrefijoParteTrabajoPorDefecto = "PT";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoClientes)) informacionEmpresa.PrefijoClientes = "TC";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoProveedores)) informacionEmpresa.PrefijoProveedores = "TP";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoAcreedores)) informacionEmpresa.PrefijoAcreedores = "TA";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoEmpleados)) informacionEmpresa.PrefijoEmpleados = "TE";
        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoReservas)) informacionEmpresa.PrefijoReservas = "AR";

        if (informacionEmpresa.PaddingNumero == 0) informacionEmpresa.PaddingNumero = 5;
        if (informacionEmpresa.PaddingCuentaContable == 0) informacionEmpresa.PaddingCuentaContable = 10;

        int paddingCC = informacionEmpresa.PaddingCuentaContable;

        informacionEmpresa.CuentaPadreClientes ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "430".PadRight(paddingCC, '0').Substring(0, 5) || c.Codigo == "430" || c.Codigo == "43000");
        informacionEmpresa.CuentaPadreProveedores ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "400".PadRight(paddingCC, '0').Substring(0, 5) || c.Codigo == "400" || c.Codigo == "40000");
        informacionEmpresa.CuentaPadreAcreedores ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "410".PadRight(paddingCC, '0').Substring(0, 5) || c.Codigo == "410" || c.Codigo == "41000");
        
        informacionEmpresa.CuentaComprasPorDefecto ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "600".PadRight(paddingCC, '0'));
        informacionEmpresa.CuentaVentasPorDefecto ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "700".PadRight(paddingCC, '0'));
        informacionEmpresa.CuentaCobrosPorDefecto ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "572".PadRight(paddingCC, '0'));
        informacionEmpresa.CuentaPagosPorDefecto ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "572".PadRight(paddingCC, '0'));
        
        informacionEmpresa.DiarioVentasPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Ventas");
        informacionEmpresa.DiarioComprasPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Compras");
        informacionEmpresa.DiarioTesoreriaPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Tesorería");
        informacionEmpresa.DiarioOperacionesVariasPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Operaciones Varias");
        informacionEmpresa.DiarioAperturaPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Apertura");
        informacionEmpresa.DiarioCierrePorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Cierre");
        informacionEmpresa.DiarioRegularizacionPorDefecto ??= OS.FirstOrDefault<Diario>(d => d.Nombre == "Regularización");
        informacionEmpresa.PosicionFiscalPorDefecto ??= OS.FirstOrDefault<PosicionFiscal>(p => p.Nombre == "Régimen Nacional");
        informacionEmpresa.ZonaHorariaPorDefecto ??= OS.FirstOrDefault<ZonaHoraria>(z => z.IdZonaHoraria == "Europe/Madrid");

        informacionEmpresa.MedioPagoPorDefecto ??= OS.FirstOrDefault<MedioPago>(m => m.Nombre == "Efectivo");
        informacionEmpresa.CondicionPagoPorDefecto ??= OS.FirstOrDefault<CondicionPago>(c => c.Nombre == "Contado");

        // --- IMPUESTOS POR DEFECTO ---
        if (informacionEmpresa.ImpuestosVentas.Count == 0)
        {
            var iva21Rep = OS.FirstOrDefault<TipoImpuesto>(t => t.Codigo == "IVA21_REP");
            if (iva21Rep != null)
            {
                informacionEmpresa.ImpuestosVentas.Add(iva21Rep);
            }
        }

        if (informacionEmpresa.ImpuestosCompras.Count == 0)
        {
            var iva21Sop = OS.FirstOrDefault<TipoImpuesto>(t => t.Codigo == "IVA21_SOP");
            if (iva21Sop != null)
            {
                informacionEmpresa.ImpuestosCompras.Add(iva21Sop);
            }
        }

        OS.CommitChanges(); // Nos aseguramos de guardar la empresa inicial para evitar nulos en otras partes si es necesario
        
        InitializeAllSequences(informacionEmpresa);
    }

    private void InitializeAllSequences(InformacionEmpresa companyInfo)
    {
        if (OS is not XPObjectSpace xpOs) return;
        var session = xpOs.Session;
        var padding = companyInfo.PaddingNumero;
        var anio = companyInfo.GetLocalTime().Year;
        var mes = companyInfo.GetLocalTime().Month.ToString("D2");

        // Tipos de documentos y sus prefijos
        var docTypes = new (Type Type, string Prefix)[]
        {
            (typeof(OfertaVenta), companyInfo.PrefijoOfertasVentaPorDefecto ?? ""),
            (typeof(PedidoVenta), companyInfo.PrefijoPedidosPorDefecto ?? ""),
            (typeof(AlbaranVenta), companyInfo.PrefijoAlbaranesVentaPorDefecto ?? ""),
            (typeof(FacturaVenta), companyInfo.PrefijoFacturasVentaPorDefecto ?? ""),
            (typeof(FacturaSimplificada), companyInfo.PrefijoFacturasSimplificadasPorDefecto ?? ""),
            (typeof(OfertaCompra), companyInfo.PrefijoOfertasCompraPorDefecto ?? ""),
            (typeof(PedidoCompra), companyInfo.PrefijoPedidosCompraPorDefecto ?? ""),
            (typeof(AlbaranCompra), companyInfo.PrefijoAlbaranesCompraPorDefecto ?? ""),
            (typeof(FacturaCompra), companyInfo.PrefijoFacturasCompraPorDefecto ?? ""),
            (typeof(ParteTrabajo), companyInfo.PrefijoParteTrabajoPorDefecto ?? ""),
        };

        foreach (var (type, prefix) in docTypes)
        {
            if (string.IsNullOrEmpty(prefix)) continue;

            var sequenceName = companyInfo.TipoNumeracionDocumento switch
            {
                TipoNumeracionDocumento.PrefijoNumero => $"{type.FullName}",
                TipoNumeracionDocumento.PrefijoEjercicioNumero => $"{type.FullName}.{anio}",
                TipoNumeracionDocumento.PrefijoEjercicioMesNumero => $"{type.FullName}.{anio}.{mes}",
                _ => $"{type.FullName}.{anio}"
            };

            SequenceFactory.EnsureSequenceExists(session, sequenceName, prefix, padding);
        }

        // Contactos (estos suelen ser Prefijo/Número siempre según Contacto.cs)
        var contactTypes = new (Type Type, string Prefix)[]
        {
            (typeof(Cliente), companyInfo.PrefijoClientes ?? ""),
            (typeof(Proveedor), companyInfo.PrefijoProveedores ?? ""),
            (typeof(Acreedor), companyInfo.PrefijoAcreedores ?? ""),
            (typeof(Empleado), companyInfo.PrefijoEmpleados ?? ""),
        };

        foreach (var (type, prefix) in contactTypes)
        {
            if (string.IsNullOrEmpty(prefix)) continue;
            SequenceFactory.EnsureSequenceExists(session, type.FullName!, prefix, padding);
        }

        // TPV (Requieren código de TPV, inicializamos para un TPV '01' por defecto si existe o genérico)
        var tpv = OS.FirstOrDefault<BusinessObjects.Tpv.Tpv>(t => true);
        var tpvCodigo = tpv?.Codigo ?? "01";
        
        var tpvTypes = new (Type Type, string Prefix)[]
        {
            (typeof(SesionTpv), companyInfo.PrefijoSesionTpvPorDefecto ?? ""),
            (typeof(VentaTpv), companyInfo.PrefijoVentaTpvPorDefecto ?? ""),
        };

        foreach (var (type, prefix) in tpvTypes)
        {
            if (string.IsNullOrEmpty(prefix)) continue;
            
            var sequenceName = companyInfo.TipoNumeracionDocumento switch
            {
                TipoNumeracionDocumento.PrefijoNumero => $"{type.FullName}.{tpvCodigo}",
                TipoNumeracionDocumento.PrefijoEjercicioNumero => $"{type.FullName}.{anio}.{tpvCodigo}",
                TipoNumeracionDocumento.PrefijoEjercicioMesNumero => $"{type.FullName}.{anio}.{mes}.{tpvCodigo}",
                _ => $"{type.FullName}.{anio}.{tpvCodigo}"
            };
            
            var fullPrefix = $"{prefix}/{tpvCodigo}";
            SequenceFactory.EnsureSequenceExists(session, sequenceName, fullPrefix, padding);
        }
    }
}