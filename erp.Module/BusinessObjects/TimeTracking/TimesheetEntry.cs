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
[ImageName("BO_Time")]
[XafDisplayName("Timesheet Entry")]
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
    [XafDisplayName("Application User")]
    public ApplicationUser User
    {
        get => _user;
        set => SetPropertyValue(nameof(User), ref _user, value);
    }

    [RuleRequiredField]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [XafDisplayName("Start On")]
    public DateTime StartOn
    {
        get => _startOn;
        set
        {
            if (SetPropertyValue(nameof(StartOn), ref _startOn, value)) RecalculateDuration();
        }
    }

    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [XafDisplayName("End On")]
    public DateTime? EndOn
    {
        get => _endOn;
        set
        {
            if (SetPropertyValue(nameof(EndOn), ref _endOn, value)) RecalculateDuration();
        }
    }

    [Association("Project-TimesheetEntries")]
    [RuleRequiredField]
    [XafDisplayName("Project")]
    public Project Project
    {
        get => _project;
        set => SetPropertyValue(nameof(Project), ref _project, value);
    }

    [Association("ProjectActivity-TimesheetEntries")]
    [XafDisplayName("Activity")]
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

    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    [XafDisplayName("Duration")]
    public TimeSpan Duration
    {
        get => _duration;
        protected set => SetPropertyValue(nameof(Duration), ref _duration, value);
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