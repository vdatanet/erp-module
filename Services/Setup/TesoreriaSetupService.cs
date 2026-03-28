using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.Helpers.Contactos;

namespace erp.Module.Services.Setup;

public class TesoreriaSetupService(IObjectSpace objectSpace)
{
    private IObjectSpace? _os;
    private IObjectSpace OS => _os ??= GetWorkingObjectSpace();

    private IObjectSpace GetWorkingObjectSpace()
    {
        if (objectSpace is CompositeObjectSpace compositeOS)
        {
            return compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os.IsKnownType(typeof(MedioPago))) ?? objectSpace;
        }

        return objectSpace;
    }

    public void CreateInitialData()
    {
        if (!OS.IsKnownType(typeof(MedioPago))) return;

        // 1. Crear Medios de Pago
        var efectivo = CreateMedioPago("Efectivo", true, true);
        var tarjeta = CreateMedioPago("Tarjeta de Crédito/Débito", false, true);
        var transferencia = CreateMedioPago("Transferencia Bancaria", false, false);
        var domiciliacion = CreateMedioPago("Domiciliación Bancaria (SEPA)", false, false);

        // 2. Crear Condiciones de Pago
        CreateCondicionPago("Contado", efectivo, 0, 0, 1);
        CreateCondicionPago("Contado Tarjeta", tarjeta, 0, 0, 1);
        CreateCondicionPago("30 días", transferencia, 30, 0, 1);
        CreateCondicionPago("60 días", transferencia, 60, 0, 1);
        CreateCondicionPago("30/60 días", transferencia, 30, 30, 2);
        CreateCondicionPago("30/60/90 días", transferencia, 30, 30, 3);
        CreateCondicionPago("Recibo 30 días", domiciliacion, 30, 0, 1);
    }

    private MedioPago CreateMedioPago(string nombre, bool esEfectivo, bool disponibleEnTpv)
    {
        var medioPago = OS.FirstOrDefault<MedioPago>(m => m.Nombre == nombre);
        if (medioPago == null)
        {
            medioPago = OS.CreateObject<MedioPago>();
            medioPago.Nombre = nombre;
            medioPago.EsEfectivo = esEfectivo;
            medioPago.DisponibleEnTpv = disponibleEnTpv;

            // Asignar cuentas contables predeterminadas de la empresa
            var session = ((DevExpress.ExpressApp.Xpo.XPObjectSpace)OS).Session;
            var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(session);

            if (esEfectivo)
            {
                var cuentaEfectivo = OS.FirstOrDefault<erp.Module.BusinessObjects.Contabilidad.CuentaContable>(c => c.Codigo == "5700000000");
                if (cuentaEfectivo == null)
                {
                    cuentaEfectivo = OS.CreateObject<erp.Module.BusinessObjects.Contabilidad.CuentaContable>();
                    cuentaEfectivo.Codigo = "5700000000";
                    cuentaEfectivo.Nombre = "Caja";
                }
                medioPago.CuentaContableCobros = cuentaEfectivo;
                medioPago.CuentaContablePagos = cuentaEfectivo;
            }
            else if (companyInfo != null)
            {
                medioPago.CuentaContableCobros = companyInfo.CuentaCobrosPorDefecto;
                medioPago.CuentaContablePagos = companyInfo.CuentaPagosPorDefecto;
            }
        }
        return medioPago;
    }

    private void CreateCondicionPago(string nombre, MedioPago medioPago, int plazoPrimerPago, int diasEntrePlazos, int numeroPlazos)
    {
        var condicionPago = OS.FirstOrDefault<CondicionPago>(c => c.Nombre == nombre);
        if (condicionPago == null)
        {
            condicionPago = OS.CreateObject<CondicionPago>();
            condicionPago.Nombre = nombre;
            condicionPago.MedioPago = medioPago;
            condicionPago.PlazoPrimerPago = plazoPrimerPago;
            condicionPago.DiasEntrePlazos = diasEntrePlazos;
            condicionPago.NumeroPlazos = numeroPlazos;
        }
    }
}
