using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Helpers.Imprenta;

namespace erp.Module.BusinessObjects.Imprenta;

[DefaultClassOptions]
[NavigationItem("Imprenta")]
public class TrabajoImpresionMaterial(Session session) : DocumentoVentaLinea(session)
{
    private TrabajoImpresion? _trabajoImpresion;

    [Association("TrabajoImpresion-Materiales")]
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

    protected override void OnProductoChanged()
    {
        base.OnProductoChanged();
        if (Producto != null) TotalizarLinea();
    }

    protected override void OnCantidadChanged()
    {
        base.OnCantidadChanged();
        TotalizarLinea();
    }

    private void TotalizarLinea()
    {
        if (Producto != null)
        {
            var tramo = ImprentaHelper.BuscarTramoDePrecio(Producto, Cantidad);
            if (tramo != null) PrecioUnitario = ImprentaHelper.CalcularPrecioUnitario(Cantidad, tramo);
        }
    }
}