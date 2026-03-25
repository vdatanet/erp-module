using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Helpers.Imprenta;

namespace erp.Module.BusinessObjects.Imprenta;

[DefaultClassOptions]
[NavigationItem("Imprenta")]
public class TrabajoImpresionServicio(Session session) : DocumentoVentaLinea(session)
{
    private decimal _numEntradasMaq;

    private decimal _precio;

    private decimal _precioEntrada;
    private TrabajoImpresion? _trabajoImpresion;

    [Association("TrabajoImpresion-Servicios")]
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
    public decimal Precio
    {
        get => _precio;
        set => SetAndRecalculate(nameof(Precio), ref _precio, value);
    }

    [ImmediatePostData]
    public decimal NumEntradasMaq
    {
        get => _numEntradasMaq;
        set => SetAndRecalculate(nameof(NumEntradasMaq), ref _numEntradasMaq, value, true);
    }

    [ImmediatePostData]
    public decimal PrecioEntrada
    {
        get => _precioEntrada;
        set => SetAndRecalculate(nameof(PrecioEntrada), ref _precioEntrada, value);
    }

    protected override void OnAsignarProductoFinished()
    {
        base.OnAsignarProductoFinished();
        if (Producto != null) TotalizarLinea();
    }

    protected override void OnCantidadChanged()
    {
        base.OnCantidadChanged();
        TotalizarLinea();
    }

    private bool SetAndRecalculate<T>(string propertyName, ref T field, T value, bool buscarTramo = false)
    {
        var modified = SetPropertyValue(propertyName, ref field, value);
        if (modified && !IsLoading && !IsSaving)
        {
            if (buscarTramo) TotalizarLinea();
            else TotalizarLineaSinCambiarPrecio();
        }

        return modified;
    }

    private void TotalizarLinea()
    {
        if (Producto != null && Cantidad != 0)
        {
            var tramo = ImprentaHelper.BuscarTramoDePrecio(Producto, Cantidad);
            if (tramo != null)
            {
                Precio = ImprentaHelper.CalcularPrecioUnitario(Cantidad, tramo);
                PrecioEntrada = tramo.PrecioEntrada;
            }
        }

        TotalizarLineaSinCambiarPrecio();
    }

    private void TotalizarLineaSinCambiarPrecio()
    {
        if (Cantidad != 0)
            PrecioUnitario = (NumEntradasMaq * PrecioEntrada + Cantidad * Precio * NumEntradasMaq) / Cantidad;
        else
            PrecioUnitario = 0;
    }
}