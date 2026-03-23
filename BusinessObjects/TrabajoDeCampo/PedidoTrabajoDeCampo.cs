using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Ventas;

namespace erp.Module.BusinessObjects.TrabajoDeCampo;

[DefaultClassOptions]
[NavigationItem("Mantenimientos")]
[XafDisplayName("Trabajos de Campo")]
public class PedidoTrabajoDeCampo(Session session) : EntidadBase(session)
{
    private SolicitudTrabajoDeCampo? _solicitud;
    private PedidoVenta? _pedidoVentaRelacionado;
    private string? _referencia;
    private DateTime _fechaPedido;

    [Association("Solicitud-PedidosTC")]
    [XafDisplayName("Solicitud origen")]
    public SolicitudTrabajoDeCampo? Solicitud
    {
        get => _solicitud;
        set => SetPropertyValue(nameof(Solicitud), ref _solicitud, value);
    }

    [XafDisplayName("Pedido de venta relacionado")]
    public PedidoVenta? PedidoVentaRelacionado
    {
        get => _pedidoVentaRelacionado;
        set => SetPropertyValue(nameof(PedidoVentaRelacionado), ref _pedidoVentaRelacionado, value);
    }

    [XafDisplayName("Referencia")]
    public string? Referencia
    {
        get => _referencia;
        set => SetPropertyValue(nameof(Referencia), ref _referencia, value);
    }

    [XafDisplayName("Fecha pedido")]
    public DateTime FechaPedido
    {
        get => _fechaPedido;
        set => SetPropertyValue(nameof(FechaPedido), ref _fechaPedido, value);
    }

    [Association("PedidoTC-ServiciosTC")]
    [XafDisplayName("Servicios")]
    public XPCollection<ServicioTrabajoDeCampo> Servicios => GetCollection<ServicioTrabajoDeCampo>(nameof(Servicios));

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaPedido = DateTime.Now;
    }
}
