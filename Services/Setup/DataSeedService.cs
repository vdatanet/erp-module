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
                } catch {
                    // Si no se puede setear directamente, los servicios que lo necesiten podrían fallar, 
                    // pero al menos lo intentamos.
                }
            }
        }

        if (!objectSpace.CanInstantiate(typeof(ApplicationUser)))
        {
            return;
        }

        new TenantSetupService(objectSpace).CreateInitialTenants(tenantName ?? "Default");
        objectSpace.CommitChanges();

        new SecuritySetupService(objectSpace).CreateRolesAndUsers(tenantName);
        objectSpace.CommitChanges();

        if (tenantId != null)
        {
            new CuentaSetupService(objectSpace).CreateInitialCuentas();
            objectSpace.CommitChanges();
            
            new ImpuestoSetupService(objectSpace).CreateInitialImpuestos();
            objectSpace.CommitChanges();
            
            new InformacionEmpresaSetupService(objectSpace).CreateInitialInformacionEmpresa();
            objectSpace.CommitChanges();
        }
    }
}
