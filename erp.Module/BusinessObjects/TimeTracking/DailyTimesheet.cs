using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Invoicing;
using erp.Module.BusinessObjects.Projects;
using erp.Module.Factories;

namespace erp.Module.BusinessObjects.TimeTracking;

[DefaultClassOptions]
[NavigationItem("Time Tracking")]
[ImageName("ShowWorkTimeOnly")]
[XafDefaultProperty(nameof(DailyTimesheetCode))]
public class DailyTimesheet(Session session) : BaseEntity(session)
{
    private ApplicationUser _employee;
    private string _dailyTimesheetPrefix;
    private string _dailyTimesheetCode;
    private DateTime _date;
    private TimeSpan _totalWork;
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
    public string DailyTimesheetPrefix
    {
        get => _dailyTimesheetPrefix;
        set => SetPropertyValue(nameof(DailyTimesheetPrefix), ref _dailyTimesheetPrefix, value);
    }

    public string DailyTimesheetCode
    {
        get => _dailyTimesheetCode;
        set => SetPropertyValue(nameof(DailyTimesheetCode), ref _dailyTimesheetCode, value);
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
    }
    
    protected override void OnSaving()
    {
        base.OnSaving();
        if (!Session.IsNewObject(this) || !string.IsNullOrEmpty(DailyTimesheetCode) || Session is NestedUnitOfWork) return;
        DailyTimesheetCode =
            SequenceFactory.GetNextSequence(Session, $"{typeof(Invoice).FullName}.{DailyTimesheetPrefix}", DailyTimesheetPrefix, 5);
    }
}