using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Mantenimientos.Enums;
using erp.Module.BusinessObjects.Ventas;

namespace erp.Module.BusinessObjects.Mantenimientos;

[DefaultClassOptions]
[NavigationItem("Mantenimientos")]
[XafDisplayName("Contratos de Mantenimiento")]
public class ContratoMantenimiento(Session session) : EntidadBase(session)
{
    private string? _codigo;
    private string? _descripcion;
    private Cliente? _cliente;
    private PedidoVenta? _pedidoVenta;
    private DateTime _fechaInicio;
    private DateTime _fechaFin;
    private EstadoContratoMantenimiento _estado;
    private string? _observaciones;

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

    [XafDisplayName("Cliente")]
    public Cliente? Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [XafDisplayName("Pedido de Venta")]
    public PedidoVenta? PedidoVenta
    {
        get => _pedidoVenta;
        set
        {
            if (SetPropertyValue(nameof(PedidoVenta), ref _pedidoVenta, value))
            {
                if (!IsLoading && !IsSaving && value != null && Cliente == null)
                {
                    Cliente = value.Cliente as Cliente;
                }
            }
        }
    }

    [XafDisplayName("Fecha Inicio")]
    public DateTime FechaInicio
    {
        get => _fechaInicio;
        set => SetPropertyValue(nameof(FechaInicio), ref _fechaInicio, value);
    }

    [XafDisplayName("Fecha Fin")]
    public DateTime FechaFin
    {
        get => _fechaFin;
        set => SetPropertyValue(nameof(FechaFin), ref _fechaFin, value);
    }

    [XafDisplayName("Estado")]
    public EstadoContratoMantenimiento Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [Association("Contrato-Activos")]
    [XafDisplayName("Activos")]
    public XPCollection<ActivoMantenimiento> Activos => GetCollection<ActivoMantenimiento>(nameof(Activos));

    [Association("Contrato-Tareas")]
    [XafDisplayName("Tareas")]
    public XPCollection<TareaMantenimiento> Tareas => GetCollection<TareaMantenimiento>(nameof(Tareas));

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadoContratoMantenimiento.Borrador;
        FechaInicio = DateTime.Today;
        FechaFin = DateTime.Today.AddYears(1);
    }
}
