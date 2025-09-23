using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Projects;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.BusinessObjects.TimeTracking;

[DefaultClassOptions]
[NavigationItem("Time Tracking")]
[ImageName("Time")]
public class TimesheetEntry(Session session) : BaseEntity(session)
{
    private ApplicationUser _user;
    private DateTime _startOn;
    private DateTime? _endOn;
    private Project _project;
    private ProjectActivity _activity;
    private string _notes;
    private TimeSpan _duration;
    private DailyTimesheet _dailyTimesheet;
    private DailyTimesheet _previousDailyTimesheet;
    private WorkdayRule _previousWorkdayRuleEmployee;

    [Association("ApplicationUser-TimesheetEntries")]
    [RuleRequiredField]
    [ModelDefault("AllowEdit", "False")]
    public ApplicationUser Employee
    {
        get => _user;
        set => SetPropertyValue(nameof(Employee), ref _user, value);
    }

    [RuleRequiredField]
    [ImmediatePostData]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.EditMask), "g")]

    public DateTime StartOn
    {
        get => _startOn;
        set
        {
            if (SetPropertyValue(nameof(StartOn), ref _startOn, value)) RecalculateDuration();
        }
    }

    [ImmediatePostData]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.EditMask), "g")]
    public DateTime? EndOn
    {
        get => _endOn;
        set
        {
            if (SetPropertyValue(nameof(EndOn), ref _endOn, value)) RecalculateDuration();
        }
    }

    [Association("Project-TimesheetEntries")]
    [ImmediatePostData]
    public Project Project
    {
        get => _project;
        set => SetPropertyValue(nameof(Project), ref _project, value);
    }

    [Association("ProjectActivity-TimesheetEntries")]
    [DataSourceProperty("Project.Activities")]
    public ProjectActivity Activity
    {
        get => _activity;
        set => SetPropertyValue(nameof(Activity), ref _activity, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("RowCount", "3")]
    [XafDisplayName("Notes")]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Duration")]
    public TimeSpan Duration
    {
        get => _duration;
        set => SetPropertyValue(nameof(Duration), ref _duration, value);
    }

    [Association("DailyTimesheet-Entries")]
    public DailyTimesheet DailyTimesheet
    {
        get => _dailyTimesheet;
        set => SetPropertyValue(nameof(DailyTimesheet), ref _dailyTimesheet, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(Employee), GetCurrentUser());
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        RecalculateDuration();
    }

    protected override void OnDeleting()
    {
        if (DailyTimesheet is not null) _previousDailyTimesheet = DailyTimesheet;
        _previousWorkdayRuleEmployee = Employee?.WorkdayRule ?? _previousWorkdayRuleEmployee;
        base.OnDeleting();
    }

    protected override void OnDeleted()
    {
        base.OnDeleted();

        if (_previousDailyTimesheet is { } ts && _previousWorkdayRuleEmployee is { } rule)
            ts.Recalculate(rule);

        _previousDailyTimesheet = null;
        _previousWorkdayRuleEmployee = null;
    }

    private void RecalculateDuration()
    {
        if (EndOn.HasValue && EndOn.Value >= StartOn)
            Duration = EndOn.Value - StartOn;
        else
            Duration = TimeSpan.Zero;

        if (DailyTimesheet is { } ts && Employee?.WorkdayRule is { } rule)
            ts.Recalculate(rule);
    }

    private ApplicationUser GetCurrentUser()
    {
        return Session.GetObjectByKey<ApplicationUser>(
            Session.ServiceProvider.GetRequiredService<ISecurityStrategyBase>().UserId);
    }
}