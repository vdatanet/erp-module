using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
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
[XafDisplayName("Registro de Jornada")]
[ImageName("Time")]
[RuleCriteria("FechaFin >= FechaInicio", CustomMessageTemplate = "La fecha de fin debe ser posterior a la fecha de inicio")]
public class RegistroJornada(Session session) : EntidadBase(session)
{
    private Empleado _empleado;
    private DateTime _fechaInicio;
    private DateTime? _fechaFin;
    private Proyecto _proyecto;
    private ActividadProyecto _actividad;
    private string _notas;
    private TimeSpan _duracion;

    [RuleRequiredField]
    [Association("Empleado-RegistrosJornada")]
    [ModelDefault("AllowEdit", "False")]
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

    [Association("Proyecto-RegistrosJornada")]
    [ImmediatePostData]
    [XafDisplayName("Proyecto")]
    public Proyecto Proyecto
    {
        get => _proyecto;
        set => SetPropertyValue(nameof(Proyecto), ref _proyecto, value);
    }

    [Association("ActividadProyecto-RegistrosJornada")]
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
    [ModelDefault("DisplayFormat", "{0:hh\\:mm}")]
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
        Empleado = GetCurrentUser()?.Empleado;
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        RecalcularDuracion();
    }

    private void RecalcularDuracion()
    {
        Duracion = (FechaFin.HasValue && FechaFin.Value >= FechaInicio) 
            ? FechaFin.Value - FechaInicio 
            : TimeSpan.Zero;
    }

    private UsuarioAplicacion GetCurrentUser()
    {
        return Session.GetObjectByKey<UsuarioAplicacion>(
            Session.ServiceProvider.GetRequiredService<ISecurityStrategyBase>().UserId);
    }
}