using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Productos;

[DefaultClassOptions]
[NavigationItem("Productos")]
public class PrecioPorCantidad : EntidadBase
{
    private decimal _finIntervalo;

    private decimal _importeMinimo;

    private decimal _inicioIntervalo;

    private string? _observaciones;

    private decimal _precioEntrada;

    private decimal _precioUnitario;

    private Producto? _producto;

    public PrecioPorCantidad(Session session) : base(session)
    {
    }

    [Association("Producto-PreciosPorCantidad")]
    public Producto? Producto
    {
        get => _producto;
        set => SetPropertyValue(nameof(Producto), ref _producto, value);
    }

    public decimal InicioIntervalo
    {
        get => _inicioIntervalo;
        set => SetPropertyValue(nameof(InicioIntervalo), ref _inicioIntervalo, value);
    }

    public decimal FinIntervalo
    {
        get => _finIntervalo;
        set => SetPropertyValue(nameof(FinIntervalo), ref _finIntervalo, value);
    }

    public decimal PrecioUnitario
    {
        get => _precioUnitario;
        set => SetPropertyValue(nameof(PrecioUnitario), ref _precioUnitario, value);
    }

    public decimal ImporteMinimo
    {
        get => _importeMinimo;
        set => SetPropertyValue(nameof(ImporteMinimo), ref _importeMinimo, value);
    }

    public decimal PrecioEntrada
    {
        get => _precioEntrada;
        set => SetPropertyValue(nameof(PrecioEntrada), ref _precioEntrada, value);
    }

    [ModelDefault("PropertyEditorType", "DevExpress.ExpressApp.Win.Editors.MemoEditStringPropertyEditor")]
    [Size(4000)]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }
}