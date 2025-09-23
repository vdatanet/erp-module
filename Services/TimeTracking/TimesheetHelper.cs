#nullable enable
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Projects;
using erp.Module.BusinessObjects.TimeTracking;

namespace erp.Module.Services.TimeTracking;

public static class TimesheetHelper
{
    public enum ToggleResultType { ClockIn, ClockOut }

    public sealed record ToggleResult(ToggleResultType Type, TimesheetEntry Entry, DailyTimesheet Daily);

    public static ToggleResult ToggleClock(
        Session session, 
        Employee employee,  
        DateTime now, 
        Project? project = null, 
        ProjectActivity? activity = null, 
        string? prefix = null)
    {
        var daily = GetOrCreateDaily(session, employee, now.Date, prefix);
        var open = GetOpenEntry(daily);

        if (open is null)
        {
            // Clock In
            var entry = new TimesheetEntry(session)
            {
                Employee = employee,
                StartOn = now,
                Project = project,
                Activity = activity,
                DailyTimesheet = daily
            };
            EnsureNoOverlap(session, employee, entry.StartOn, entry.EndOn, null);
            entry.Save();
            Recalc(daily);
            return new ToggleResult(ToggleResultType.ClockIn, entry, daily);
        }
        else
        {
            // Clock Out
            open.EndOn = now;
            if (open.EndOn < open.StartOn)
                throw new UserFriendlyException("La hora de salida no puede ser anterior a la de entrada.");
            EnsureNoOverlap(session, employee, open.StartOn, open.EndOn, open);
            open.Save();
            Recalc(daily);
            return new ToggleResult(ToggleResultType.ClockOut, open, daily);
        }
    }
    
    private static DailyTimesheet GetOrCreateDaily(Session session, Employee employee, DateTime date, string? prefix)
    {
        var q = new XPQuery<DailyTimesheet>(session);
        var daily = q.FirstOrDefault(t => t.Employee == employee && t.Date == date);
        if (daily != null) return daily;

        daily = new DailyTimesheet(session)
        {
            Employee = employee,
            Date = date
        };
        if (!string.IsNullOrWhiteSpace(prefix))
            daily.DailyTimesheetPrefix = prefix;

        daily.Save();
        return daily;
    }

    private static TimesheetEntry? GetOpenEntry(DailyTimesheet daily) =>
        daily.Entries.FirstOrDefault(e => !e.EndOn.HasValue);

    private static void EnsureNoOverlap(Session session, Employee employee, DateTime start, DateTime? end, TimesheetEntry? exclude)
    {
        var q = new XPQuery<TimesheetEntry>(session);
        var overlaps = q.Any(e =>
            e != exclude &&
            e.Employee == employee &&
            e.StartOn.Date == start.Date &&
            start < (e.EndOn ?? DateTime.MaxValue) &&
            (end ?? DateTime.MaxValue) > e.StartOn);

        if (overlaps)
            throw new UserFriendlyException("El rango de tiempo se solapa con otra entrada del día.");
    }

    private static void Recalc(DailyTimesheet daily)
    {
        var rule = daily.Employee?.WorkdayRule;
        if (rule != null)
        {
            daily.Recalculate(rule);
            daily.Save();
        }
    }
}