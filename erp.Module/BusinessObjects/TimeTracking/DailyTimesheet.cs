using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Projects;
// using erp.Module.BusinessObjects.Security; // Ajusta el namespace de ApplicationUser

namespace erp.Module.BusinessObjects.TimeTracking;

[DefaultClassOptions]
[NavigationItem("Registro Horario")]
[ImageName("BO_List")]
[XafDisplayName("Parte Diario")]
public class DailyTimesheet : BaseEntity
{
    public DailyTimesheet(Session session) : base(session) { }

    private ApplicationUser _user;
    private DateTime _date; // solo fecha
    private TimeSpan _totalWork;
    private TimeSpan _regularTime;
    private TimeSpan _overtime;
    private bool _isLateIn;
    private bool _isEarlyOut;

    [Association("ApplicationUser-DailyTimesheets")]
    [RuleRequiredField]
    [XafDisplayName("ApplicationUser")]
    public ApplicationUser User
    {
        get => _user;
        set => SetPropertyValue(nameof(User), ref _user, value);
    }

    [RuleRequiredField]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "d")]
    [XafDisplayName("Fecha")]
    public DateTime Date
    {
        get => _date.Date;
        set => SetPropertyValue(nameof(Date), ref _date, value.Date);
    }

    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    [XafDisplayName("Trabajo Total")]
    public TimeSpan TotalWork
    {
        get => _totalWork;
        protected set => SetPropertyValue(nameof(TotalWork), ref _totalWork, value);
    }

    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    [XafDisplayName("Tiempo Regular")]
    public TimeSpan RegularTime
    {
        get => _regularTime;
        protected set => SetPropertyValue(nameof(RegularTime), ref _regularTime, value);
    }

    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    [XafDisplayName("Horas Extra")]
    public TimeSpan Overtime
    {
        get => _overtime;
        protected set => SetPropertyValue(nameof(Overtime), ref _overtime, value);
    }

    [XafDisplayName("Entrada Tardía")]
    public bool IsLateIn
    {
        get => _isLateIn;
        protected set => SetPropertyValue(nameof(IsLateIn), ref _isLateIn, value);
    }

    [XafDisplayName("Salida Temprana")]
    public bool IsEarlyOut
    {
        get => _isEarlyOut;
        protected set => SetPropertyValue(nameof(IsEarlyOut), ref _isEarlyOut, value);
    }

    [Association("DailyTimesheet-Entries")]
    [XafDisplayName("Entries")]
    public XPCollection<TimesheetEntry> Entries => GetCollection<TimesheetEntry>(nameof(Entries));

    // Recalcula agregados a partir de las TimesheetEntry asociadas
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

        // Flags según tolerancias
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

        switch (rule.OvertimePolicy)
        {
            case OvertimePolicy.None:
                regular = total;
                extra = TimeSpan.Zero;
                break;
            case OvertimePolicy.AfterTarget:
                if (total > rule.DailyTarget)
                {
                    regular = rule.DailyTarget;
                    extra = total - rule.DailyTarget;
                }
                break;
            case OvertimePolicy.AfterEndPlusTolerance:
                if (lastEnd.HasValue)
                {
                    var threshold = lastEnd.Value.Date + rule.WorkdayEnd + rule.ToleranceLateOut;
                    if (lastEnd.Value > threshold)
                    {
                        // Extra = tiempo trabajado después del umbral
                        var after = lastEnd.Value - threshold;
                        extra = after > TimeSpan.Zero ? after : TimeSpan.Zero;
                        regular = total - extra;
                    }
                }
                break;
        }

        RegularTime = regular < TimeSpan.Zero ? TimeSpan.Zero : regular;
        Overtime = extra < TimeSpan.Zero ? TimeSpan.Zero : extra;
    }
}