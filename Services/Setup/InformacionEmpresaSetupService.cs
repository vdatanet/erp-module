using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contabilidad;

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
        bool isNew = false;
        if (informacionEmpresa == null)
        {
            informacionEmpresa = OS.CreateObject<InformacionEmpresa>();
            informacionEmpresa.Nombre = "Empresa por Defecto";
            informacionEmpresa.Nif = "B00000000";
            informacionEmpresa.PrefijoAsientosPorDefecto = "AS";
            informacionEmpresa.PrefijoOfertasCompraPorDefecto = "CO";
            informacionEmpresa.PrefijoPedidosCompraPorDefecto = "CP";
            informacionEmpresa.PrefijoAlbaranesCompraPorDefecto = "CA";
            informacionEmpresa.PrefijoFacturasCompraPorDefecto = "CF";
            informacionEmpresa.PrefijoOfertasVentaPorDefecto = "VO";
            informacionEmpresa.PrefijoPedidosPorDefecto = "VP";
            informacionEmpresa.PrefijoAlbaranesVentaPorDefecto = "VA";
            informacionEmpresa.PrefijoFacturasVentaPorDefecto = "VF";
            informacionEmpresa.PrefijoFacturasSimplificadasPorDefecto = "VS";
            informacionEmpresa.PrefijoParteTrabajoPorDefecto = "PT";
            isNew = true;
        }

        // Siempre establecemos estos valores o nos aseguramos de que existan
        if (isNew || string.IsNullOrEmpty(informacionEmpresa.PrefijoClientes))
            informacionEmpresa.PrefijoClientes = "TC";

        if (isNew || string.IsNullOrEmpty(informacionEmpresa.PrefijoProveedores))
            informacionEmpresa.PrefijoProveedores = "TP";

        if (isNew || string.IsNullOrEmpty(informacionEmpresa.PrefijoAcreedores))
            informacionEmpresa.PrefijoAcreedores = "TA";

        // Establecer CuentaPadre para clientes, proveedores y acreedores
        if (informacionEmpresa.CuentaPadreClientes == null)
            informacionEmpresa.CuentaPadreClientes = OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "43000");

        if (informacionEmpresa.CuentaPadreProveedores == null)
            informacionEmpresa.CuentaPadreProveedores = OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "40000");

        if (informacionEmpresa.CuentaPadreAcreedores == null)
            informacionEmpresa.CuentaPadreAcreedores = OS.FirstOrDefault<CuentaContable>(c => c.Codigo == "41000");

        if (isNew)
        {
            informacionEmpresa.PrefijoEmpleados = "TE";
            informacionEmpresa.PrefijoReservas = "AR";
            informacionEmpresa.PaddingNumero = 5;
            informacionEmpresa.PaddingCuentaContable = 10;
        }

        OS.CommitChanges(); // Nos aseguramos de guardar la empresa inicial para evitar nulos en otras partes si es necesario
    }
}