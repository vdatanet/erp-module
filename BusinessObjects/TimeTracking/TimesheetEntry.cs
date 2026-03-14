using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.BusinessObjects.TimeTracking;

[DefaultClassOptions]
[NavigationItem("Time Tracking")]
[ImageName("Time")]
public class TimesheetEntry(Session session) : BaseEntity(session)
{
    private Employee _empleado;
    private DateTime _fechaInicio;
    private DateTime? _fechaFin;
    private Project _proyecto;
    private ProjectActivity _actividad;
    private string _notas;
    private TimeSpan _duracion;
    private DailyTimesheet _parteDiario;
    private DailyTimesheet _parteDiarioAnterior;
    private WorkdayRule _reglaJornadaAnteriorEmpleado;

    [Association("Employee-TimesheetEntries")]
    [RuleRequiredField]
    [ModelDefault("AllowEdit", "False")]
    public Employee Empleado
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

    [Association("Project-TimesheetEntries")]
    [ImmediatePostData]
    public Project Proyecto
    {
        get => _proyecto;
        set => SetPropertyValue(nameof(Proyecto), ref _proyecto, value);
    }

    [Association("ProjectActivity-TimesheetEntries")]
    [DataSourceProperty("Proyecto.Activities")]
    public ProjectActivity Actividad
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

    [Association("DailyTimesheet-Entries")]
    public DailyTimesheet ParteDiario
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

    private ApplicationUser GetCurrentUser()
    {
        return Session.GetObjectByKey<ApplicationUser>(
            Session.ServiceProvider.GetRequiredService<ISecurityStrategyBase>().UserId);
    }
}