using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Contabilidad;

[NavigationItem(false)]
[XafDisplayName("Periodo Bloqueado")]
public class PeriodoBloqueado(Session session) : EntidadBase(session)
{
    private Ejercicio? _ejercicio;
    private DateTime _fechaInicio;
    private DateTime _fechaFin;
    private string? _descripcion;

    [Association("Ejercicio-PeriodosBloqueados")]
    [XafDisplayName("Ejercicio")]
    [RuleRequiredField]
    public Ejercicio? Ejercicio
    {
        get => _ejercicio;
        set => SetPropertyValue(nameof(Ejercicio), ref _ejercicio, value);
    }

    [XafDisplayName("Fecha Inicio")]
    [RuleRequiredField]
    public DateTime FechaInicio
    {
        get => _fechaInicio;
        set => SetPropertyValue(nameof(FechaInicio), ref _fechaInicio, value);
    }

    [XafDisplayName("Fecha Fin")]
    [RuleRequiredField]
    public DateTime FechaFin
    {
        get => _fechaFin;
        set => SetPropertyValue(nameof(FechaFin), ref _fechaFin, value);
    }

    [Size(255)]
    [XafDisplayName("Descripción")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [Browsable(false)]
    [RuleFromBoolProperty("PeriodoBloqueado_RangoValido", DefaultContexts.Save, "La fecha de inicio debe ser anterior o igual a la fecha de fin.", UsedProperties = nameof(FechaInicio) + "," + nameof(FechaFin))]
    public bool IsRangoValido => FechaInicio <= FechaFin;

    [Browsable(false)]
    [RuleFromBoolProperty("PeriodoBloqueado_DentroDelEjercicio", DefaultContexts.Save, "Las fechas deben estar dentro del rango del ejercicio.", UsedProperties = nameof(FechaInicio) + "," + nameof(FechaFin))]
    public bool IsDentroDelEjercicio => Ejercicio == null || (FechaInicio >= Ejercicio.FechaInicio && FechaFin <= Ejercicio.FechaFin);

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaInicio = DateTime.Today;
        FechaFin = DateTime.Today;
    }
}
