using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using erp.Module.BusinessObjects;
using erp.Module.Services.Setup;

namespace erp.Module.Services.Setup;

public interface IDataSeedService
{
    void Seed(IObjectSpace objectSpace, string? tenantName, Guid? tenantId);
}

public class DataSeedService(IServiceProvider serviceProvider) : IDataSeedService
{
    public void Seed(IObjectSpace objectSpace, string? tenantName, Guid? tenantId)
    {
        // Intentamos asignar el ServiceProvider mediante reflexión si es necesario
        if (objectSpace is BaseObjectSpace baseOs && baseOs.ServiceProvider == null)
        {
            var prop = typeof(BaseObjectSpace).GetProperty("ServiceProvider", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (prop != null)
            {
                try {
                    prop.SetValue(baseOs, serviceProvider);
                } catch { }
            }
        }

        if (!objectSpace.CanInstantiate(typeof(ApplicationUser)))
        {
            return;
        }

        try {
            new SecuritySetupService(objectSpace).CreateRolesAndUsers(tenantName);
            
            if (tenantId != null || tenantName != null)
            {
                new CuentaSetupService(objectSpace).CreateInitialCuentas();
                new ImpuestoSetupService(objectSpace).CreateInitialImpuestos();
                new InformacionEmpresaSetupService(objectSpace).CreateInitialInformacionEmpresa();
            }

            objectSpace.CommitChanges();
        } catch (Exception ex) {
            System.Diagnostics.Debug.WriteLine($"Error durante el Data Seeding: {ex.Message}");
            throw;
        }
    }
}
