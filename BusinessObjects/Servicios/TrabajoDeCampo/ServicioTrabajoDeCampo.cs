using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;

namespace erp.Module.BusinessObjects.Servicios.TrabajoDeCampo;

[DefaultClassOptions]
[NavigationItem("Servicios")]
[XafDisplayName("Servicio de trabajo de campo")]
public class ServicioTrabajoDeCampo(Session session) : EntidadBase(session)
{
    private PedidoTrabajoDeCampo? _pedidoTC;
    private PeriodicidadTrabajoDeCampo? _periodicidad;
    private EstadoServicioTrabajoDeCampo _estado;
    private DateTime? _fechaPlanificada;
    private DateTime? _fechaInicioReal;
    private DateTime? _fechaFinReal;
    private Empleado? _empleadoAsignado;
    private string? _notas;

    [Association("PedidoTC-ServiciosTC")]
    [XafDisplayName("Pedido TC")]
    public PedidoTrabajoDeCampo? PedidoTC
    {
        get => _pedidoTC;
        set => SetPropertyValue(nameof(PedidoTC), ref _pedidoTC, value);
    }

    [XafDisplayName("Periodicidad")]
    public PeriodicidadTrabajoDeCampo? Periodicidad
    {
        get => _periodicidad;
        set => SetPropertyValue(nameof(Periodicidad), ref _periodicidad, value);
    }

    [XafDisplayName("Estado")]
    public EstadoServicioTrabajoDeCampo Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [XafDisplayName("Fecha planificada")]
    public DateTime? FechaPlanificada
    {
        get => _fechaPlanificada;
        set => SetPropertyValue(nameof(FechaPlanificada), ref _fechaPlanificada, value);
    }

    [XafDisplayName("Fecha inicio real")]
    public DateTime? FechaInicioReal
    {
        get => _fechaInicioReal;
        set => SetPropertyValue(nameof(FechaInicioReal), ref _fechaInicioReal, value);
    }

    [XafDisplayName("Fecha fin real")]
    public DateTime? FechaFinReal
    {
        get => _fechaFinReal;
        set => SetPropertyValue(nameof(FechaFinReal), ref _fechaFinReal, value);
    }

    [XafDisplayName("Empleado asignado")]
    public Empleado? EmpleadoAsignado
    {
        get => _empleadoAsignado;
        set => SetPropertyValue(nameof(EmpleadoAsignado), ref _empleadoAsignado, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [Association("ServicioTC-TareasTC")]
    [XafDisplayName("Tareas")]
    public XPCollection<TareaTrabajoDeCampo> Tareas => GetCollection<TareaTrabajoDeCampo>(nameof(Tareas));

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadoServicioTrabajoDeCampo.PendientePlanificacion;
    }
}
