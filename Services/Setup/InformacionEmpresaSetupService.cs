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
            informacionEmpresa.PrefijoClientes = "C";
            informacionEmpresa.PrefijoProveedores = "P";
            informacionEmpresa.PrefijoAcreedores = "A";
            informacionEmpresa.PrefijoEmpleados = "E";
            informacionEmpresa.PrefijoReservas = "RES";
            informacionEmpresa.PaddingNumero = 5;
            objectSpace.CommitChanges(); // Nos aseguramos de guardar la empresa inicial para evitar nulos en otras partes si es necesario
        }
    }
}