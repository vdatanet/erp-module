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
        // En modo DEBUG, si estamos en el Host, queremos crear el usuario 'admin' administrador.
        if (objectSpace.IsKnownType(typeof(DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant)))
        {
            // Intentamos obtener el TenantId del ServiceProvider para ver si realmente estamos en el Host.
            // En el Host el TenantId debería ser nulo.
            var isHost = true;
            try {
                var tenantProvider = objectSpace.ServiceProvider?.GetService(typeof(DevExpress.ExpressApp.MultiTenancy.ITenantProvider)) as DevExpress.ExpressApp.MultiTenancy.ITenantProvider;
                if (tenantProvider != null && tenantProvider.TenantId != null)
                {
                    isHost = false;
                }
            } catch {
                // Si no se puede obtener el provider, confiamos en IsKnownType pero sin return prematuro si no estamos seguros
            }

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

                // Crear un tenant de ejemplo 'demo' en postgresql local
                var tenantSetup = new TenantSetupService(objectSpace);
                tenantSetup.CreateTenant("demo", "erp_demo", "postgres", "db-local", "postgres", "");
                
                objectSpace.CommitChanges();
                return;
            }
        }
#else
        // Si el ObjectSpace conoce el tipo Tenant, estamos en el Host.
        // El usuario indica que la siembra solo debe hacerse en el tenant.
        if (objectSpace.IsKnownType(typeof(DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant)))
        {
            return;
        }
#endif

#if DEBUG
        new SecuritySetupService(objectSpace).CreateRolesAndUsers(tenantName);
        objectSpace.CommitChanges();

        new PaisProvinciaPoblacionSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        new ZonaHorariaSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        new InformacionEmpresaSetupService(objectSpace).CreateInitialInformacionEmpresa(tenantName);
        objectSpace.CommitChanges();

        // En la siembra inicial del tenant, es posible que el tenantId aun no esté disponible en el provider, 
        // pero si hemos llegado hasta aquí y no somos el Host, estamos en un tenant.
        new ContabilidadSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        new ImpuestoSetupService(objectSpace).CreateInitialImpuestos();
        objectSpace.CommitChanges();

        new TesoreriaSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        new ProductoSetupService(objectSpace).CreateInitialData();
        objectSpace.CommitChanges();

        // Llamamos de nuevo a InformacionEmpresaSetupService para que asigne las cuentas contables 
        // una vez ya han sido creadas por ContabilidadSetupService
        new InformacionEmpresaSetupService(objectSpace).CreateInitialInformacionEmpresa(tenantName);
        objectSpace.CommitChanges();
#endif
    }
}
