using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Mantenimientos.Enums;
using erp.Module.BusinessObjects.TrabajoDeCampo;

namespace erp.Module.BusinessObjects.Mantenimientos;

[DefaultClassOptions]
[NavigationItem("Mantenimientos")]
[XafDisplayName("Planificación de Mantenimiento")]
public class PlanificacionMantenimiento(Session session) : EntidadBase(session)
{
    private TareaMantenimiento? _tarea;
    private DateTime _fechaPrevista;
    private DateTime? _fechaReal;
    private EstadoTareaMantenimiento _estado;
    private int _prioridad;
    private bool _generadoAutomaticamente;
    private PedidoTrabajoDeCampo? _trabajoCampo;
    private string? _observaciones;

    [Association("Tarea-Planificaciones")]
    [XafDisplayName("Tarea")]
    public TareaMantenimiento? Tarea
    {
        get => _tarea;
        set => SetPropertyValue(nameof(Tarea), ref _tarea, value);
    }

    [XafDisplayName("Fecha Prevista")]
    public DateTime FechaPrevista
    {
        get => _fechaPrevista;
        set => SetPropertyValue(nameof(FechaPrevista), ref _fechaPrevista, value);
    }

    [XafDisplayName("Fecha Real")]
    public DateTime? FechaReal
    {
        get => _fechaReal;
        set => SetPropertyValue(nameof(FechaReal), ref _fechaReal, value);
    }

    [XafDisplayName("Estado")]
    public EstadoTareaMantenimiento Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [XafDisplayName("Prioridad")]
    public int Prioridad
    {
        get => _prioridad;
        set => SetPropertyValue(nameof(Prioridad), ref _prioridad, value);
    }

    [XafDisplayName("Generado Automáticamente")]
    public bool GeneradoAutomaticamente
    {
        get => _generadoAutomaticamente;
        set => SetPropertyValue(nameof(GeneradoAutomaticamente), ref _generadoAutomaticamente, value);
    }

    [XafDisplayName("Trabajo de Campo (Pedido)")]
    public PedidoTrabajoDeCampo? TrabajoCampo
    {
        get => _trabajoCampo;
        set => SetPropertyValue(nameof(TrabajoCampo), ref _trabajoCampo, value);
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
        Estado = EstadoTareaMantenimiento.Pendiente;
        FechaPrevista = DateTime.Today;
    }
}
