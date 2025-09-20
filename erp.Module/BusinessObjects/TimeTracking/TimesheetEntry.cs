using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Projects;

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

    [Association("ApplicationUser-TimesheetEntries")]
    [RuleRequiredField]
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
    [RuleRequiredField]
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

    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "h'h 'm'm'")]
    [XafDisplayName("Duration")]
    public TimeSpan Duration
    {
        get => _duration;
        protected set => SetPropertyValue(nameof(Duration), ref _duration, value);
    }

    private DailyTimesheet _dailyTimesheet;

    [Association("DailyTimesheet-Entries")]
    public DailyTimesheet DailyTimesheet
    {
        get => _dailyTimesheet;
        set => SetPropertyValue(nameof(DailyTimesheet), ref _dailyTimesheet, value);
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        RecalculateDuration();
    }

    private void RecalculateDuration()
    {
        if (EndOn.HasValue && EndOn.Value >= StartOn)
            Duration = EndOn.Value - StartOn;
        else
            Duration = TimeSpan.Zero;
    }
}