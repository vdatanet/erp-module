using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Productos;

namespace erp.Module.BusinessObjects.Inventario;

[DefaultClassOptions]
[NavigationItem("Inventario")]
[XafDisplayName("Stock Actual")]
[Persistent("StockActual")]
[DefaultProperty(nameof(Producto))]
public class StockActual(Session session) : EntidadBase(session)
{
    private Almacen? _almacen;
    private Producto? _producto;
    private decimal _cantidad;
    private decimal _reservado;

    [Association("Almacen-StockActual")]
    [XafDisplayName("Almacén")]
    public Almacen? Almacen
    {
        get => _almacen;
        set => SetPropertyValue(nameof(Almacen), ref _almacen, value);
    }

    [Association("Producto-StockActual")]
    [XafDisplayName("Producto")]
    public Producto? Producto
    {
        get => _producto;
        set => SetPropertyValue(nameof(Producto), ref _producto, value);
    }

    [XafDisplayName("Cantidad")]
    public decimal Cantidad
    {
        get => _cantidad;
        set => SetPropertyValue(nameof(Cantidad), ref _cantidad, value);
    }

    [XafDisplayName("Reservado")]
    public decimal Reservado
    {
        get => _reservado;
        set => SetPropertyValue(nameof(Reservado), ref _reservado, value);
    }

    [VisibleInDetailView(false)]
    [VisibleInListView(true)]
    [XafDisplayName("Disponible")]
    public decimal Disponible => Cantidad - Reservado;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
    }
}
