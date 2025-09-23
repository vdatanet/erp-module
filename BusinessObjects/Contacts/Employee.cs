using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.TimeTracking;

namespace erp.Module.BusinessObjects.Contacts;

[DefaultClassOptions]
[NavigationItem("Contacts")]
[ImageName("BO_Employee")]
public class Employee(Session session) : Contact(session)
{
    private WorkdayRule _workdayRule;
    private bool _isWorking;
    private DateTime? _lastClockIn;
    private DateTime? _lastClockOut;

    [Association("WorkdayRule-Employees")]
    [XafDisplayName("Workday Rule")]
    public WorkdayRule WorkdayRule
    {
        get => _workdayRule;
        set => SetPropertyValue(nameof(WorkdayRule), ref _workdayRule, value);
    }

    [XafDisplayName("Is Working")]
    public bool IsWorking
    {
        get => _isWorking;
        set => SetPropertyValue(nameof(IsWorking), ref _isWorking, value);
    }

    [XafDisplayName("Last Clock In")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    public DateTime? LastClockIn
    {
        get => _lastClockIn;
        set => SetPropertyValue(nameof(LastClockIn), ref _lastClockIn, value);
    }

    [XafDisplayName("Last Clock Out")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    public DateTime? LastClockOut
    {
        get => _lastClockOut;
        set => SetPropertyValue(nameof(LastClockOut), ref _lastClockOut, value);
    }
    
    [Association("Employee-TimesheetEntries")]
    [XafDisplayName("Timesheet Entries")]
    public XPCollection<TimesheetEntry> TimesheetEntries => GetCollection<TimesheetEntry>(nameof(TimesheetEntries));

    [Association("Employee-DailyTimesheets")]
    [XafDisplayName("Daily Timesheets")]
    public XPCollection<DailyTimesheet> DailyTimesheets => GetCollection<DailyTimesheet>(nameof(DailyTimesheets));
}