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
        }
    }
}