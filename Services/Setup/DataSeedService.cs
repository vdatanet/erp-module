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
                } catch (Exception) {
                    // Si no se puede setear directamente, los servicios que lo necesiten podrían fallar, 
                    // pero al menos lo intentamos.
                }
            }
        }

        if (!objectSpace.CanInstantiate(typeof(ApplicationUser)))
        {
            return;
        }

        // Si el ObjectSpace conoce el tipo Tenant, estamos en el Host.
        // El usuario indica que la siembra solo debe hacerse en el tenant.
        if (objectSpace.IsKnownType(typeof(DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant)))
        {
            return;
        }

        new SecuritySetupService(objectSpace).CreateRolesAndUsers(tenantName);
        objectSpace.CommitChanges();

        new PaisProvinciaPoblacionSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        new ZonaHorariaSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        if (tenantId != null)
        {
            new ImpuestoSetupService(objectSpace).CreateInitialImpuestos();
            objectSpace.CommitChanges();

            new TesoreriaSetupService(objectSpace).CreateInitialData();
            objectSpace.CommitChanges();

            new InformacionEmpresaSetupService(objectSpace).CreateInitialInformacionEmpresa();
            objectSpace.CommitChanges();

            new ContabilidadSetupService(objectSpace).CreateInitialData();
            objectSpace.CommitChanges();
        }
    }
}
