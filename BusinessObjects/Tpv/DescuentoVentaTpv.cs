using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using System.ComponentModel;

namespace erp.Module.BusinessObjects.Tpv;

public enum TipoDescuentoTpv
{
    Fijo,
    Porcentaje
}

[XafDisplayName("Descuento de Venta TPV")]
[Persistent("DescuentoVentaTpv")]
public class DescuentoVentaTpv(Session session) : EntidadBase(session)
{
    private VentaTpv? _ventaTpv;
    private LineaVentaTpv? _lineaVentaTpv;
    private TipoDescuentoTpv _tipoDescuento;
    private decimal _valor;
    private string? _motivo;

    [XafDisplayName("Venta TPV")]
    [Association("VentaTpv-Descuentos")]
    public VentaTpv? VentaTpv
    {
        get => _ventaTpv;
        set => SetPropertyValue(nameof(VentaTpv), ref _ventaTpv, value);
    }

    [XafDisplayName("Línea de Venta TPV")]
    [Association("LineaVentaTpv-Descuentos")]
    public LineaVentaTpv? LineaVentaTpv
    {
        get => _lineaVentaTpv;
        set => SetPropertyValue(nameof(LineaVentaTpv), ref _lineaVentaTpv, value);
    }

    [XafDisplayName("Tipo de Descuento")]
    public TipoDescuentoTpv TipoDescuento
    {
        get => _tipoDescuento;
        set => SetPropertyValue(nameof(TipoDescuento), ref _tipoDescuento, value);
    }

    [XafDisplayName("Valor")]
    public decimal Valor
    {
        get => _valor;
        set => SetPropertyValue(nameof(Valor), ref _valor, value);
    }

    [XafDisplayName("Motivo")]
    [Size(255)]
    public string? Motivo
    {
        get => _motivo;
        set => SetPropertyValue(nameof(Motivo), ref _motivo, value);
    }
}
