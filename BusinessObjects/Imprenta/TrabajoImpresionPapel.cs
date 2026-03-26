using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Helpers.Imprenta;

namespace erp.Module.BusinessObjects.Imprenta;

[DefaultClassOptions]
[NavigationItem("Imprenta")]
[XafDisplayName("Papel de Trabajo de Impresión")]
public class TrabajoImpresionPapel(Session session) : DocumentoVentaLinea(session)
{
    private decimal _alto;

    private decimal _ancho;

    private decimal _gramaje;

    private TamanoPapel? _tamanoPapel;
    private TrabajoImpresion? _trabajoImpresion;

    [Association("TrabajoImpresion-Papeles")]
    public TrabajoImpresion? TrabajoImpresion
    {
        get => _trabajoImpresion;
        set
        {
            if (SetPropertyValue(nameof(TrabajoImpresion), ref _trabajoImpresion, value) && !IsLoading && !IsSaving)
                if (value != null && DocumentoVenta != value)
                    DocumentoVenta = value;
        }
    }

    [ImmediatePostData]
    public TamanoPapel? TamanoPapel
    {
        get => _tamanoPapel;
        set
        {
            var modified = SetPropertyValue(nameof(TamanoPapel), ref _tamanoPapel, value);
            if (!IsLoading && !IsSaving && value != null && modified)
            {
                Ancho = value.Ancho;
                Alto = value.Alto;
            }
        }
    }

    [ImmediatePostData]
    public decimal Ancho
    {
        get => _ancho;
        set => SetAndRecalculate(nameof(Ancho), ref _ancho, value);
    }

    [ImmediatePostData]
    public decimal Alto
    {
        get => _alto;
        set => SetAndRecalculate(nameof(Alto), ref _alto, value);
    }

    [ImmediatePostData]
    public decimal Gramaje
    {
        get => _gramaje;
        set => SetAndRecalculate(nameof(Gramaje), ref _gramaje, value);
    }

    protected override void OnAsignarProductoFinished()
    {
        base.OnAsignarProductoFinished();
        if (Producto != null) TotalizarLinea();
    }

    private bool SetAndRecalculate<T>(string propertyName, ref T field, T value)
    {
        var modified = SetPropertyValue(propertyName, ref field, value);
        if (modified && !IsLoading && !IsSaving) TotalizarLinea();
        return modified;
    }

    private void TotalizarLinea()
    {
        if (Producto != null && DocumentoVenta is TrabajoImpresion trabajoImpresion)
        {
            var ctd = trabajoImpresion.Cantidad;
            var tramo = ImprentaHelper.BuscarTramoDePrecio(Producto, ctd);
            if (tramo != null) PrecioUnitario = ImprentaHelper.CalcularPrecioUnitario(ctd, tramo);
        }
    }
}