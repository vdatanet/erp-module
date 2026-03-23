using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Servicios.PartesTrabajo;

namespace erp.Module.BusinessObjects.Suscripciones;

[DefaultClassOptions]
[NavigationItem("Suscripciones")]
[XafDisplayName("Consumo de Suscripción")]
public class ConsumoSuscripcion(Session session) : EntidadBase(session)
{
    private CoberturaSuscripcion? _cobertura;
    private ParteTrabajo? _parteTrabajo;
    private DateTime _fecha;
    private decimal _cantidadHoras;
    private int _cantidadVisitas;

    [Association("Cobertura-Consumos")]
    public CoberturaSuscripcion? Cobertura
    {
        get => _cobertura;
        set => SetPropertyValue(nameof(Cobertura), ref _cobertura, value);
    }

    [XafDisplayName("Parte de Trabajo")]
    public ParteTrabajo? ParteTrabajo
    {
        get => _parteTrabajo;
        set => SetPropertyValue(nameof(ParteTrabajo), ref _parteTrabajo, value);
    }

    [XafDisplayName("Fecha")]
    public DateTime Fecha
    {
        get => _fecha;
        set => SetPropertyValue(nameof(Fecha), ref _fecha, value);
    }

    [XafDisplayName("Horas Consumidas")]
    public decimal CantidadHoras
    {
        get => _cantidadHoras;
        set => SetPropertyValue(nameof(CantidadHoras), ref _cantidadHoras, value);
    }

    [XafDisplayName("Visitas Consumidas")]
    public int CantidadVisitas
    {
        get => _cantidadVisitas;
        set => SetPropertyValue(nameof(CantidadVisitas), ref _cantidadVisitas, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Fecha = DateTime.Today;
    }
}
