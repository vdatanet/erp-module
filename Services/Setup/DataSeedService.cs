using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Security;
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
        // El TenantId es la fuente de verdad:
        // Si es null => Host
        // Si tiene valor => Tenant
        var isHost = tenantId == null;

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

#if DEBUG
        // En DEBUG, siempre permite la siembra automática.
        if (isHost)
        {
            var securitySetup = new SecuritySetupService(objectSpace);
            var adminRole = securitySetup.CreateAdminRole();
            var adminUserName = string.IsNullOrEmpty(tenantName) ? "admin" : $"admin@{tenantName.ToLower()}";
            var existingUser = objectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == adminUserName);
            if (existingUser == null)
            {
                var adminUser = objectSpace.CreateObject<ApplicationUser>();
                adminUser.UserName = adminUserName;
                adminUser.SetPassword("");
                adminUser.ChangePasswordOnFirstLogon = true;
                adminUser.Roles.Add(adminRole);
                
                if (adminUser is ISecurityUserWithLoginInfo userWithLoginInfo)
                {
                    userWithLoginInfo.CreateUserLoginInfo(DevExpress.ExpressApp.Security.SecurityDefaults.PasswordAuthentication, adminUserName);
                }
            }

            // Crear un tenant de ejemplo 'demo' en postgresql local.
            var tenantSetup = new TenantSetupService(objectSpace);
            tenantSetup.CreateTenant("demo", "erp_demo", "postgres", "db-local", "joan", "");
            
            objectSpace.CommitChanges();
            return;
        }

        // Si hemos llegado hasta aquí y no somos el Host, estamos en un tenant.
        SeedTenant(objectSpace, tenantName);
#endif
    }

    private void SeedTenant(IObjectSpace objectSpace, string? tenantName)
    {
        new SecuritySetupService(objectSpace).CreateRolesAndUsers(tenantName, true);
        objectSpace.CommitChanges();

        new PaisProvinciaPoblacionSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        new ZonaHorariaSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        new InformacionEmpresaSetupService(objectSpace, serviceProvider).CreateInitialInformacionEmpresa(tenantName);
        objectSpace.CommitChanges();

        // En la siembra inicial del tenant, es posible que el tenantId aun no esté disponible en el provider, 
        // pero si hemos llegado hasta aquí y no somos el Host, estamos en un tenant.
        new ContabilidadSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        new ImpuestoSetupService(objectSpace).CreateInitialImpuestos();
        objectSpace.CommitChanges();

        new TesoreriaSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        new TipoDocumentoSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        new EtiquetaDocumentoSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        new ProductoSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        // Llamamos de nuevo a InformacionEmpresaSetupService para que asigne las cuentas contables 
        // una vez ya han sido creadas por ContabilidadSetupService
        new InformacionEmpresaSetupService(objectSpace, serviceProvider).CreateInitialInformacionEmpresa(tenantName);
        objectSpace.CommitChanges();
    }
}
