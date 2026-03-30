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

        // Determinamos si estamos en el Host o en un Tenant
        // Si el ObjectSpace conoce el tipo Tenant, estamos en el Host
        var isHost = objectSpace.IsKnownType(typeof(DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant));

        // Verificación adicional: si tenantId tiene valor, definitivamente NO estamos en el host
        if (tenantId.HasValue && tenantId.Value != Guid.Empty)
        {
            isHost = false;
        }

#if DEBUG
        // Log de diagnóstico
        System.Diagnostics.Debug.WriteLine($"[DataSeedService] Seed called - isHost: {isHost}, tenantName: {tenantName}, tenantId: {tenantId}");

        if (isHost)
        {
            // MODO DEBUG - BASE DE DATOS HOST
            // El host contiene:
            // 1. Usuario admin y roles de seguridad
            // 2. Definiciones de tenants
            // 3. Datos maestros geográficos básicos (solo lectura, para referencia)
            System.Diagnostics.Debug.WriteLine("[DataSeedService] Creando datos para HOST...");

            var securitySetup = new SecuritySetupService(objectSpace);
            var adminRole = securitySetup.CreateAdminRole();

            var adminUserName = "admin";
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
                System.Diagnostics.Debug.WriteLine("[DataSeedService] Usuario admin creado");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[DataSeedService] Usuario admin ya existe");
            }

            // Crear un tenant de ejemplo 'demo' en postgresql local
            var tenantSetup = new TenantSetupService(objectSpace);
            tenantSetup.CreateTenant("demo", "erp_demo", "postgres", "db-local", "postgres", "");

            objectSpace.CommitChanges();

            // Crear datos geográficos básicos (solo lectura para referencia)
            System.Diagnostics.Debug.WriteLine("[DataSeedService] Creando datos geográficos básicos en HOST...");
            new PaisProvinciaPoblacionSetupService(objectSpace).CreateInitialData();
            objectSpace.CommitChanges();

            System.Diagnostics.Debug.WriteLine("[DataSeedService] Datos de HOST creados. Saliendo...");
            return; // IMPORTANTE: salir aquí para no ejecutar código de tenant
        }

        // Si llegamos aquí en DEBUG, estamos en un tenant
        System.Diagnostics.Debug.WriteLine("[DataSeedService] Creando datos para TENANT...");

        // TENANT: Crear toda la estructura de datos de negocio
        // - Roles y usuarios específicos del tenant
        // - Copia de datos geográficos (pueden modificarse independientemente)
        // - Estructura contable, impuestos, tesorería, etc.
        new SecuritySetupService(objectSpace).CreateRolesAndUsers(tenantName);
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
#else
        // MODO RELEASE - Si estamos en el host, no hacer nada
        if (isHost)
        {
            return;
        }
#endif
    }
}
