using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Invoicing;
using erp.Module.Helpers.Contacts;
using Microsoft.Extensions.DependencyInjection;
using SequenceFactory = erp.Module.Factories.SequenceFactory;

namespace erp.Module.BusinessObjects.TimeTracking;

[DefaultClassOptions]
[NavigationItem("Time Tracking")]
[ImageName("ShowWorkTimeOnly")]
[XafDefaultProperty(nameof(SecuenciaParteDiario))]
public class DailyTimesheet(Session session) : BaseEntity(session)
{
    private Employee _empleado;
    private string _prefijoParteDiario;
    private string _secuenciaParteDiario;
    private DateTime _fecha;
    private TimeSpan _totalTrabajo;
    private bool _esEntradaTarde;
    private bool _esSalidaTemprana;
    private string _notas;

    [Association("Employee-DailyTimesheets")]
    [ModelDefault("AllowEdit", "False")]
    [RuleRequiredField]
    public Employee Empleado
    {
        get => _empleado;
        set => SetPropertyValue(nameof(Empleado), ref _empleado, value);
    }

    [RuleRequiredField]
    public string PrefijoParteDiario
    {
        get => _prefijoParteDiario;
        set => SetPropertyValue(nameof(PrefijoParteDiario), ref _prefijoParteDiario, value);
    }

    public string SecuenciaParteDiario
    {
        get => _secuenciaParteDiario;
        set => SetPropertyValue(nameof(SecuenciaParteDiario), ref _secuenciaParteDiario, value);
    }

    [RuleRequiredField]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "d")]
    public DateTime Fecha
    {
        get => _fecha.Date;
        set => SetPropertyValue(nameof(Fecha), ref _fecha, value.Date);
    }

    //[ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "h'h 'm'm'")]
    [ModelDefault("AllowEdit", "False")]
    public TimeSpan TotalTrabajo
    {
        get => _totalTrabajo;
        set => SetPropertyValue(nameof(TotalTrabajo), ref _totalTrabajo, value);
    }

    [ModelDefault("AllowEdit", "False")]
    public bool EsEntradaTarde
    {
        get => _esEntradaTarde;
        set => SetPropertyValue(nameof(EsEntradaTarde), ref _esEntradaTarde, value);
    }

    [ModelDefault("AllowEdit", "False")]
    public bool EsSalidaTemprana
    {
        get => _esSalidaTemprana;
        set => SetPropertyValue(nameof(EsSalidaTemprana), ref _esSalidaTemprana, value);
    }
    
    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("RowCount", "3")]
    [XafDisplayName("Notas")]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [Association("DailyTimesheet-Entries")]
    [DevExpress.Xpo.Aggregated]
    public XPCollection<TimesheetEntry> Registros => GetCollection<TimesheetEntry>(nameof(Registros));

    public void Recalcular(WorkdayRule regla)
    {
        var total = TimeSpan.Zero;
        DateTime? primerInicio = null;
        DateTime? ultimoFin = null;

        foreach (var e in Registros)
            if (e.FechaFin.HasValue && e.FechaFin.Value >= e.FechaInicio)
            {
                total += e.FechaFin.Value - e.FechaInicio;
                if (!primerInicio.HasValue || e.FechaInicio < primerInicio.Value) primerInicio = e.FechaInicio;
                if (!ultimoFin.HasValue || e.FechaFin.Value > ultimoFin.Value) ultimoFin = e.FechaFin.Value;
            }

        TotalTrabajo = total;

        if (primerInicio.HasValue)
        {
            var inicioPermitidoMax = primerInicio.Value.Date + regla.InicioJornada + regla.ToleranciaEntradaTarde;
            EsEntradaTarde = primerInicio.Value > inicioPermitidoMax;
        }
        else
        {
            EsEntradaTarde = false;
        }

        if (ultimoFin.HasValue)
        {
            var finPermitidoMin = ultimoFin.Value.Date + regla.FinJornada - regla.ToleranciaSalidaTemprana;
            EsSalidaTemprana = ultimoFin.Value < finPermitidoMin;
        }
        else
        {
            EsSalidaTemprana = false;
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        SecuredPropertySetter.SetPropertyValueWithSecurityBypass(this, nameof(Empleado), GetCurrentUser());
        Fecha = DateTime.Today.Date;
        var companyInfo = CompanyInfoHelper.GetCompanyInfo(Session);
        if (companyInfo == null) return;
        PrefijoParteDiario = companyInfo.PrefijoPartesDiariosPorDefecto;
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        
        if (!Session.IsNewObject(this) || !string.IsNullOrEmpty(SecuenciaParteDiario) ||
            Session is NestedUnitOfWork) return;
        SecuenciaParteDiario =
            SequenceFactory.GetNextSequence(Session, $"{typeof(Invoice).FullName}.{PrefijoParteDiario}",
                PrefijoParteDiario, 5);
    }

    private ApplicationUser GetCurrentUser()
    {
        return Session.GetObjectByKey<ApplicationUser>(
            Session.ServiceProvider.GetRequiredService<ISecurityStrategyBase>().UserId);
    }
}