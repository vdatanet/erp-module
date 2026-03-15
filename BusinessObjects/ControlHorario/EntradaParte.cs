using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Proyectos;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.BusinessObjects.ControlHorario;

[DefaultClassOptions]
[NavigationItem("Control Horario")]
[ImageName("Time")]
public class EntradaParte(Session session) : EntidadBase(session)
{
    private Empleado _empleado;
    private DateTime _fechaInicio;
    private DateTime? _fechaFin;
    private Proyecto _proyecto;
    private ActividadProyecto _actividad;
    private string _notas;
    private TimeSpan _duracion;
    
    [Association("Empleado-EntradasParte")]
    [RuleRequiredField]
    //[ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Empleado")]
    public Empleado Empleado
    {
        get => _empleado;
        set => SetPropertyValue(nameof(Empleado), ref _empleado, value);
    }

    [RuleRequiredField]
    [ImmediatePostData]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.EditMask), "g")]
    [XafDisplayName("Fecha Inicio")]
    public DateTime FechaInicio
    {
        get => _fechaInicio;
        set
        {
            if (SetPropertyValue(nameof(FechaInicio), ref _fechaInicio, value)) RecalcularDuracion();
        }
    }

    [ImmediatePostData]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.EditMask), "g")]
    [XafDisplayName("Fecha Fin")]
    public DateTime? FechaFin
    {
        get => _fechaFin;
        set
        {
            if (SetPropertyValue(nameof(FechaFin), ref _fechaFin, value)) RecalcularDuracion();
        }
    }

    [Association("Proyecto-EntradasParte")]
    [ImmediatePostData]
    [XafDisplayName("Proyecto")]
    public Proyecto Proyecto
    {
        get => _proyecto;
        set => SetPropertyValue(nameof(Proyecto), ref _proyecto, value);
    }

    [Association("ActividadProyecto-EntradasParte")]
    [DataSourceProperty("Proyecto.Actividades")]
    [XafDisplayName("Actividad")]
    public ActividadProyecto Actividad
    {
        get => _actividad;
        set => SetPropertyValue(nameof(Actividad), ref _actividad, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("RowCount", "3")]
    [XafDisplayName("Notas")]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Duración")]
    public TimeSpan Duracion
    {
        get => _duracion;
        set => SetPropertyValue(nameof(Duracion), ref _duracion, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        //SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(Empleado), GetCurrentUser());
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        RecalcularDuracion();
    }

    protected override void OnDeleting()
    {
        base.OnDeleting();
    }

    protected override void OnDeleted()
    {
        base.OnDeleted();
    }

    private void RecalcularDuracion()
    {
        if (FechaFin.HasValue && FechaFin.Value >= FechaInicio)
            Duracion = FechaFin.Value - FechaInicio;
        else
            Duracion = TimeSpan.Zero;
    }

    private UsuarioAplicacion GetCurrentUser()
    {
        return Session.GetObjectByKey<UsuarioAplicacion>(
            Session.ServiceProvider.GetRequiredService<ISecurityStrategyBase>().UserId);
    }
}