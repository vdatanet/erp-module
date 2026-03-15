using DevExpress.ExpressApp;
using DevExpress.ExpressApp.MultiTenancy;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.MultiTenancy;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Configuracion;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.DatabaseUpdate;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
public class Updater(IObjectSpace objectSpace, Version currentDbVersion) : ModuleUpdater(objectSpace, currentDbVersion)
{
    private Guid? TenantId => ObjectSpace.ServiceProvider.GetService<ITenantProvider>()?.TenantId;

    private string TenantName => ObjectSpace.ServiceProvider.GetService<ITenantProvider>()?.TenantName;

    public override void UpdateDatabaseAfterUpdateSchema()
    {
        base.UpdateDatabaseAfterUpdateSchema();
        //string name = "MyName";
        //DomainObject1 theObject = ObjectSpace.FirstOrDefault<DomainObject1>(u => u.Name == name);
        //if(theObject == null) {
        //    theObject = ObjectSpace.CreateObject<DomainObject1>();
        //    theObject.Name = name;
        //}
        if (!ObjectSpace.CanInstantiate(typeof(UsuarioAplicacion))) return;

        // The code below creates users and roles for testing purposes only.
        // In production code, you can create users and assign roles to them automatically, as described in the following help topic:
        // https://docs.devexpress.com/eXpressAppFramework/119064/data-security-and-safety/security-system/authentication
        // If a role doesn't exist in the database, create this role
        var adminRole = CreateAdminRole();

        var infoEmpresa = ObjectSpace.FirstOrDefault<InformacionEmpresa>(_ => true);
        if (infoEmpresa == null)
        {
            infoEmpresa = ObjectSpace.CreateObject<InformacionEmpresa>();
            infoEmpresa.Nombre = "Mi Empresa";
            infoEmpresa.Nif = "B00000000";
        }

        var userManager = ObjectSpace.ServiceProvider.GetRequiredService<UserManager>();

        if (TenantName != null)
        {
            var defaultRole = CreateDefaultRole();

            var userName = $"User@{TenantName}";
            // If a user named 'userName' doesn't exist in the database, create this user
            if (userManager.FindUserByName<UsuarioAplicacion>(ObjectSpace, userName) == null)
            {
                // Set a password if the standard authentication type is used
                var emptyPassword = "";
                _ = userManager.CreateUser<UsuarioAplicacion>(ObjectSpace, userName, emptyPassword, user =>
                {
                    // Add the Users role to the user
                    user.Roles.Add(defaultRole);
                });
            }
        }

        var adminUserName = TenantName != null ? $"Admin@{TenantName}" : "Admin";
        if (userManager.FindUserByName<UsuarioAplicacion>(ObjectSpace, adminUserName) == null)
        {
            // Set a password if the standard authentication type is used
            var emptyPassword = "";
            _ = userManager.CreateUser<UsuarioAplicacion>(ObjectSpace, adminUserName, emptyPassword, user =>
            {
                // Add the Administrators role to the user
                user.Roles.Add(adminRole);
            });
        }

        ObjectSpace.CommitChanges(); //This line persists created object(s).
    }

    public override void UpdateDatabaseBeforeUpdateSchema()
    {
        base.UpdateDatabaseBeforeUpdateSchema();
        //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
        //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
        //}
    }

    private Tenant CreateTenant(string tenantName, string databaseName)
    {
        var tenant = ObjectSpace.FirstOrDefault<Tenant>(t => t.Name == tenantName);
        if (tenant == null)
        {
            tenant = ObjectSpace.CreateObject<Tenant>();
            tenant.Name = tenantName;
            tenant.ConnectionString =
                $"XpoProvider=MySql;server=localhost;user=root;password=password;database={databaseName}";
        }

        return tenant;
    }

    private PermissionPolicyRole CreateAdminRole()
    {
        var adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
        if (adminRole == null)
        {
            adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            adminRole.Name = "Administrators";
            adminRole.IsAdministrative = true;
        }

        return adminRole;
    }

    private PermissionPolicyRole CreateDefaultRole()
    {
        var defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
        if (defaultRole == null)
        {
            defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            defaultRole.Name = "Default";

            defaultRole.AddObjectPermissionFromLambda<UsuarioAplicacion>(SecurityOperations.Read,
                cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails",
                SecurityPermissionState.Allow);
            defaultRole.AddMemberPermissionFromLambda<UsuarioAplicacion>(SecurityOperations.Write,
                "ChangePasswordOnFirstLogon", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(),
                SecurityPermissionState.Allow);
            defaultRole.AddMemberPermissionFromLambda<UsuarioAplicacion>(SecurityOperations.Write, "StoredPassword",
                cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read,
                SecurityPermissionState.Deny);
            defaultRole.AddObjectPermission<ModelDifference>(SecurityOperations.ReadWriteAccess,
                "UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
            defaultRole.AddObjectPermission<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess,
                "Owner.UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create,
                SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create,
                SecurityPermissionState.Allow);
        }

        return defaultRole;
    }
}