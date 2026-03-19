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
        Console.WriteLine($"[DEBUG_LOG] Iniciando Data Seeding para Tenant: {tenantName ?? "N/A"} (ID: {tenantId})");

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
            Console.WriteLine("[DEBUG_LOG] No se puede instanciar ApplicationUser, saltando siembra de datos.");
            return;
        }

        Console.WriteLine("[DEBUG_LOG] Ejecutando TenantSetupService...");
        new TenantSetupService(objectSpace).CreateInitialTenants(tenantName ?? "Default");

        Console.WriteLine("[DEBUG_LOG] Ejecutando SecuritySetupService...");
        new SecuritySetupService(objectSpace).CreateRolesAndUsers(tenantName ?? "Default");

        if (tenantId != null)
        {
            Console.WriteLine("[DEBUG_LOG] Ejecutando CuentaSetupService...");
            new CuentaSetupService(objectSpace).CreateInitialCuentas();
            Console.WriteLine("[DEBUG_LOG] Ejecutando ImpuestoSetupService...");
            new ImpuestoSetupService(objectSpace).CreateInitialImpuestos();
            Console.WriteLine("[DEBUG_LOG] Ejecutando InformacionEmpresaSetupService...");
            new InformacionEmpresaSetupService(objectSpace).CreateInitialInformacionEmpresa();
        }
        else
        {
            Console.WriteLine("[DEBUG_LOG] TenantId es nulo, saltando servicios específicos de tenant.");
        }

        Console.WriteLine("[DEBUG_LOG] Realizando CommitChanges en DataSeedService...");
        objectSpace.CommitChanges();
        Console.WriteLine("[DEBUG_LOG] Data Seeding finalizado con éxito.");
    }
}
