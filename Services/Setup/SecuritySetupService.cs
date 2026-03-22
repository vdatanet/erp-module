using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Alquileres;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Tesoreria;
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
using erp.Module.BusinessObjects.Suscripciones;
using erp.Module.BusinessObjects.Tpv;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.Helpers.Comun;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.Services.Setup;

public class SecuritySetupService(IObjectSpace objectSpace)
{
    private IObjectSpace? _os;
    private IObjectSpace OS => _os ??= GetWorkingObjectSpace();

    private IObjectSpace GetWorkingObjectSpace()
    {
        if (objectSpace is CompositeObjectSpace compositeOS)
        {
            var result = compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os.IsKnownType(typeof(PermissionPolicyRole)));
            if (result != null) return result;

            // Fallback to the first persistent Object Space if no specific match is found for the type
            var fallback = compositeOS.AdditionalObjectSpaces.FirstOrDefault();
            if (fallback != null) return fallback;
        }

        return objectSpace;
    }

    public void CreateRolesAndUsers(string? tenantName, bool onlyAdmin = false)
    {
        var adminRole = CreateAdminRole();
        // Crear roles de negocio si no es onlyAdmin
        if (!onlyAdmin)
        {
            CreateImprentaRole();
            CreateContactosRole();
            CreateVentasRole();
            CreateComprasRole();
            CreateProduccionRole();
            CreateTpvRole();
            CreateContabilidadRole();
            CreateAuxiliaresRole();
            CreateConfiguracionesRole();
            CreateControlHorarioRole();
            CreateCrmRole();
            CreateImpuestosRole();
            CreateProductosRole();
            CreateAlquileresRole();
            CreateSuscripcionesRole();
            CreateTesoreriaRole();
            CreateReportsRole();
        }
    }

    private PermissionPolicyRole CreateAdminRole()
    {
        var adminRole = OS.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administradores");
        if (adminRole == null)
        {
            adminRole = OS.CreateObject<PermissionPolicyRole>();
            adminRole.Name = "Administradores";
        }

        adminRole.IsAdministrative = true;
        return adminRole;
    }

    private PermissionPolicyRole CreateDefaultRole()
    {
        var defaultRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Predeterminado");
        if (defaultRole == null)
        {
            defaultRole = OS.CreateObject<PermissionPolicyRole>();
            defaultRole.Name = "Predeterminado";
        }

        defaultRole.AddObjectPermission<ApplicationUser>(SecurityOperations.Read,
            "Oid = CurrentUserId()", SecurityPermissionState.Allow);
        defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Seguridad/Items/MyDetails",
            SecurityPermissionState.Allow);
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
        var imprentaRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Imprenta");
        if (imprentaRole == null)
        {
            imprentaRole = OS.CreateObject<PermissionPolicyRole>();
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
        var contactosRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Contactos");
        if (contactosRole == null)
        {
            contactosRole = OS.CreateObject<PermissionPolicyRole>();
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
        var ventasRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Ventas");
        if (ventasRole == null)
        {
            ventasRole = OS.CreateObject<PermissionPolicyRole>();
            ventasRole.Name = "Ventas";
        }

        ventasRole.AddTypePermissionsRecursively<PedidoVenta>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        ventasRole.AddTypePermissionsRecursively<AlbaranVenta>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        ventasRole.AddTypePermissionsRecursively<OfertaVenta>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        ventasRole.AddTypePermissionsRecursively<FacturaVenta>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        ventasRole.AddTypePermissionsRecursively<CategoriaVenta>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        ventasRole.AddTypePermissionsRecursively<Comision>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        ventasRole.AddTypePermissionsRecursively<LiquidacionComision>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        ventasRole.AddNavigationPermission(@"Application/NavigationItems/Items/Ventas", SecurityPermissionState.Allow);

        return ventasRole;
    }

    private PermissionPolicyRole CreateComprasRole()
    {
        var comprasRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Compras");
        if (comprasRole == null)
        {
            comprasRole = OS.CreateObject<PermissionPolicyRole>();
            comprasRole.Name = "Compras";
        }

        comprasRole.AddTypePermissionsRecursively<PedidoCompra>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        comprasRole.AddTypePermissionsRecursively<AlbaranCompra>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        comprasRole.AddTypePermissionsRecursively<OfertaCompra>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        comprasRole.AddTypePermissionsRecursively<FacturaCompra>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        comprasRole.AddNavigationPermission(@"Application/NavigationItems/Items/Compras",
            SecurityPermissionState.Allow);

        return comprasRole;
    }

    private PermissionPolicyRole CreateProduccionRole()
    {
        var produccionRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Producción");
        if (produccionRole == null)
        {
            produccionRole = OS.CreateObject<PermissionPolicyRole>();
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
        var tpvRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Tpv");
        if (tpvRole == null)
        {
            tpvRole = OS.CreateObject<PermissionPolicyRole>();
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
        var contabilidadRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Contabilidad");
        if (contabilidadRole == null)
        {
            contabilidadRole = OS.CreateObject<PermissionPolicyRole>();
            contabilidadRole.Name = "Contabilidad";
        }

        contabilidadRole.AddTypePermissionsRecursively<CuentaContable>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contabilidadRole.AddTypePermissionsRecursively<Diario>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        contabilidadRole.AddNavigationPermission(@"Application/NavigationItems/Items/Contabilidad",
            SecurityPermissionState.Allow);

        return contabilidadRole;
    }

    private PermissionPolicyRole CreateAuxiliaresRole()
    {
        var auxiliaresRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Auxiliares");
        if (auxiliaresRole == null)
        {
            auxiliaresRole = OS.CreateObject<PermissionPolicyRole>();
            auxiliaresRole.Name = "Auxiliares";
        }

        auxiliaresRole.AddTypePermissionsRecursively<Adjunto>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Imagen>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Pais>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Poblacion>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Provincia>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Sector>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Tarea>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Nacionalidad>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        auxiliaresRole.AddTypePermissionsRecursively<Parentesco>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        auxiliaresRole.AddNavigationPermission(@"Application/NavigationItems/Items/Auxiliares",
            SecurityPermissionState.Allow);

        return auxiliaresRole;
    }

    private PermissionPolicyRole CreateConfiguracionesRole()
    {
        var configuracionesRole =
            OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Configuraciones");
        if (configuracionesRole == null)
        {
            configuracionesRole = OS.CreateObject<PermissionPolicyRole>();
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
            OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "ControlHorario");
        if (controlHorarioRole == null)
        {
            controlHorarioRole = OS.CreateObject<PermissionPolicyRole>();
            controlHorarioRole.Name = "ControlHorario";
        }

        controlHorarioRole.AddTypePermissionsRecursively<RegistroJornada>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        controlHorarioRole.AddNavigationPermission(@"Application/NavigationItems/Items/Control Horario",
            SecurityPermissionState.Allow);

        return controlHorarioRole;
    }

    private PermissionPolicyRole CreateCrmRole()
    {
        var crmRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Crm");
        if (crmRole == null)
        {
            crmRole = OS.CreateObject<PermissionPolicyRole>();
            crmRole.Name = "Crm";
        }

        crmRole.AddTypePermissionsRecursively<Campana>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        crmRole.AddTypePermissionsRecursively<EquipoVenta>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        crmRole.AddTypePermissionsRecursively<Fuente>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        crmRole.AddTypePermissionsRecursively<Lead>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        crmRole.AddTypePermissionsRecursively<Medio>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        crmRole.AddTypePermissionsRecursively<Oportunidad>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        crmRole.AddNavigationPermission(@"Application/NavigationItems/Items/Crm", SecurityPermissionState.Allow);

        return crmRole;
    }

    private PermissionPolicyRole CreateImpuestosRole()
    {
        var impuestosRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Impuestos");
        if (impuestosRole == null)
        {
            impuestosRole = OS.CreateObject<PermissionPolicyRole>();
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
        var productosRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Productos");
        if (productosRole == null)
        {
            productosRole = OS.CreateObject<PermissionPolicyRole>();
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
        var alquileresRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Alquileres");
        if (alquileresRole == null)
        {
            alquileresRole = OS.CreateObject<PermissionPolicyRole>();
            alquileresRole.Name = "Alquileres";
        }

        alquileresRole.AddTypePermissionsRecursively<DetalleTarifa>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        alquileresRole.AddTypePermissionsRecursively<Extra>(SecurityOperations.FullAccess,
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

    private PermissionPolicyRole CreateSuscripcionesRole()
    {
        var suscripcionesRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Suscripciones");
        if (suscripcionesRole == null)
        {
            suscripcionesRole = OS.CreateObject<PermissionPolicyRole>();
            suscripcionesRole.Name = "Suscripciones";
        }

        suscripcionesRole.AddTypePermissionsRecursively<Suscripcion>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        suscripcionesRole.AddTypePermissionsRecursively<TipoSuscripcion>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        suscripcionesRole.AddTypePermissionsRecursively<EstadoVigenciaPedido>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        suscripcionesRole.AddNavigationPermission(@"Application/NavigationItems/Items/Suscripciones",
            SecurityPermissionState.Allow);

        return suscripcionesRole;
    }

    private PermissionPolicyRole CreateTesoreriaRole()
    {
        var tesoreriaRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Tesorería");
        if (tesoreriaRole == null)
        {
            tesoreriaRole = OS.CreateObject<PermissionPolicyRole>();
            tesoreriaRole.Name = "Tesorería";
        }

        tesoreriaRole.AddTypePermissionsRecursively<Banco>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        tesoreriaRole.AddTypePermissionsRecursively<CondicionPago>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        tesoreriaRole.AddTypePermissionsRecursively<MedioPago>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        tesoreriaRole.AddTypePermissionsRecursively<MandatoSepa>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        tesoreriaRole.AddTypePermissionsRecursively<EfectoBase>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        tesoreriaRole.AddTypePermissionsRecursively<EfectoCobro>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        tesoreriaRole.AddTypePermissionsRecursively<EfectoPago>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        tesoreriaRole.AddTypePermissionsRecursively<EstadoEfecto>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        tesoreriaRole.AddNavigationPermission(@"Application/NavigationItems/Items/Tesorería",
            SecurityPermissionState.Allow);

        return tesoreriaRole;
    }

    private PermissionPolicyRole CreateReportsRole()
    {
        var reportsRole = OS.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Reports");
        if (reportsRole == null)
        {
            reportsRole = OS.CreateObject<PermissionPolicyRole>();
            reportsRole.Name = "Reports";
        }

        reportsRole.AddTypePermissionsRecursively<ReportDataV2>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        reportsRole.AddNavigationPermission(@"Application/NavigationItems/Items/Reports",
            SecurityPermissionState.Allow);

        return reportsRole;
    }

}