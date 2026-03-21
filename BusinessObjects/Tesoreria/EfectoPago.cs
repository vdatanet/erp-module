using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Compras;

namespace erp.Module.BusinessObjects.Tesoreria;

[DefaultClassOptions]
[NavigationItem("Tesorería")]
[XafDisplayName("Efecto de Pago")]
[ImageName("BO_Invoice")]
public class EfectoPago(Session session) : EfectoBase(session)
{
    private FacturaCompra? _facturaCompra;

    [Association("FacturaCompra-EfectosPago")]
    [XafDisplayName("Factura de Compra")]
    public FacturaCompra? FacturaCompra
    {
        get => _facturaCompra;
        set => SetPropertyValue(nameof(FacturaCompra), ref _facturaCompra, value);
    }
}
