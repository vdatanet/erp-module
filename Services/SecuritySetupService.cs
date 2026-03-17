using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Alquileres;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Imprenta;
using erp.Module.BusinessObjects.Ventas;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.Services;

public class SecuritySetupService(IObjectSpace objectSpace)
{
    public void CreateRolesAndUsers(string? tenantName)
    {
        var adminRole = CreateAdminRole();
        var imprentaRole = CreateImprentaRole();
        var contactosRole = CreateContactosRole();
        var ventasRole = CreateVentasRole();
        var alquileresRole = CreateAlquileresRole();
        var reportsRole = CreateReportsRole();

        var userManager = objectSpace.ServiceProvider.GetRequiredService<UserManager>();

        if (tenantName != null)
        {
            var defaultRole = CreateDefaultRole();
            var imprentaRole_User = CreateImprentaRole();
            var contactosRole_User = CreateContactosRole();
            var ventasRole_User = CreateVentasRole();
            var alquileresRole_User = CreateAlquileresRole();
            var reportsRole_User = CreateReportsRole();

            var userName = $"User@{tenantName}";
            if (userManager.FindUserByName<ApplicationUser>(objectSpace, userName) == null)
            {
                var EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(objectSpace, userName, EmptyPassword, user =>
                {
                    user.Roles.Add(defaultRole);
                    user.Roles.Add(imprentaRole_User);
                    user.Roles.Add(contactosRole_User);
                    user.Roles.Add(ventasRole_User);
                    user.Roles.Add(alquileresRole_User);
                    user.Roles.Add(reportsRole_User);
                });
            }
        }

        var adminUserName = tenantName != null ? $"Admin@{tenantName}" : "Admin";
        if (userManager.FindUserByName<ApplicationUser>(objectSpace, adminUserName) == null)
        {
            var EmptyPassword = "";
            _ = userManager.CreateUser<ApplicationUser>(objectSpace, adminUserName, EmptyPassword,
                user => { user.Roles.Add(adminRole); });
        }
    }

    private PermissionPolicyRole CreateAdminRole()
    {
        var adminRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
        if (adminRole == null)
        {
            adminRole = objectSpace.CreateObject<PermissionPolicyRole>();
            adminRole.Name = "Administrators";
        }

        adminRole.IsAdministrative = true;
        return adminRole;
    }

    private PermissionPolicyRole CreateDefaultRole()
    {
        var defaultRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
        if (defaultRole == null)
        {
            defaultRole = objectSpace.CreateObject<PermissionPolicyRole>();
            defaultRole.Name = "Default";
        }

        defaultRole.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read,
            cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
        defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails",
            SecurityPermissionState.Allow);
        defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write,
            "ChangePasswordOnFirstLogon", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(),
            SecurityPermissionState.Allow);
        defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword",
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

        return defaultRole;
    }

    private PermissionPolicyRole CreateImprentaRole()
    {
        var imprentaRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Imprenta");
        if (imprentaRole == null)
        {
            imprentaRole = objectSpace.CreateObject<PermissionPolicyRole>();
            imprentaRole.Name = "Imprenta";
        }

        imprentaRole.AddTypePermissionsRecursively<TrabajoImpresion>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        imprentaRole.AddTypePermissionsRecursively<TrabajoImpresionHora>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        imprentaRole.AddTypePermissionsRecursively<TamanoPapel>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        imprentaRole.AddTypePermissionsRecursively<TrabajoImpresionMaterial>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        imprentaRole.AddTypePermissionsRecursively<TrabajoImpresionPapel>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        imprentaRole.AddTypePermissionsRecursively<TrabajoImpresionServicio>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        imprentaRole.AddNavigationPermission(@"Application/NavigationItems/Items/Imprenta",
            SecurityPermissionState.Allow);

        return imprentaRole;
    }

    private PermissionPolicyRole CreateContactosRole()
    {
        var contactosRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Contactos");
        if (contactosRole == null)
        {
            contactosRole = objectSpace.CreateObject<PermissionPolicyRole>();
            contactosRole.Name = "Contactos";
        }

        contactosRole.AddTypePermissionsRecursively<Cliente>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contactosRole.AddTypePermissionsRecursively<Contacto>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contactosRole.AddTypePermissionsRecursively<Domicilio>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contactosRole.AddTypePermissionsRecursively<Empleado>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contactosRole.AddTypePermissionsRecursively<Proveedor>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contactosRole.AddTypePermissionsRecursively<Tercero>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        contactosRole.AddNavigationPermission(@"Application/NavigationItems/Items/Contactos",
            SecurityPermissionState.Allow);

        return contactosRole;
    }

    private PermissionPolicyRole CreateVentasRole()
    {
        var ventasRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Ventas");
        if (ventasRole == null)
        {
            ventasRole = objectSpace.CreateObject<PermissionPolicyRole>();
            ventasRole.Name = "Ventas";
        }

        ventasRole.AddTypePermissionsRecursively<Pedido>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        ventasRole.AddTypePermissionsRecursively<Presupuesto>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        ventasRole.AddNavigationPermission(@"Application/NavigationItems/Items/Ventas", SecurityPermissionState.Allow);

        return ventasRole;
    }

    private PermissionPolicyRole CreateAlquileresRole()
    {
        var alquileresRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Alquileres");
        if (alquileresRole == null)
        {
            alquileresRole = objectSpace.CreateObject<PermissionPolicyRole>();
            alquileresRole.Name = "Alquileres";
        }

        alquileresRole.AddTypePermissionsRecursively<DetalleTarifa>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        alquileresRole.AddTypePermissionsRecursively<Extra>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        alquileresRole.AddTypePermissionsRecursively<Pago>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        alquileresRole.AddTypePermissionsRecursively<RecursoAlquilable>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        alquileresRole.AddTypePermissionsRecursively<Reserva>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        alquileresRole.AddTypePermissionsRecursively<Simulacion>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        alquileresRole.AddTypePermissionsRecursively<Tarifa>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        alquileresRole.AddTypePermissionsRecursively<Ubicacion>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        alquileresRole.AddTypePermissionsRecursively<Viajero>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        alquileresRole.AddNavigationPermission(@"Application/NavigationItems/Items/Alquileres",
            SecurityPermissionState.Allow);

        return alquileresRole;
    }

    private PermissionPolicyRole CreateReportsRole()
    {
        var reportsRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Reports");
        if (reportsRole == null)
        {
            reportsRole = objectSpace.CreateObject<PermissionPolicyRole>();
            reportsRole.Name = "Reports";
        }

        reportsRole.AddTypePermissionsRecursively<ReportDataV2>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        reportsRole.AddNavigationPermission(@"Application/NavigationItems/Items/Reports",
            SecurityPermissionState.Allow);

        return reportsRole;
    }
}