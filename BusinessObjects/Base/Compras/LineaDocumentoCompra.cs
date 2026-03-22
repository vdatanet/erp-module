using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Productos;
using erp.Module.Helpers.Comun;

namespace erp.Module.BusinessObjects.Base.Compras;

public class LineaDocumentoCompra(Session session) : EntidadBase(session)
{
    private decimal _baseImponible;
    private decimal _cantidad;
    private string? _descripcion;
    private decimal _descuento;
    private DocumentoCompra? _documentoCompra;
    private decimal _importeDescuento;
    private string? _notas;
    private decimal _precio;
    private Producto? _producto;
    private int _secuencia;

    [Association("DocumentoCompra-Lineas")]
    [XafDisplayName("Documento Compra")]
    public DocumentoCompra? DocumentoCompra
    {
        get => _documentoCompra;
        set
        {
            var modified = SetPropertyValue(nameof(DocumentoCompra), ref _documentoCompra, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            EstablecerBaseImponible();
        }
    }

    [XafDisplayName("Secuencia")]
    public int Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    [ImmediatePostData]
    [XafDisplayName("Producto")]
    public Producto? Producto
    {
        get => _producto;
        set
        {
            var modified = SetPropertyValue(nameof(Producto), ref _producto, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            OnProductoChanged();
        }
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripción")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Cantidad")]
    public decimal Cantidad
    {
        get => _cantidad;
        set
        {
            var modified = SetPropertyValue(nameof(Cantidad), ref _cantidad, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            OnCantidadChanged();
        }
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Precio")]
    public decimal Precio
    {
        get => _precio;
        set
        {
            var modified = SetPropertyValue(nameof(Precio), ref _precio, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            EstablecerBaseImponible();
        }
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Descuento %")]
    public decimal Descuento
    {
        get => _descuento;
        set
        {
            var modified = SetPropertyValue(nameof(Descuento), ref _descuento, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            EstablecerBaseImponible();
        }
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Importe Descuento")]
    public decimal ImporteDescuento
    {
        get => _importeDescuento;
        set => SetPropertyValue(nameof(ImporteDescuento), ref _importeDescuento, value);
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Base Imponible")]
    public decimal BaseImponible
    {
        get => _baseImponible;
        set
        {
            var modified = SetPropertyValue(nameof(BaseImponible), ref _baseImponible, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            ReconstruirImpuestos();
        }
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [DevExpress.Xpo.Aggregated]
    [Association("LineaDocumentoCompra-Impuestos")]
    [XafDisplayName("Impuestos")]
    public XPCollection<ImpuestoLineaDocumentoCompra> Impuestos => GetCollection<ImpuestoLineaDocumentoCompra>();

    private void OnProductoChanged()
    {
        if (Producto == null) return;
        Descripcion = Producto.Nombre;
        Precio = Producto.CosteEstandar;
        BorrarImpuestosProducto();
        foreach (var t in Producto.ImpuestosCompras)
        {
            var tax = new ImpuestoLineaDocumentoCompra(Session)
            {
                LineaDocumentoCompra = this,
                TipoImpuesto = t
            };
            Impuestos.Add(tax);
        }

        EstablecerBaseImponible();
    }

    private void OnCantidadChanged()
    {
        EstablecerBaseImponible();
    }

    private void EstablecerBaseImponible()
    {
        ImporteDescuento = MoneyMath.RoundMoney(Cantidad * Precio * (Descuento / 100));
        BaseImponible = MoneyMath.RoundMoney(Cantidad * Precio - ImporteDescuento);
    }

    private void ReconstruirImpuestos()
    {
        foreach (var tax in Impuestos)
        {
            tax.BaseImponible = BaseImponible;
            tax.ImporteImpuestos = AmountCalculator.GetTaxAmount(BaseImponible, tax.Tipo, tax.EsRetencion);
        }

        DocumentoCompra?.ReconstruirResumenImpuestos();
    }

    private void BorrarImpuestosProducto()
    {
        for (var i = Impuestos.Count - 1; i >= 0; i--)
            Impuestos[i].Delete();
    }
}