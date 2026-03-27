using DevExpress.ExpressApp.DC;

namespace erp.Module.BusinessObjects.Productos;

public enum ContextoPrecio
{
    [XafDisplayName("Ambos")]
    Ambos = 0,
    [XafDisplayName("Compra")]
    Compra = 1,
    [XafDisplayName("Venta")]
    Venta = 2
}
