using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Configuraciones;

namespace erp.Module.Services.Setup;

public class InformacionEmpresaSetupService(IObjectSpace objectSpace)
{
    public void CreateInitialInformacionEmpresa()
    {
        var informacionEmpresa = objectSpace.FirstOrDefault<InformacionEmpresa>(i => true);
        if (informacionEmpresa == null)
        {
            informacionEmpresa = objectSpace.CreateObject<InformacionEmpresa>();
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
            informacionEmpresa.PrefijoClientes = "TC";
            informacionEmpresa.PrefijoProveedores = "TP";
            informacionEmpresa.PrefijoAcreedores = "TA";
            informacionEmpresa.PrefijoEmpleados = "TE";
            informacionEmpresa.PrefijoReservas = "AR";
            informacionEmpresa.PaddingNumero = 5;
            informacionEmpresa.PaddingCuentaContable = 10;
            objectSpace.CommitChanges(); // Nos aseguramos de guardar la empresa inicial para evitar nulos en otras partes si es necesario
        }
    }
}