using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Tesoreria;

namespace erp.Module.Services.Setup;

public class InformacionEmpresaSetupService(IObjectSpace objectSpace)
{
    private IObjectSpace? _os;
    private IObjectSpace OS => _os ??= GetWorkingObjectSpace();

    private IObjectSpace GetWorkingObjectSpace()
    {
        if (objectSpace is CompositeObjectSpace compositeOS)
        {
            var result = compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os.IsKnownType(typeof(InformacionEmpresa)));
            if (result != null) return result;

            // Fallback to the first persistent Object Space if no specific match is found for the type
            var fallback = compositeOS.AdditionalObjectSpaces.FirstOrDefault();
            if (fallback != null) return fallback;
        }

        return objectSpace;
    }

    public void CreateInitialInformacionEmpresa()
    {
        var informacionEmpresa = OS.FirstOrDefault<InformacionEmpresa>(i => true);
        if (informacionEmpresa == null)
        {
            informacionEmpresa = OS.CreateObject<InformacionEmpresa>();
        }

        // Siempre establecemos estos valores o nos aseguramos de que existan
        informacionEmpresa.Nombre = string.IsNullOrEmpty(informacionEmpresa.Nombre) ? "Empresa por Defecto" : informacionEmpresa.Nombre;
        informacionEmpresa.Nif = string.IsNullOrEmpty(informacionEmpresa.Nif) ? "B00000000" : informacionEmpresa.Nif;
        informacionEmpresa.PrefijoAsientosPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoAsientosPorDefecto) ? "AS" : informacionEmpresa.PrefijoAsientosPorDefecto;
        informacionEmpresa.PrefijoOfertasCompraPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoOfertasCompraPorDefecto) ? "CO" : informacionEmpresa.PrefijoOfertasCompraPorDefecto;
        informacionEmpresa.PrefijoPedidosCompraPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoPedidosCompraPorDefecto) ? "CP" : informacionEmpresa.PrefijoPedidosCompraPorDefecto;
        informacionEmpresa.PrefijoAlbaranesCompraPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoAlbaranesCompraPorDefecto) ? "CA" : informacionEmpresa.PrefijoAlbaranesCompraPorDefecto;
        informacionEmpresa.PrefijoFacturasCompraPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoFacturasCompraPorDefecto) ? "CF" : informacionEmpresa.PrefijoFacturasCompraPorDefecto;
        informacionEmpresa.PrefijoOfertasVentaPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoOfertasVentaPorDefecto) ? "VO" : informacionEmpresa.PrefijoOfertasVentaPorDefecto;
        informacionEmpresa.PrefijoPedidosPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoPedidosPorDefecto) ? "VP" : informacionEmpresa.PrefijoPedidosPorDefecto;
        informacionEmpresa.PrefijoAlbaranesVentaPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoAlbaranesVentaPorDefecto) ? "VA" : informacionEmpresa.PrefijoAlbaranesVentaPorDefecto;
        informacionEmpresa.PrefijoFacturasVentaPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoFacturasVentaPorDefecto) ? "VF" : informacionEmpresa.PrefijoFacturasVentaPorDefecto;
        informacionEmpresa.PrefijoFacturasSimplificadasPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoFacturasSimplificadasPorDefecto) ? "VS" : informacionEmpresa.PrefijoFacturasSimplificadasPorDefecto;
        informacionEmpresa.PrefijoSesionTpvPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoSesionTpvPorDefecto) ? "TS" : informacionEmpresa.PrefijoSesionTpvPorDefecto;
        informacionEmpresa.PrefijoVentaTpvPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoVentaTpvPorDefecto) ? "TV" : informacionEmpresa.PrefijoVentaTpvPorDefecto;
        informacionEmpresa.PrefijoParteTrabajoPorDefecto = string.IsNullOrEmpty(informacionEmpresa.PrefijoParteTrabajoPorDefecto) ? "PT" : informacionEmpresa.PrefijoParteTrabajoPorDefecto;
        informacionEmpresa.NombreReporteTicket = string.IsNullOrEmpty(informacionEmpresa.NombreReporteTicket) ? "Ticket Factura Simplificada" : informacionEmpresa.NombreReporteTicket;

        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoClientes))
            informacionEmpresa.PrefijoClientes = "TC";

        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoProveedores))
            informacionEmpresa.PrefijoProveedores = "TP";

        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoAcreedores))
            informacionEmpresa.PrefijoAcreedores = "TA";

        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoEmpleados))
            informacionEmpresa.PrefijoEmpleados = "TE";

        if (string.IsNullOrEmpty(informacionEmpresa.PrefijoReservas))
            informacionEmpresa.PrefijoReservas = "AR";

        if (informacionEmpresa.PaddingNumero == 0)
            informacionEmpresa.PaddingNumero = 5;

        if (informacionEmpresa.PaddingCuentaContable == 0)
            informacionEmpresa.PaddingCuentaContable = 10;

        // Establecer CuentaPadre para clientes, proveedores y acreedores
        if (informacionEmpresa.CuentaPadreClientes == null)
            informacionEmpresa.CuentaPadreClientes = OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "43000");

        if (informacionEmpresa.CuentaPadreProveedores == null)
            informacionEmpresa.CuentaPadreProveedores = OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "40000");

        if (informacionEmpresa.CuentaPadreAcreedores == null)
            informacionEmpresa.CuentaPadreAcreedores = OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "41000");

        if (informacionEmpresa.ZonaHorariaPorDefecto == null)
        {
            informacionEmpresa.ZonaHorariaPorDefecto = OS.FirstOrDefault<ZonaHoraria>(z => z.IdZonaHoraria == "Europe/Madrid");
        }

        if (informacionEmpresa.MedioPagoPorDefecto == null)
        {
            var medioPagoEfectivo = OS.FirstOrDefault<MedioPago>(m => m.Nombre == "Efectivo");
            if (medioPagoEfectivo == null)
            {
                medioPagoEfectivo = OS.CreateObject<MedioPago>();
                medioPagoEfectivo.Nombre = "Efectivo";
                medioPagoEfectivo.EsEfectivo = true;
            }
            informacionEmpresa.MedioPagoPorDefecto = medioPagoEfectivo;
        }

        if (informacionEmpresa.CondicionPagoPorDefecto == null)
        {
            var condicionPagoEfectivo = OS.FirstOrDefault<CondicionPago>(c => c.Nombre == "Efectivo");
            if (condicionPagoEfectivo == null)
            {
                condicionPagoEfectivo = OS.CreateObject<CondicionPago>();
                condicionPagoEfectivo.Nombre = "Efectivo";
                condicionPagoEfectivo.NumeroPlazos = 1;
                condicionPagoEfectivo.PlazoPrimerPago = 0;
                condicionPagoEfectivo.DiasEntrePlazos = 0;
                condicionPagoEfectivo.MedioPago = informacionEmpresa.MedioPagoPorDefecto;
            }
            informacionEmpresa.CondicionPagoPorDefecto = condicionPagoEfectivo;
        }

        OS.CommitChanges(); // Nos aseguramos de guardar la empresa inicial para evitar nulos en otras partes si es necesario
    }
}