using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Ventas;

namespace erp.Module.BusinessObjects.Tesoreria;

[DefaultClassOptions]
[NavigationItem("Tesorería")]
[XafDisplayName("Efecto de Cobro")]
[ImageName("BO_Invoice")]
public class EfectoCobro(Session session) : EfectoBase(session)
{
    private FacturaVenta? _factura;

    [Association("FacturaVenta-EfectosCobro")]
    [XafDisplayName("Factura")]
    public FacturaVenta? Factura
    {
        get => _factura;
        set => SetPropertyValue(nameof(Factura), ref _factura, value);
    }
}
