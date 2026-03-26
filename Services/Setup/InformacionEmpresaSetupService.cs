using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contabilidad;
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

        informacionEmpresa.CuentaPadreClientes ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "43000");
        informacionEmpresa.CuentaPadreProveedores ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "40000");
        informacionEmpresa.CuentaPadreAcreedores ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "41000");
        informacionEmpresa.CuentaComprasPorDefecto ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "6000000000");
        informacionEmpresa.CuentaVentasPorDefecto ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "7000000000");
        informacionEmpresa.CuentaCobrosPorDefecto ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "5720000000");
        informacionEmpresa.CuentaPagosPorDefecto ??= OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "5720000000");
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
    }
}