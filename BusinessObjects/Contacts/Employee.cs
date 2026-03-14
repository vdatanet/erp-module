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
    private WorkdayRule _reglaJornadaLaboral;
    private bool _estaTrabajando;
    private DateTime? _ultimoRegistroEntrada;
    private DateTime? _ultimoRegistroSalida;

    [Association("WorkdayRule-Employees")]
    [XafDisplayName("Regla Jornada Laboral")]
    public WorkdayRule ReglaJornadaLaboral
    {
        get => _reglaJornadaLaboral;
        set => SetPropertyValue(nameof(ReglaJornadaLaboral), ref _reglaJornadaLaboral, value);
    }

    [XafDisplayName("¿Está trabajando?")]
    public bool EstaTrabajando
    {
        get => _estaTrabajando;
        set => SetPropertyValue(nameof(EstaTrabajando), ref _estaTrabajando, value);
    }

    [XafDisplayName("Último Registro Entrada")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    public DateTime? UltimoRegistroEntrada
    {
        get => _ultimoRegistroEntrada;
        set => SetPropertyValue(nameof(UltimoRegistroEntrada), ref _ultimoRegistroEntrada, value);
    }

    [XafDisplayName("Último Registro Salida")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    public DateTime? UltimoRegistroSalida
    {
        get => _ultimoRegistroSalida;
        set => SetPropertyValue(nameof(UltimoRegistroSalida), ref _ultimoRegistroSalida, value);
    }
    
    [Association("Employee-TimesheetEntries")]
    [XafDisplayName("Registros de Tiempo")]
    public XPCollection<TimesheetEntry> RegistrosTiempo => GetCollection<TimesheetEntry>(nameof(RegistrosTiempo));

    [Association("Employee-DailyTimesheets")]
    [XafDisplayName("Partes Diarios")]
    public XPCollection<DailyTimesheet> PartesDiarios => GetCollection<DailyTimesheet>(nameof(PartesDiarios));
}