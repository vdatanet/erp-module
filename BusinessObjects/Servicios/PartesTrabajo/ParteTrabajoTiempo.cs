using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;

namespace erp.Module.BusinessObjects.Servicios.PartesTrabajo;

[DefaultClassOptions]
[NavigationItem("Servicios")]
[XafDisplayName("Parte de Trabajo - Tiempo")]
public class ParteTrabajoTiempo(Session session) : EntidadBase(session)
{
    private ParteTrabajo? _parte;
    private DateTime _fechaInicio;
    private DateTime? _fechaFin;
    private double _horas;
    private Empleado? _tecnico;
    private string? _actividad;
    private string? _observaciones;

    [Association("ParteTrabajo-Tiempos")]
    [XafDisplayName("Parte de Trabajo")]
    public ParteTrabajo? Parte
    {
        get => _parte;
        set => SetPropertyValue(nameof(Parte), ref _parte, value);
    }

    [XafDisplayName("Fecha Inicio")]
    public DateTime FechaInicio
    {
        get => _fechaInicio;
        set => SetPropertyValue(nameof(FechaInicio), ref _fechaInicio, value);
    }

    [XafDisplayName("Fecha Fin")]
    public DateTime? FechaFin
    {
        get => _fechaFin;
        set => SetPropertyValue(nameof(FechaFin), ref _fechaFin, value);
    }

    [XafDisplayName("Horas")]
    public double Horas
    {
        get => _horas;
        set => SetPropertyValue(nameof(Horas), ref _horas, value);
    }

    [XafDisplayName("Técnico")]
    public Empleado? Tecnico
    {
        get => _tecnico;
        set => SetPropertyValue(nameof(Tecnico), ref _tecnico, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Actividad")]
    public string? Actividad
    {
        get => _actividad;
        set => SetPropertyValue(nameof(Actividad), ref _actividad, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaInicio = DateTime.Now;
    }
}
