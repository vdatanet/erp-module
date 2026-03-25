using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Base.Ventas;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[NavigationItem("Ventas")]
[XafDisplayName("Comisión")]
[ImageName("BO_Lead")]
public class Comision(Session session) : EntidadBase(session)
{
    private DocumentoVentaLinea? _documentoVentaLinea;
    private LiquidacionComision? _liquidacion;
    private decimal _importe;

    [XafDisplayName("Línea de Venta")]
    public DocumentoVentaLinea? DocumentoVentaLinea
    {
        get => _documentoVentaLinea;
        set => SetPropertyValue(nameof(DocumentoVentaLinea), ref _documentoVentaLinea, value);
    }

    [XafDisplayName("Liquidación")]
    [Association("Liquidacion-Comisiones")]
    public LiquidacionComision? Liquidacion
    {
        get => _liquidacion;
        set => SetPropertyValue(nameof(Liquidacion), ref _liquidacion, value);
    }

    [XafDisplayName("Importe")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    public decimal Importe
    {
        get => _importe;
        set => SetPropertyValue(nameof(Importe), ref _importe, value);
    }
}
