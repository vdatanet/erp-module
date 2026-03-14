using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Productos;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.Helpers.Comun;

namespace erp.Module.BusinessObjects.Base.Ventas;

[ImageName("BO_Order_Item")]
public class LineaDocumentoVenta(Session session) : EntidadBase(session)
{
    private DocumentoVenta _documentoVenta;
    private Producto _producto;
    private string _nombreProducto;
    private string _notas;
    private decimal _cantidad;
    private decimal _precioUnitario;
    private decimal _porcentajeDescuento;
    private decimal _baseImponible;
    private decimal _importeImpuestos;
    private decimal _importeTotal;

    [Association("DocumentoVenta-Lines")]
    public DocumentoVenta DocumentoVenta
    {
        get => _documentoVenta;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _documentoVenta, value);
    }

    [ImmediatePostData]
    [LookupEditorMode(LookupEditorMode.Search)]
    public Producto Producto
    {
        get => _producto;
        set
        {
            var modified = SetPropertyValue(nameof(Producto), ref _producto, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            BorrarImpuestosProducto();
            AplicarInstantaneaProducto();
            ReconstruirImpuestos();
        }
    }

    [Size(SizeAttribute.Unlimited)]
    public string NombreProducto
    {
        get => _nombreProducto;
        set => SetPropertyValue(nameof(NombreProducto), ref _nombreProducto, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal Cantidad
    {
        get => _cantidad;
        set
        {
            var modified = SetPropertyValue(nameof(Cantidad), ref _cantidad, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            EstablecerBaseImponible();
        }
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
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
    public decimal BaseImponible
    {
        get => _baseImponible;
        protected set => SetPropertyValue(nameof(BaseImponible), ref _baseImponible, value);
    }

    [Persistent(nameof(ImporteImpuestos))]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal ImporteImpuestos
    {
        get => _importeImpuestos;
        protected set => SetPropertyValue(nameof(ImporteImpuestos), ref _importeImpuestos, value);
    }

    [Persistent(nameof(ImporteTotal))]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal ImporteTotal
    {
        get => _importeTotal;
        protected set => SetPropertyValue(nameof(ImporteTotal), ref _importeTotal, value);
    }

    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("LineaDocumentoVentas-TipoImpuestos")]
    [DataSourceCriteria("DisponibleEnVentas = True AND EstaActivo = True")]
    public XPCollection<TipoImpuesto> TiposImpuestoVenta
    {
        get
        {
            var collection = GetCollection<TipoImpuesto>(nameof(TiposImpuestoVenta));
            if (!collection.IsLoaded)
            {
                collection.CollectionChanged += TiposImpuestoVenta_CollectionChanged;
            }
            return collection;
        }
    }

    private void TiposImpuestoVenta_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
    {
        if (IsLoading || IsSaving || IsDeleted) return;
        ReconstruirImpuestos();
    }

    [Aggregated]
    [VisibleInDetailView(false)]
    [Association("LineaDocumentoVenta-Taxes")]
    public XPCollection<ImpuestoLineaDocumentoVenta> Impuestos => GetCollection<ImpuestoLineaDocumentoVenta>();

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

        foreach (var tax in Producto.SalesTaxes.OrderBy(t => t.Secuencia))
        {
            TiposImpuestoVenta.Add(tax);
        }
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
        {
            _ = new ImpuestoLineaDocumentoVenta(Session)
            {
                LineaDocumentoVenta = this,
                TipoImpuesto = tax,
                BaseImponible = BaseImponible,
                ImporteImpuestos = AmountCalculator.GetTaxAmount(BaseImponible, tax.Tipo, tax.EsRetencion)
            };
            
        }

        ImporteImpuestos = Impuestos.Sum(t => t.ImporteImpuestos);
        ImporteTotal = BaseImponible + ImporteImpuestos;
        
        if (DocumentoVenta is null) return;
        
        DocumentoVenta.BorrarResumenImpuestos();
        DocumentoVenta.ReconstruirResumenImpuestos();
    }

    private void BorrarImpuestosProducto()
    { 
        var salesTaxesToRemove = TiposImpuestoVenta.ToList();
        foreach (var tax in salesTaxesToRemove)
        {
            TiposImpuestoVenta.Remove(tax);
        }
    }
}