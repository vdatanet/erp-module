using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.Services.Tpv;
using Microsoft.Extensions.DependencyInjection;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Tpv;

[DefaultClassOptions]
[NavigationItem("Tpv")]
[XafDisplayName("TPV")]
[Persistent("Tpv")]
public class Tpv(Session session) : EntidadBase(session)
{
    private bool _activo;
    private string? _codigo;
    private string? _nombre;
    private string? _seriePorDefecto;
    private string? _ubicacion;
    private ZonaHoraria? _zonaHoraria;
    private DateTime? _ultimaConexion;

    [Size(100)]
    [RuleRequiredField("RuleRequiredField_Tpv_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre del TPV es obligatorio")]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(20)]
    [RuleRequiredField("RuleRequiredField_Tpv_Codigo", DefaultContexts.Save, CustomMessageTemplate = "El Código del TPV es obligatorio")]
    [RuleUniqueValue]
    [XafDisplayName("Código")]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [Size(10)]
    [XafDisplayName("Serie por defecto")]
    public string? SeriePorDefecto
    {
        get => _seriePorDefecto;
        set => SetPropertyValue(nameof(SeriePorDefecto), ref _seriePorDefecto, value);
    }

    [XafDisplayName("Activo")]
    public bool Activo
    {
        get => _activo;
        set => SetPropertyValue(nameof(Activo), ref _activo, value);
    }

    [Size(255)]
    [XafDisplayName("Ubicación")]
    public string? Ubicacion
    {
        get => _ubicacion;
        set => SetPropertyValue(nameof(Ubicacion), ref _ubicacion, value);
    }

    [XafDisplayName("Zona horaria")]
    public ZonaHoraria? ZonaHoraria
    {
        get => _zonaHoraria;
        set => SetPropertyValue(nameof(ZonaHoraria), ref _zonaHoraria, value);
    }

    [XafDisplayName("Última conexión")]
    public DateTime? UltimaConexion
    {
        get => _ultimaConexion;
        set => SetPropertyValue(nameof(UltimaConexion), ref _ultimaConexion, value);
    }

    public DateTime GetLocalTime()
    {
        var tz = ZonaHoraria?.GetTimeZoneInfo();
        if (tz != null)
        {
            return TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
        }

        return InformacionEmpresaHelper.GetLocalTime(Session);
    }

    [XafDisplayName("Facturas Simplificadas")]
    public XPCollection<FacturaSimplificada> FacturasSimplificadas => new XPCollection<FacturaSimplificada>(Session, CriteriaOperator.Parse("Tpv.Oid = ? AND TipoDocumento = ?", Oid, TipoDocumentoVenta.FacturaSimplificada));

    [Association("Tpv-DocumentosVenta")]
    [XafDisplayName("Documentos de Venta")]
    public XPCollection<DocumentoVenta> DocumentosVenta => GetCollection<DocumentoVenta>();

    [Association("Tpv-Sesiones")]
    [XafDisplayName("Sesiones")]
    public XPCollection<SesionTpv> Sesiones => GetCollection<SesionTpv>();

    [Association("RolesAutorizados-Tpvs")]
    [XafDisplayName("Roles autorizados")]
    public XPCollection<ApplicationRole> RolesAutorizados => GetCollection<ApplicationRole>();

    public void AbrirSesionAction(decimal importeApertura = 0)
    {
        var service = Session.ServiceProvider?.GetService<ISesionTpvService>();
        if (service != null)
        {
            service.AbrirSesion(this, Session.GetObjectByKey<ApplicationUser>(SecuritySystem.CurrentUserId), importeApertura);
        }
        else
        {
            var sesionAbierta = SesionActualAbierta;
            if (sesionAbierta != null) return;

            if (!Activo)
                throw new UserFriendlyException("No se puede abrir una sesión en un TPV inactivo.");

            var usuarioActual = Session.GetObjectByKey<ApplicationUser>(SecuritySystem.CurrentUserId);
            if (usuarioActual == null) return;

            var nuevaSesion = new SesionTpv(Session);
            nuevaSesion.AbrirSesion(this, usuarioActual, importeApertura);
        }
        
        Session.Reload(this);
        OnChanged(nameof(SesionActualAbierta));
        OnChanged(nameof(SesionAbierta));
    }

    public void ContinuarSesionAction()
    {
        var sesionAbierta = SesionActualAbierta;
        if (sesionAbierta == null) return;
        // En XAF, esta acción solo sirve para navegar o validar.
    }

    public void CerrarSesionAction(string? observaciones = null)
    {
        var sesionAbierta = SesionActualAbierta;
        if (sesionAbierta == null) return;

        var service = Session.ServiceProvider?.GetService<ISesionTpvService>();
        if (service != null)
        {
            service.CerrarSesion(sesionAbierta, null, observaciones);
        }
        else
        {
            sesionAbierta.CerrarSesion(null, observaciones);
        }
        
        Session.Reload(this);
        OnChanged(nameof(SesionActualAbierta));
        OnChanged(nameof(SesionAbierta));
    }

    public void ReabrirSesionAction()
    {
        var ultimaSesion = Sesiones.OrderByDescending(s => s.Apertura).FirstOrDefault();
        if (ultimaSesion == null) return;

        var service = Session.ServiceProvider?.GetService<ISesionTpvService>();
        if (service != null)
        {
            service.ReabrirSesion(ultimaSesion);
        }
        else
        {
            if (ultimaSesion.EstaAbierta) return;
            ultimaSesion.ReabrirSesion();
        }
        
        Session.Reload(this);
        OnChanged(nameof(SesionActualAbierta));
        OnChanged(nameof(SesionAbierta));
    }

    public SesionTpv? SesionActualAbierta => Sesiones.FirstOrDefault(s => s.Estado == EstadoSesionTpv.Abierta);

    [XafDisplayName("Sesión Abierta")]
    public bool SesionAbierta => SesionActualAbierta != null;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Activo = true;

        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        ZonaHoraria ??= companyInfo.ZonaHorariaPorDefecto;
        SeriePorDefecto ??= companyInfo.PrefijoFacturasSimplificadasPorDefecto;
    }
}