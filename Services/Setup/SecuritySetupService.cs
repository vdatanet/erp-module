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
using erp.Module.BusinessObjects.Inventario;
using erp.Module.BusinessObjects.Logistica;
using erp.Module.BusinessObjects.Servicios.PartesTrabajo;
using erp.Module.BusinessObjects.Servicios.Mantenimientos;
using erp.Module.BusinessObjects.Servicios.TrabajoDeCampo;
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
            return compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os.IsKnownType(typeof(PermissionPolicyRole))) ?? objectSpace;
        }

        return objectSpace;
    }

    public void CreateRolesAndUsers(string? tenantName, bool onlyAdmin = false)
    {
        if (!OS.IsKnownType(typeof(ApplicationRole))) return;

        var adminRole = CreateAdminRole();

#if DEBUG
        if (!string.IsNullOrEmpty(tenantName))
        {
            var adminUserName = $"admin@{tenantName}.com";
            var existingUser = OS.FirstOrDefault<ApplicationUser>(u => u.UserName == adminUserName);
            if (existingUser == null)
            {
                var adminUser = OS.CreateObject<ApplicationUser>();
                adminUser.UserName = adminUserName;
                adminUser.SetPassword("");
                adminUser.ChangePasswordOnFirstLogon = true;
                adminUser.Roles.Add(adminRole);
                
                if (adminUser is ISecurityUserWithLoginInfo userWithLoginInfo)
                {
                    userWithLoginInfo.CreateUserLoginInfo(SecurityDefaults.PasswordAuthentication, adminUserName);
                }
            }
        }
#endif

        // Crear roles de negocio si no es onlyAdmin
        if (!onlyAdmin)
        {
            CreateImprentaRole();
            CreateContactosRole();
            CreateVentasRole();
            CreateComprasRole();
            CreateServiciosRole();
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
            CreateInventarioRole();
            CreateLogisticaRole();
            CreateReportsRole();
        }
    }

    private ApplicationRole CreateAdminRole()
    {
        var adminRole = OS.FirstOrDefault<ApplicationRole>(r => r.Name == "Administradores");
        if (adminRole == null)
        {
            adminRole = OS.CreateObject<ApplicationRole>();
            adminRole.Name = "Administradores";
        }

        adminRole.IsAdministrative = true;
        return adminRole;
    }

    private ApplicationRole CreateDefaultRole()
    {
        var defaultRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Predeterminado");
        if (defaultRole == null)
        {
            defaultRole = OS.CreateObject<ApplicationRole>();
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
        defaultRole.AddTypePermissionsRecursively<ApplicationRole>(SecurityOperations.Read,
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

    private ApplicationRole CreateImprentaRole()
    {
        var imprentaRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Imprenta");
        if (imprentaRole == null)
        {
            imprentaRole = OS.CreateObject<ApplicationRole>();
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

    private ApplicationRole CreateContactosRole()
    {
        var contactosRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Contactos");
        if (contactosRole == null)
        {
            contactosRole = OS.CreateObject<ApplicationRole>();
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
        contactosRole.AddTypePermissionsRecursively<Acreedor>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contactosRole.AddTypePermissionsRecursively<Tercero>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        contactosRole.AddNavigationPermission(@"Application/NavigationItems/Items/Contactos",
            SecurityPermissionState.Allow);

        return contactosRole;
    }

    private ApplicationRole CreateVentasRole()
    {
        var ventasRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Ventas");
        if (ventasRole == null)
        {
            ventasRole = OS.CreateObject<ApplicationRole>();
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
        ventasRole.AddTypePermissionsRecursively<GrupoMaestro>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        ventasRole.AddNavigationPermission(@"Application/NavigationItems/Items/Ventas", SecurityPermissionState.Allow);

        return ventasRole;
    }

    private ApplicationRole CreateComprasRole()
    {
        var comprasRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Compras");
        if (comprasRole == null)
        {
            comprasRole = OS.CreateObject<ApplicationRole>();
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

    private ApplicationRole CreateServiciosRole()
    {
        var serviciosRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Servicios");
        if (serviciosRole == null)
        {
            serviciosRole = OS.CreateObject<ApplicationRole>();
            serviciosRole.Name = "Servicios";
        }

        // Partes de Trabajo
        serviciosRole.AddTypePermissionsRecursively<ParteTrabajo>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        serviciosRole.AddTypePermissionsRecursively<ParteTrabajoMaterial>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        serviciosRole.AddTypePermissionsRecursively<ParteTrabajoTiempo>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        // Mantenimientos
        serviciosRole.AddTypePermissionsRecursively<ActivoMantenimiento>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        serviciosRole.AddTypePermissionsRecursively<ContratoMantenimiento>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        serviciosRole.AddTypePermissionsRecursively<IncidenciaMantenimiento>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        serviciosRole.AddTypePermissionsRecursively<PlanificacionMantenimiento>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        serviciosRole.AddTypePermissionsRecursively<TareaMantenimiento>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        // Trabajo de Campo
        serviciosRole.AddTypePermissionsRecursively<PedidoTrabajoDeCampo>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        serviciosRole.AddTypePermissionsRecursively<PeriodicidadTrabajoDeCampo>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        serviciosRole.AddTypePermissionsRecursively<ServicioTrabajoDeCampo>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        serviciosRole.AddTypePermissionsRecursively<SolicitudTrabajoDeCampo>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        serviciosRole.AddTypePermissionsRecursively<TareaTrabajoDeCampo>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        serviciosRole.AddTypePermissionsRecursively<TipoServicioTrabajoDeCampo>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        serviciosRole.AddNavigationPermission(@"Application/NavigationItems/Items/Servicios",
            SecurityPermissionState.Allow);

        return serviciosRole;
    }

    private ApplicationRole CreateTpvRole()
    {
        var tpvRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Tpv");
        if (tpvRole == null)
        {
            tpvRole = OS.CreateObject<ApplicationRole>();
            tpvRole.Name = "Tpv";
        }

        tpvRole.AddTypePermissionsRecursively<erp.Module.BusinessObjects.Tpv.Tpv>(SecurityOperations.FullAccess, SecurityPermissionState.Allow);
        tpvRole.AddTypePermissionsRecursively<FacturaSimplificada>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        tpvRole.AddTypePermissionsRecursively<SesionTpv>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        tpvRole.AddTypePermissionsRecursively<SesionTpvEvento>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        tpvRole.AddTypePermissionsRecursively<MovimientoCajaTpv>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        tpvRole.AddNavigationPermission(@"Application/NavigationItems/Items/Tpv", SecurityPermissionState.Allow);

        return tpvRole;
    }

    private ApplicationRole CreateContabilidadRole()
    {
        var contabilidadRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Contabilidad");
        if (contabilidadRole == null)
        {
            contabilidadRole = OS.CreateObject<ApplicationRole>();
            contabilidadRole.Name = "Contabilidad";
        }

        contabilidadRole.AddTypePermissionsRecursively<CuentaContable>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contabilidadRole.AddTypePermissionsRecursively<Asiento>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contabilidadRole.AddTypePermissionsRecursively<Apunte>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contabilidadRole.AddTypePermissionsRecursively<Diario>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contabilidadRole.AddTypePermissionsRecursively<Ejercicio>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        contabilidadRole.AddTypePermissionsRecursively<PeriodoBloqueado>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        contabilidadRole.AddNavigationPermission(@"Application/NavigationItems/Items/Contabilidad",
            SecurityPermissionState.Allow);

        return contabilidadRole;
    }

    private ApplicationRole CreateAuxiliaresRole()
    {
        var auxiliaresRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Auxiliares");
        if (auxiliaresRole == null)
        {
            auxiliaresRole = OS.CreateObject<ApplicationRole>();
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

    private ApplicationRole CreateConfiguracionesRole()
    {
        var configuracionesRole =
            OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Configuraciones");
        if (configuracionesRole == null)
        {
            configuracionesRole = OS.CreateObject<ApplicationRole>();
            configuracionesRole.Name = "Configuraciones";
        }

        configuracionesRole.AddTypePermissionsRecursively<InformacionEmpresa>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        configuracionesRole.AddTypePermissionsRecursively<Secuencia>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        configuracionesRole.AddTypePermissionsRecursively<ZonaHoraria>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        configuracionesRole.AddNavigationPermission(@"Application/NavigationItems/Items/Configuraciones",
            SecurityPermissionState.Allow);

        return configuracionesRole;
    }

    private ApplicationRole CreateControlHorarioRole()
    {
        var controlHorarioRole =
            OS.FirstOrDefault<ApplicationRole>(role => role.Name == "ControlHorario");
        if (controlHorarioRole == null)
        {
            controlHorarioRole = OS.CreateObject<ApplicationRole>();
            controlHorarioRole.Name = "ControlHorario";
        }

        controlHorarioRole.AddTypePermissionsRecursively<RegistroJornada>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        controlHorarioRole.AddNavigationPermission(@"Application/NavigationItems/Items/Control Horario",
            SecurityPermissionState.Allow);

        return controlHorarioRole;
    }

    private ApplicationRole CreateCrmRole()
    {
        var crmRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Crm");
        if (crmRole == null)
        {
            crmRole = OS.CreateObject<ApplicationRole>();
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

    private ApplicationRole CreateImpuestosRole()
    {
        var impuestosRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Impuestos");
        if (impuestosRole == null)
        {
            impuestosRole = OS.CreateObject<ApplicationRole>();
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

    private ApplicationRole CreateProductosRole()
    {
        var productosRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Productos");
        if (productosRole == null)
        {
            productosRole = OS.CreateObject<ApplicationRole>();
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

    private ApplicationRole CreateAlquileresRole()
    {
        var alquileresRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Alquileres");
        if (alquileresRole == null)
        {
            alquileresRole = OS.CreateObject<ApplicationRole>();
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

    private ApplicationRole CreateSuscripcionesRole()
    {
        var suscripcionesRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Suscripciones");
        if (suscripcionesRole == null)
        {
            suscripcionesRole = OS.CreateObject<ApplicationRole>();
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

    private ApplicationRole CreateTesoreriaRole()
    {
        var tesoreriaRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Tesorería");
        if (tesoreriaRole == null)
        {
            tesoreriaRole = OS.CreateObject<ApplicationRole>();
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

    private ApplicationRole CreateInventarioRole()
    {
        var inventarioRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Inventario");
        if (inventarioRole == null)
        {
            inventarioRole = OS.CreateObject<ApplicationRole>();
            inventarioRole.Name = "Inventario";
        }

        inventarioRole.AddTypePermissionsRecursively<Almacen>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        inventarioRole.AddTypePermissionsRecursively<MovimientoAlmacen>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        inventarioRole.AddTypePermissionsRecursively<MovimientoAlmacenLinea>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        inventarioRole.AddTypePermissionsRecursively<StockActual>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        inventarioRole.AddNavigationPermission(@"Application/NavigationItems/Items/Inventario",
            SecurityPermissionState.Allow);

        return inventarioRole;
    }

    private ApplicationRole CreateLogisticaRole()
    {
        var logisticaRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Logística");
        if (logisticaRole == null)
        {
            logisticaRole = OS.CreateObject<ApplicationRole>();
            logisticaRole.Name = "Logística";
        }

        logisticaRole.AddTypePermissionsRecursively<Transportista>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);
        logisticaRole.AddTypePermissionsRecursively<MetodoEntrega>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        logisticaRole.AddNavigationPermission(@"Application/NavigationItems/Items/Logística",
            SecurityPermissionState.Allow);

        return logisticaRole;
    }

    private ApplicationRole CreateReportsRole()
    {
        var reportsRole = OS.FirstOrDefault<ApplicationRole>(role => role.Name == "Reports");
        if (reportsRole == null)
        {
            reportsRole = OS.CreateObject<ApplicationRole>();
            reportsRole.Name = "Reports";
        }

        reportsRole.AddTypePermissionsRecursively<ReportDataV2>(SecurityOperations.FullAccess,
            SecurityPermissionState.Allow);

        reportsRole.AddNavigationPermission(@"Application/NavigationItems/Items/Reports",
            SecurityPermissionState.Allow);

        return reportsRole;
    }

}