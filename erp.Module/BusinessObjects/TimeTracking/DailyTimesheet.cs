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
[ImageName("BO_List")]
[XafDisplayName("Daily Timesheet")]
public class DailyTimesheet : BaseEntity
{
    public DailyTimesheet(Session session) : base(session) { }

    private ApplicationUser _employee;
    private DateTime _date;
    private TimeSpan _totalWork;
    private TimeSpan _regularTime;
    private bool _isLateIn;
    private bool _isEarlyOut;

    [Association("ApplicationUser-DailyTimesheets")]
    [RuleRequiredField]
    public ApplicationUser Employee
    {
        get => _employee;
        set => SetPropertyValue(nameof(Employee), ref _employee, value);
    }

    [RuleRequiredField]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "d")]
    public DateTime Date
    {
        get => _date.Date;
        set => SetPropertyValue(nameof(Date), ref _date, value.Date);
    }

    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan TotalWork
    {
        get => _totalWork;
        protected set => SetPropertyValue(nameof(TotalWork), ref _totalWork, value);
    }

    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    public TimeSpan RegularTime
    {
        get => _regularTime;
        protected set => SetPropertyValue(nameof(RegularTime), ref _regularTime, value);
    }
    
    public bool IsLateIn
    {
        get => _isLateIn;
        protected set => SetPropertyValue(nameof(IsLateIn), ref _isLateIn, value);
    }
    
    public bool IsEarlyOut
    {
        get => _isEarlyOut;
        protected set => SetPropertyValue(nameof(IsEarlyOut), ref _isEarlyOut, value);
    }

    [Association("DailyTimesheet-Entries")]
    public XPCollection<TimesheetEntry> Entries => GetCollection<TimesheetEntry>(nameof(Entries));
    
    public void Recalculate(WorkdayRule rule)
    {
        TimeSpan total = TimeSpan.Zero;
        DateTime? firstStart = null;
        DateTime? lastEnd = null;

        foreach (var e in Entries)
        {
            if (e.EndOn.HasValue && e.EndOn.Value >= e.StartOn)
            {
                total += (e.EndOn.Value - e.StartOn);
                if (!firstStart.HasValue || e.StartOn < firstStart.Value) firstStart = e.StartOn;
                if (!lastEnd.HasValue || e.EndOn.Value > lastEnd.Value) lastEnd = e.EndOn.Value;
            }
        }

        TotalWork = total;
        
        if (firstStart.HasValue)
        {
            var allowedStartMax = firstStart.Value.Date + rule.WorkdayStart + rule.ToleranceLateIn;
            IsLateIn = firstStart.Value > allowedStartMax;
        }
        else
        {
            IsLateIn = false;
        }

        if (lastEnd.HasValue)
        {
            var allowedEndMin = lastEnd.Value.Date + rule.WorkdayEnd - rule.ToleranceEarlyOut;
            IsEarlyOut = lastEnd.Value < allowedEndMin;
        }
        else
        {
            IsEarlyOut = false;
        }

        // Regular vs Extra
        TimeSpan regular = total;
        TimeSpan extra = TimeSpan.Zero;
        
        RegularTime = regular < TimeSpan.Zero ? TimeSpan.Zero : regular;
    }
}