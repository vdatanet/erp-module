using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.BusinessObjects.Productos;
using erp.Module.Helpers.Comun;

namespace erp.Module.BusinessObjects.Base.Ventas;

[ImageName("BO_Order_Item")]
public class LineaDocumentoVenta(Session session) : EntidadBase(session)
{
    private decimal _baseImponible;
    private decimal _cantidad;
    private DocumentoVenta? _documentoVenta;
    private decimal _importeImpuestos;
    private decimal _importeTotal;
    private string? _nombreProducto;
    private string? _notas;
    private decimal _porcentajeDescuento;
    private decimal _precioUnitario;
    private Producto? _producto;

    [Association("DocumentoVenta-Lineas")]
    [XafDisplayName("Documento Venta")]
    public DocumentoVenta? DocumentoVenta
    {
        get => _documentoVenta;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _documentoVenta, value);
    }

    [ImmediatePostData]
    [LookupEditorMode(LookupEditorMode.Search)]
    [XafDisplayName("Producto")]
    public Producto? Producto
    {
        get => _producto;
        set
        {
            var modified = SetPropertyValue(nameof(Producto), ref _producto, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            BorrarImpuestosProducto();
            AplicarInstantaneaProducto();
            ReconstruirImpuestos();
            OnProductoChanged();
        }
    }

    protected virtual void OnProductoChanged() { }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Nombre Producto")]
    public string? NombreProducto
    {
        get => _nombreProducto;
        set => SetPropertyValue(nameof(NombreProducto), ref _nombreProducto, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
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
            EstablecerBaseImponible();
            OnCantidadChanged();
        }
    }

    protected virtual void OnCantidadChanged() { }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Precio Unitario")]
    public decimal PrecioUnitario
    {
        get => _precioUnitario;
        set
        {
            var modified = SetPropertyValue(nameof(PrecioUnitario), ref _precioUnitario, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            EstablecerBaseImponible();
        }
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("% Descuento")]
    public decimal PorcentajeDescuento
    {
        get => _porcentajeDescuento;
        set
        {
            var modified = SetPropertyValue(nameof(PorcentajeDescuento), ref _porcentajeDescuento, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            EstablecerBaseImponible();
        }
    }

    [Persistent(nameof(BaseImponible))]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Base Imponible")]
    public decimal BaseImponible
    {
        get => _baseImponible;
        protected set => SetPropertyValue(nameof(BaseImponible), ref _baseImponible, value);
    }

    [Persistent(nameof(ImporteImpuestos))]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Impuestos")]
    public decimal ImporteImpuestos
    {
        get => _importeImpuestos;
        protected set => SetPropertyValue(nameof(ImporteImpuestos), ref _importeImpuestos, value);
    }

    [Persistent(nameof(ImporteTotal))]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Total")]
    public decimal ImporteTotal
    {
        get => _importeTotal;
        protected set => SetPropertyValue(nameof(ImporteTotal), ref _importeTotal, value);
    }

    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("LineaDocumentoVenta-TipoImpuestos")]
    [DataSourceCriteria("DisponibleEnVentas = True AND EstaActivo = True")]
    [XafDisplayName("Impuestos")]
    public XPCollection<TipoImpuesto> TiposImpuestoVenta
    {
        get
        {
            var collection = GetCollection<TipoImpuesto>();
            if (!collection.IsLoaded) collection.CollectionChanged += TiposImpuestoVenta_CollectionChanged;
            return collection;
        }
    }

    [DevExpress.Xpo.Aggregated]
    [VisibleInDetailView(false)]
    [Association("LineaDocumentoVenta-Impuestos")]
    [XafDisplayName("Detalle Impuestos")]
    public XPCollection<ImpuestoLineaDocumentoVenta> Impuestos => GetCollection<ImpuestoLineaDocumentoVenta>();

    private void TiposImpuestoVenta_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
    {
        if (IsLoading || IsSaving || IsDeleted) return;
        ReconstruirImpuestos();
    }

    private void AplicarInstantaneaProducto()
    {
        if (Producto is null)
        {
            NombreProducto = null;
            Notas = null;
            Cantidad = 0;
            PrecioUnitario = 0m;
            PorcentajeDescuento = 0m;
            return;
        }

        NombreProducto = Producto.Nombre;
        Notas = Producto.Notas;
        PrecioUnitario = Producto.PrecioVenta;

        if (Cantidad == 0m)
            Cantidad = 1m;

        foreach (var tax in Producto.SalesTaxes.OrderBy(t => t.Secuencia)) TiposImpuestoVenta.Add(tax);
    }

    private void EstablecerBaseImponible()
    {
        BaseImponible = AmountCalculator.GetTaxableAmount(Cantidad, PrecioUnitario, PorcentajeDescuento);
        ReconstruirImpuestos();
    }

    private void ReconstruirImpuestos()
    {
        for (var i = Impuestos.Count - 1; i >= 0; i--) Impuestos[i].Delete();

        foreach (var tax in TiposImpuestoVenta)
            _ = new ImpuestoLineaDocumentoVenta(Session)
            {
                LineaDocumentoVenta = this,
                TipoImpuesto = tax,
                BaseImponible = BaseImponible,
                ImporteImpuestos = AmountCalculator.GetTaxAmount(BaseImponible, tax.Tipo, tax.EsRetencion)
            };

        ImporteImpuestos = Impuestos.Sum(t => t.ImporteImpuestos);
        ImporteTotal = BaseImponible + ImporteImpuestos;

        if (DocumentoVenta is null) return;

        DocumentoVenta.BorrarResumenImpuestos();
        DocumentoVenta.ReconstruirResumenImpuestos();
    }

    private void BorrarImpuestosProducto()
    {
        var salesTaxesToRemove = TiposImpuestoVenta.ToList();
        foreach (var tax in salesTaxesToRemove) TiposImpuestoVenta.Remove(tax);
    }
}