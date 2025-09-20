using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
// using erp.Module.BusinessObjects.Security; // Ajusta el namespace de ApplicationUser si asocias por usuario

namespace erp.Module.BusinessObjects.TimeTracking;

public enum OvertimePolicy
{
    None = 0,
    AfterTarget = 1,          // Horas extra después de DailyTarget
    AfterEndPlusTolerance = 2 // Horas extra después de WorkdayEnd + ToleranceLateOut
}

[DefaultClassOptions]
[NavigationItem("Time Tracking")]
[ImageName("BO_Rules")]
[XafDisplayName("Workday Rule")]
public class WorkdayRule : BaseEntity
{
    public WorkdayRule(Session session) : base(session) { }

    private string _name;
    private TimeSpan _workdayStart = new(9, 0, 0);
    private TimeSpan _workdayEnd = new(18, 0, 0);
    private TimeSpan _dailyTarget = new(8, 0, 0);

    private TimeSpan _toleranceEarlyIn = TimeSpan.Zero;
    private TimeSpan _toleranceLateIn = TimeSpan.Zero;
    private TimeSpan _toleranceEarlyOut = TimeSpan.Zero;
    private TimeSpan _toleranceLateOut = TimeSpan.Zero;

    private OvertimePolicy _overtimePolicy = OvertimePolicy.AfterTarget;
    private bool _isDefault = true;

    [Size(128)]
    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    [XafDisplayName("Inicio Jornada")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan WorkdayStart
    {
        get => _workdayStart;
        set => SetPropertyValue(nameof(WorkdayStart), ref _workdayStart, value);
    }

    [XafDisplayName("Fin Jornada")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan WorkdayEnd
    {
        get => _workdayEnd;
        set => SetPropertyValue(nameof(WorkdayEnd), ref _workdayEnd, value);
    }

    [XafDisplayName("Objetivo Diario")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan DailyTarget
    {
        get => _dailyTarget;
        set => SetPropertyValue(nameof(DailyTarget), ref _dailyTarget, value);
    }

    [XafDisplayName("Tol. Entrada Temprana")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan ToleranceEarlyIn
    {
        get => _toleranceEarlyIn;
        set => SetPropertyValue(nameof(ToleranceEarlyIn), ref _toleranceEarlyIn, value);
    }

    [XafDisplayName("Tol. Entrada Tardía")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan ToleranceLateIn
    {
        get => _toleranceLateIn;
        set => SetPropertyValue(nameof(ToleranceLateIn), ref _toleranceLateIn, value);
    }

    [XafDisplayName("Tol. Salida Temprana")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan ToleranceEarlyOut
    {
        get => _toleranceEarlyOut;
        set => SetPropertyValue(nameof(ToleranceEarlyOut), ref _toleranceEarlyOut, value);
    }

    [XafDisplayName("Tol. Salida Tardía")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan ToleranceLateOut
    {
        get => _toleranceLateOut;
        set => SetPropertyValue(nameof(ToleranceLateOut), ref _toleranceLateOut, value);
    }

    [XafDisplayName("Política Horas Extra")]
    public OvertimePolicy OvertimePolicy
    {
        get => _overtimePolicy;
        set => SetPropertyValue(nameof(OvertimePolicy), ref _overtimePolicy, value);
    }

    [XafDisplayName("Predeterminada")]
    public bool IsDefault
    {
        get => _isDefault;
        set => SetPropertyValue(nameof(IsDefault), ref _isDefault, value);
    }

    // Futuro: asociaciones por usuario, rol, departamento.
}