using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Servicios.Mantenimientos.Enums;
using erp.Module.BusinessObjects.Servicios.TrabajoDeCampo;

namespace erp.Module.BusinessObjects.Servicios.Mantenimientos;

[DefaultClassOptions]
[NavigationItem("Servicios")]
[XafDisplayName("Tareas de Mantenimiento")]
public class TareaMantenimiento(Session session) : EntidadBase(session)
{
    private string? _codigo;
    private string? _descripcion;
    private ActivoMantenimiento? _activo;
    private ContratoMantenimiento? _contrato;
    private TipoMantenimiento _tipo;
    private PeriodicidadTrabajoDeCampo? _periodicidad;
    private double _tiempoEstimadoHoras;
    private bool _generaTrabajoCampoAutomatico;
    private EstadoTareaMantenimiento _estado;
    private DateTime? _ultimaEjecucion;
    private DateTime? _proximaEjecucion;

    [XafDisplayName("Código")]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [XafDisplayName("Descripción")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [Association("Activo-Tareas")]
    [XafDisplayName("Activo")]
    public ActivoMantenimiento? Activo
    {
        get => _activo;
        set
        {
            if (SetPropertyValue(nameof(Activo), ref _activo, value))
            {
                if (!IsLoading && !IsSaving && value != null && Contrato == null)
                {
                    Contrato = value.Contrato;
                }
            }
        }
    }

    [Association("Contrato-Tareas")]
    [XafDisplayName("Contrato")]
    public ContratoMantenimiento? Contrato
    {
        get => _contrato;
        set => SetPropertyValue(nameof(Contrato), ref _contrato, value);
    }

    [XafDisplayName("Tipo")]
    public TipoMantenimiento Tipo
    {
        get => _tipo;
        set => SetPropertyValue(nameof(Tipo), ref _tipo, value);
    }

    [XafDisplayName("Periodicidad")]
    public PeriodicidadTrabajoDeCampo? Periodicidad
    {
        get => _periodicidad;
        set => SetPropertyValue(nameof(Periodicidad), ref _periodicidad, value);
    }

    [XafDisplayName("Tiempo Estimado (Horas)")]
    public double TiempoEstimadoHoras
    {
        get => _tiempoEstimadoHoras;
        set => SetPropertyValue(nameof(TiempoEstimadoHoras), ref _tiempoEstimadoHoras, value);
    }

    [XafDisplayName("Genera TC Automático")]
    public bool GeneraTrabajoCampoAutomatico
    {
        get => _generaTrabajoCampoAutomatico;
        set => SetPropertyValue(nameof(GeneraTrabajoCampoAutomatico), ref _generaTrabajoCampoAutomatico, value);
    }

    [XafDisplayName("Estado")]
    public EstadoTareaMantenimiento Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [XafDisplayName("Última Ejecución")]
    public DateTime? UltimaEjecucion
    {
        get => _ultimaEjecucion;
        set => SetPropertyValue(nameof(UltimaEjecucion), ref _ultimaEjecucion, value);
    }

    [XafDisplayName("Próxima Ejecución")]
    public DateTime? ProximaEjecucion
    {
        get => _proximaEjecucion;
        set => SetPropertyValue(nameof(ProximaEjecucion), ref _proximaEjecucion, value);
    }

    [Association("Tarea-Planificaciones")]
    [XafDisplayName("Planificaciones")]
    public XPCollection<PlanificacionMantenimiento> Planificaciones => GetCollection<PlanificacionMantenimiento>(nameof(Planificaciones));

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadoTareaMantenimiento.Pendiente;
        Tipo = TipoMantenimiento.Preventivo;
        GeneraTrabajoCampoAutomatico = true;
    }
}
