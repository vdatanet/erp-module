#nullable enable
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Proyectos;
using erp.Module.BusinessObjects.ControlHorario;

namespace erp.Module.Services.TimeTracking;

public static class TimesheetHelper
{
    public enum ToggleResultType { ClockIn, ClockOut }

    public sealed record ToggleResult(ToggleResultType Type, EntradaParte Entry, ParteDiario Daily);

    public static ToggleResult ToggleClock(
        Session session, 
        Empleado employee,  
        DateTime now, 
        Proyecto? project = null, 
        ActividadProyecto? activity = null, 
        string? prefix = null)
    {
        var daily = GetOrCreateDaily(session, employee, now.Date, prefix);
        var open = GetOpenEntry(daily);

        if (open is null)
        {
            // Clock In
            var entry = new EntradaParte(session)
            {
                Empleado = employee,
                FechaInicio = now,
                Proyecto = project,
                Actividad = activity,
                ParteDiario = daily
            };
            EnsureNoOverlap(session, employee, entry.FechaInicio, entry.FechaFin, null);
            entry.Save();
            Recalc(daily);
            return new ToggleResult(ToggleResultType.ClockIn, entry, daily);
        }
        else
        {
            // Clock Out
            open.FechaFin = now;
            if (open.FechaFin < open.FechaInicio)
                throw new UserFriendlyException("La hora de salida no puede ser anterior a la de entrada.");
            EnsureNoOverlap(session, employee, open.FechaInicio, open.FechaFin, open);
            open.Save();
            Recalc(daily);
            return new ToggleResult(ToggleResultType.ClockOut, open, daily);
        }
    }
    
    private static ParteDiario GetOrCreateDaily(Session session, Empleado employee, DateTime date, string? prefix)
    {
        var q = new XPQuery<ParteDiario>(session);
        var daily = q.FirstOrDefault(t => t.Empleado == employee && t.Fecha == date);
        if (daily != null) return daily;

        daily = new ParteDiario(session)
        {
            Empleado = employee,
            Fecha = date
        };
        if (!string.IsNullOrWhiteSpace(prefix))
            daily.PrefijoParteDiario = prefix;

        daily.Save();
        return daily;
    }

    private static EntradaParte? GetOpenEntry(ParteDiario daily) =>
        daily.Registros.FirstOrDefault(e => !e.FechaFin.HasValue);

    private static void EnsureNoOverlap(Session session, Empleado employee, DateTime start, DateTime? end, EntradaParte? exclude)
    {
        var q = new XPQuery<EntradaParte>(session);
        var overlaps = q.Any(e =>
            e != exclude &&
            e.Empleado == employee &&
            e.FechaInicio.Date == start.Date &&
            start < (e.FechaFin ?? DateTime.MaxValue) &&
            (end ?? DateTime.MaxValue) > e.FechaInicio);

        if (overlaps)
            throw new UserFriendlyException("El rango de tiempo se solapa con otra entrada del día.");
    }

    private static void Recalc(ParteDiario daily)
    {
        var rule = daily.Empleado?.ReglaJornadaLaboral;
        if (rule != null)
        {
            daily.Recalcular(rule);
            daily.Save();
        }
    }
}