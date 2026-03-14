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
    private ParteDiario _parteDiario;
    private ParteDiario _parteDiarioAnterior;
    private ReglaJornada _reglaJornadaAnteriorEmpleado;

    [Association("Empleado-EntradasParte")]
    [RuleRequiredField]
    [ModelDefault("AllowEdit", "False")]
    public Empleado Empleado
    {
        get => _empleado;
        set => SetPropertyValue(nameof(Empleado), ref _empleado, value);
    }

    [RuleRequiredField]
    [ImmediatePostData]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.EditMask), "g")]

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
    public Proyecto Proyecto
    {
        get => _proyecto;
        set => SetPropertyValue(nameof(Proyecto), ref _proyecto, value);
    }

    [Association("ActividadProyecto-EntradasParte")]
    [DataSourceProperty("Proyecto.Actividades")]
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

    [Association("ParteDiario-Entradas")]
    public ParteDiario ParteDiario
    {
        get => _parteDiario;
        set => SetPropertyValue(nameof(ParteDiario), ref _parteDiario, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(Empleado), GetCurrentUser());
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        RecalcularDuracion();
    }

    protected override void OnDeleting()
    {
        if (ParteDiario is not null) _parteDiarioAnterior = ParteDiario;
        _reglaJornadaAnteriorEmpleado = Empleado?.ReglaJornadaLaboral ?? _reglaJornadaAnteriorEmpleado;
        base.OnDeleting();
    }

    protected override void OnDeleted()
    {
        base.OnDeleted();

        if (_parteDiarioAnterior is { } ts && _reglaJornadaAnteriorEmpleado is { } regla)
            ts.Recalcular(regla);

        _parteDiarioAnterior = null;
        _reglaJornadaAnteriorEmpleado = null;
    }

    private void RecalcularDuracion()
    {
        if (FechaFin.HasValue && FechaFin.Value >= FechaInicio)
            Duracion = FechaFin.Value - FechaInicio;
        else
            Duracion = TimeSpan.Zero;

        if (ParteDiario is { } ts && Empleado?.ReglaJornadaLaboral is { } regla)
            ts.Recalcular(regla);
    }

    private UsuarioAplicacion GetCurrentUser()
    {
        return Session.GetObjectByKey<UsuarioAplicacion>(
            Session.ServiceProvider.GetRequiredService<ISecurityStrategyBase>().UserId);
    }
}