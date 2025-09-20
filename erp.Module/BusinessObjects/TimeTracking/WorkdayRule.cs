using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.TimeTracking;

[DefaultClassOptions]
[NavigationItem("Time Tracking")]
[ImageName("BO_Rules")]
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
    
    [Size(128)]
    [RuleRequiredField]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }
    
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan WorkdayStart
    {
        get => _workdayStart;
        set => SetPropertyValue(nameof(WorkdayStart), ref _workdayStart, value);
    }
    
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan WorkdayEnd
    {
        get => _workdayEnd;
        set => SetPropertyValue(nameof(WorkdayEnd), ref _workdayEnd, value);
    }
    
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan DailyTarget
    {
        get => _dailyTarget;
        set => SetPropertyValue(nameof(DailyTarget), ref _dailyTarget, value);
    }
    
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan ToleranceEarlyIn
    {
        get => _toleranceEarlyIn;
        set => SetPropertyValue(nameof(ToleranceEarlyIn), ref _toleranceEarlyIn, value);
    }
    
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan ToleranceLateIn
    {
        get => _toleranceLateIn;
        set => SetPropertyValue(nameof(ToleranceLateIn), ref _toleranceLateIn, value);
    }
    
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan ToleranceEarlyOut
    {
        get => _toleranceEarlyOut;
        set => SetPropertyValue(nameof(ToleranceEarlyOut), ref _toleranceEarlyOut, value);
    }
    
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan ToleranceLateOut
    {
        get => _toleranceLateOut;
        set => SetPropertyValue(nameof(ToleranceLateOut), ref _toleranceLateOut, value);
    }
}