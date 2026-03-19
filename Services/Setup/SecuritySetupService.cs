using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Alquileres;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Compras;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.ControlHorario;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Imprenta;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.BusinessObjects.Produccion;
using erp.Module.BusinessObjects.Productos;
using erp.Module.BusinessObjects.Tpv;
using erp.Module.BusinessObjects.Ventas;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.Services.Setup;

public class SecuritySetupService(IObjectSpace objectSpace)
{
    public void CreateRolesAndUsers(string? tenantName)
    {
        var adminRole = CreateAdminRole();
        var imprentaRole = CreateImprentaRole();
        var contactosRole = CreateContactosRole();
        var ventasRole = CreateVentasRole();
        var comprasRole = CreateComprasRole();
        var produccionRole = CreateProduccionRole();
        var tpvRole = CreateTpvRole();
        var contabilidadRole = CreateContabilidadRole();
        var auxiliaresRole = CreateAuxiliaresRole();
        var configuracionesRole = CreateConfiguracionesRole();
        var controlHorarioRole = CreateControlHorarioRole();
        var crmRole = CreateCrmRole();
        var impuestosRole = CreateImpuestosRole();
        var productosRole = CreateProductosRole();
        var alquileresRole = CreateAlquileresRole();
        var reportsRole = CreateReportsRole();

        var userManager = objectSpace.ServiceProvider?.GetService<UserManager>();

        if (userManager == null)
        {
            return;
        }

        if (tenantName != null)
        {
            var defaultRole = CreateDefaultRole();
            var imprentaRole_User = CreateImprentaRole();
            var contactosRole_User = CreateContactosRole();
            var ventasRole_User = CreateVentasRole();
            var comprasRole_User = CreateComprasRole();
            var produccionRole_User = CreateProduccionRole();
            var tpvRole_User = CreateTpvRole();
            var contabilidadRole_User = CreateContabilidadRole();
            var auxiliaresRole_User = CreateAuxiliaresRole();
            var configuracionesRole_User = CreateConfiguracionesRole();
            var controlHorarioRole_User = CreateControlHorarioRole();
            var crmRole_User = CreateCrmRole();
            var impuestosRole_User = CreateImpuestosRole();
            var productosRole_User = CreateProductosRole();
            var alquileresRole_User = CreateAlquileresRole();
            var reportsRole_User = CreateReportsRole();

            var userName = $"User@{tenantName}";
            if (userManager.FindUserByName<ApplicationUser>(objectSpace, userName) == null)
            {
                var EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(objectSpace, userName, EmptyPassword, user =>
                {
                    user.ChangePasswordOnFirstLogon = true;
                    user.Roles.Add(defaultRole);
                    user.Roles.Add(imprentaRole_User);
                    user.Roles.Add(contactosRole_User);
                    user.Roles.Add(ventasRole_User);
                    user.Roles.Add(comprasRole_User);
                    user.Roles.Add(produccionRole_User);
                    user.Roles.Add(tpvRole_User);
                    user.Roles.Add(contabilidadRole_User);
                    user.Roles.Add(auxiliaresRole_User);
                    user.Roles.Add(configuracionesRole_User);
                    user.Roles.Add(controlHorarioRole_User);
                    user.Roles.Add(crmRole_User);
                    user.Roles.Add(impuestosRole_User);
                    user.Roles.Add(productosRole_User);
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
                user =>
                {
                    user.ChangePasswordOnFirstLogon = true;
                    user.Roles.Add(adminRole);
                });
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

        defaultRole.AddObjectPermission<ApplicationUser>(SecurityOperations.Read,
            "Oid = CurrentUserId()", SecurityPermissionState.Allow);
        defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails",
            SecurityPermissionState.Allow);
        defaultRole.AddMemberPermission<ApplicationUser>(SecurityOperations.Write,
            "ChangePasswordOnFirstLogon", "Oid = CurrentUserId()",
            SecurityPermissionState.Allow);
        defaultRole.AddMemberPermission<ApplicationUser>(SecurityOperations.Write, "StoredPassword",
            "Oid = CurrentUserId()", SecurityPermissionState.Allow);
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
        ventasRole.AddTypePermissionsRecursively<Albaran>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        ventasRole.AddTypePermissionsRecursively<Presupuesto>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        ventasRole.AddTypePermissionsRecursively<Factura>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        ventasRole.AddNavigationPermission(@"Application/NavigationItems/Items/Ventas", SecurityPermissionState.Allow);

        return ventasRole;
    }

    private PermissionPolicyRole CreateComprasRole()
    {
        var comprasRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Compras");
        if (comprasRole == null)
        {
            comprasRole = objectSpace.CreateObject<PermissionPolicyRole>();
            comprasRole.Name = "Compras";
        }

        comprasRole.AddTypePermissionsRecursively<PedidoCompra>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        comprasRole.AddTypePermissionsRecursively<AlbaranCompra>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        comprasRole.AddTypePermissionsRecursively<PresupuestoCompra>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        comprasRole.AddTypePermissionsRecursively<FacturaCompra>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        comprasRole.AddNavigationPermission(@"Application/NavigationItems/Items/Compras",
            SecurityPermissionState.Allow);

        return comprasRole;
    }

    private PermissionPolicyRole CreateProduccionRole()
    {
        var produccionRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Producción");
        if (produccionRole == null)
        {
            produccionRole = objectSpace.CreateObject<PermissionPolicyRole>();
            produccionRole.Name = "Producción";
        }

        produccionRole.AddTypePermissionsRecursively<Parte>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        produccionRole.AddNavigationPermission(@"Application/NavigationItems/Items/Producción",
            SecurityPermissionState.Allow);

        return produccionRole;
    }

    private PermissionPolicyRole CreateTpvRole()
    {
        var tpvRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Tpv");
        if (tpvRole == null)
        {
            tpvRole = objectSpace.CreateObject<PermissionPolicyRole>();
            tpvRole.Name = "Tpv";
        }

        tpvRole.AddTypePermissionsRecursively<Tpv>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        tpvRole.AddTypePermissionsRecursively<FacturaSimplificada>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        tpvRole.AddTypePermissionsRecursively<SesionTpv>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        tpvRole.AddNavigationPermission(@"Application/NavigationItems/Items/Tpv", SecurityPermissionState.Allow);

        return tpvRole;
    }

    private PermissionPolicyRole CreateContabilidadRole()
    {
        var contabilidadRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Contabilidad");
        if (contabilidadRole == null)
        {
            contabilidadRole = objectSpace.CreateObject<PermissionPolicyRole>();
            contabilidadRole.Name = "Contabilidad";
        }

        contabilidadRole.AddTypePermissionsRecursively<Cuenta>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contabilidadRole.AddTypePermissionsRecursively<Diario>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        contabilidadRole.AddNavigationPermission(@"Application/NavigationItems/Items/Contabilidad",
            SecurityPermissionState.Allow);

        return contabilidadRole;
    }

    private PermissionPolicyRole CreateAuxiliaresRole()
    {
        var auxiliaresRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Auxiliares");
        if (auxiliaresRole == null)
        {
            auxiliaresRole = objectSpace.CreateObject<PermissionPolicyRole>();
            auxiliaresRole.Name = "Auxiliares";
        }

        auxiliaresRole.AddTypePermissionsRecursively<Adjunto>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Banco>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<CondicionPago>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Imagen>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<MedioPago>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Nacionalidad>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Pais>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Parentesco>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Poblacion>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Provincia>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Sector>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Tarea>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        auxiliaresRole.AddNavigationPermission(@"Application/NavigationItems/Items/Auxiliares",
            SecurityPermissionState.Allow);

        return auxiliaresRole;
    }

    private PermissionPolicyRole CreateConfiguracionesRole()
    {
        var configuracionesRole =
            objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Configuraciones");
        if (configuracionesRole == null)
        {
            configuracionesRole = objectSpace.CreateObject<PermissionPolicyRole>();
            configuracionesRole.Name = "Configuraciones";
        }

        configuracionesRole.AddTypePermissionsRecursively<InformacionEmpresa>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        configuracionesRole.AddTypePermissionsRecursively<Secuencia>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        configuracionesRole.AddNavigationPermission(@"Application/NavigationItems/Items/Configuraciones",
            SecurityPermissionState.Allow);

        return configuracionesRole;
    }

    private PermissionPolicyRole CreateControlHorarioRole()
    {
        var controlHorarioRole =
            objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "ControlHorario");
        if (controlHorarioRole == null)
        {
            controlHorarioRole = objectSpace.CreateObject<PermissionPolicyRole>();
            controlHorarioRole.Name = "ControlHorario";
        }

        controlHorarioRole.AddTypePermissionsRecursively<RegistroJornada>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        controlHorarioRole.AddNavigationPermission(@"Application/NavigationItems/Items/ControlHorario",
            SecurityPermissionState.Allow);

        return controlHorarioRole;
    }

    private PermissionPolicyRole CreateCrmRole()
    {
        var crmRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Crm");
        if (crmRole == null)
        {
            crmRole = objectSpace.CreateObject<PermissionPolicyRole>();
            crmRole.Name = "Crm";
        }

        crmRole.AddTypePermissionsRecursively<Campana>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        crmRole.AddTypePermissionsRecursively<Fuente>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        crmRole.AddTypePermissionsRecursively<Medio>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        crmRole.AddTypePermissionsRecursively<Oportunidad>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        crmRole.AddNavigationPermission(@"Application/NavigationItems/Items/Crm", SecurityPermissionState.Allow);

        return crmRole;
    }

    private PermissionPolicyRole CreateImpuestosRole()
    {
        var impuestosRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Impuestos");
        if (impuestosRole == null)
        {
            impuestosRole = objectSpace.CreateObject<PermissionPolicyRole>();
            impuestosRole.Name = "Impuestos";
        }

        impuestosRole.AddTypePermissionsRecursively<MapeoCuenta>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        impuestosRole.AddTypePermissionsRecursively<MapeoImpuesto>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        impuestosRole.AddTypePermissionsRecursively<PosicionFiscal>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        impuestosRole.AddTypePermissionsRecursively<TipoImpuesto>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        impuestosRole.AddNavigationPermission(@"Application/NavigationItems/Items/Impuestos",
            SecurityPermissionState.Allow);

        return impuestosRole;
    }

    private PermissionPolicyRole CreateProductosRole()
    {
        var productosRole = objectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Productos");
        if (productosRole == null)
        {
            productosRole = objectSpace.CreateObject<PermissionPolicyRole>();
            productosRole.Name = "Productos";
        }

        productosRole.AddTypePermissionsRecursively<Categoria>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        productosRole.AddTypePermissionsRecursively<PrecioPorCantidad>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        productosRole.AddTypePermissionsRecursively<Producto>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        productosRole.AddNavigationPermission(@"Application/NavigationItems/Items/Productos",
            SecurityPermissionState.Allow);

        return productosRole;
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
        alquileresRole.AddTypePermissionsRecursively<BusinessObjects.Alquileres.Reserva>(SecurityOperations.FullAccess,
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