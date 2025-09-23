using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Helpers;
using erp.Module.BusinessObjects.Invoicing;
using Microsoft.Extensions.DependencyInjection;
using SequenceFactory = erp.Module.Factories.SequenceFactory;

namespace erp.Module.BusinessObjects.TimeTracking;

[DefaultClassOptions]
[NavigationItem("Time Tracking")]
[ImageName("ShowWorkTimeOnly")]
[XafDefaultProperty(nameof(DailyTimesheetSequence))]
public class DailyTimesheet(Session session) : BaseEntity(session)
{
    private Employee _employee;
    private string _dailyTimesheetPrefix;
    private string _dailyTimesheetSequence;
    private DateTime _date;
    private TimeSpan _totalWork;
    private bool _isLateIn;
    private bool _isEarlyOut;
    private string _notes;

    [Association("Employee-DailyTimesheets")]
    [ModelDefault("AllowEdit", "False")]
    [RuleRequiredField]
    public Employee Employee
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

    public string DailyTimesheetSequence
    {
        get => _dailyTimesheetSequence;
        set => SetPropertyValue(nameof(DailyTimesheetSequence), ref _dailyTimesheetSequence, value);
    }

    [RuleRequiredField]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "d")]
    public DateTime Date
    {
        get => _date.Date;
        set => SetPropertyValue(nameof(Date), ref _date, value.Date);
    }

    //[ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "h'h 'm'm'")]
    [ModelDefault("AllowEdit", "False")]
    public TimeSpan TotalWork
    {
        get => _totalWork;
        set => SetPropertyValue(nameof(TotalWork), ref _totalWork, value);
    }

    [ModelDefault("AllowEdit", "False")]
    public bool IsLateIn
    {
        get => _isLateIn;
        set => SetPropertyValue(nameof(IsLateIn), ref _isLateIn, value);
    }

    [ModelDefault("AllowEdit", "False")]
    public bool IsEarlyOut
    {
        get => _isEarlyOut;
        set => SetPropertyValue(nameof(IsEarlyOut), ref _isEarlyOut, value);
    }
    
    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("RowCount", "3")]
    [XafDisplayName("Notes")]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }

    [Association("DailyTimesheet-Entries")]
    [DevExpress.Xpo.Aggregated]
    public XPCollection<TimesheetEntry> Entries => GetCollection<TimesheetEntry>(nameof(Entries));

    public void Recalculate(WorkdayRule rule)
    {
        var total = TimeSpan.Zero;
        DateTime? firstStart = null;
        DateTime? lastEnd = null;

        foreach (var e in Entries)
            if (e.EndOn.HasValue && e.EndOn.Value >= e.StartOn)
            {
                total += e.EndOn.Value - e.StartOn;
                if (!firstStart.HasValue || e.StartOn < firstStart.Value) firstStart = e.StartOn;
                if (!lastEnd.HasValue || e.EndOn.Value > lastEnd.Value) lastEnd = e.EndOn.Value;
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

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(Employee), GetCurrentUser());
        Date = DateTime.Today.Date;
        var companyInfo = CompanyInfoHelper.GetCompanyInfo(Session);
        if (companyInfo == null) return;
        DailyTimesheetPrefix = companyInfo.DefaultDailyTimeSheetPrefix;
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        
        if (!Session.IsNewObject(this) || !string.IsNullOrEmpty(DailyTimesheetSequence) ||
            Session is NestedUnitOfWork) return;
        DailyTimesheetSequence =
            SequenceFactory.GetNextSequence(Session, $"{typeof(Invoice).FullName}.{DailyTimesheetPrefix}",
                DailyTimesheetPrefix, 5);
    }

    private ApplicationUser GetCurrentUser()
    {
        return Session.GetObjectByKey<ApplicationUser>(
            Session.ServiceProvider.GetRequiredService<ISecurityStrategyBase>().UserId);
    }
}