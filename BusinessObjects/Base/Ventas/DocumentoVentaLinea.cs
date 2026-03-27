using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.BusinessObjects.Productos;
using erp.Module.Helpers.Comun;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Base.Ventas;

[ImageName("BO_Order_Item")]
public class DocumentoVentaLinea(Session session) : EntidadBase(session)
{
    private decimal _baseImponible;
    private decimal _cantidad;
    private DocumentoVenta? _documentoVenta;
    private decimal _importeImpuestos;
    private decimal _importeTotal;
    private string? _nombreProducto;
    private string? _notas;
    private decimal _descuento1;
    private decimal _descuento2;
    private decimal _descuento3;
    private decimal _precioUnitario;
    private Producto? _producto;
    private CuentaContable? _cuentaContable;
    private decimal _porcentajeComision;
    private decimal _importeComisionFijo;
    private DocumentoVentaGrupo? _grupo;
    private UnidadFacturacion? _unidadFacturacion;

    [Association("DocumentoVenta-Lineas")]
    [XafDisplayName("Documento Venta")]
    public DocumentoVenta? DocumentoVenta
    {
        get => _documentoVenta;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _documentoVenta, value);
    }

    [Association("DocumentoVentaGrupo-Lineas")]
    [XafDisplayName("Grupo")]
    public DocumentoVentaGrupo? Grupo
    {
        get => _grupo;
        set => SetPropertyValue(nameof(Grupo), ref _grupo, value);
    }

    [DataSourceCriteria("EstaActiva = True AND EsAsentable = True")]
    [XafDisplayName("Cuenta Contable")]
    public CuentaContable? CuentaContable
    {
        get => _cuentaContable;
        set => SetPropertyValue(nameof(CuentaContable), ref _cuentaContable, value);
    }

    [XafDisplayName("Unidad de Facturación")]
    public UnidadFacturacion? UnidadFacturacion
    {
        get => _unidadFacturacion;
        set => SetPropertyValue(nameof(UnidadFacturacion), ref _unidadFacturacion, value);
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
            AsignarProducto(value);
        }
    }

    /// <summary>
    /// Regla de negocio: Al asignar un producto se actualizan los datos de la línea 
    /// (nombre, precio, cuenta contable e impuestos) desde el producto.
    /// </summary>
    public virtual void AsignarProducto(Producto? value)
    {
        BorrarImpuestosProducto();

        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo != null)
        {
            CuentaContable = companyInfo.CuentaVentasPorDefecto;
            UnidadFacturacion = companyInfo.UnidadFacturacionPredeterminada;
            foreach (var tax in companyInfo.ImpuestosVentas.OrderBy(t => t.Secuencia))
                TiposImpuestoVenta.Add(tax);
        }

        if (value == null)
        {
            NombreProducto = null;
            Notas = null;
            PrecioUnitario = 0m;
            // No reseteamos cantidad ni descuento por defecto si se quita el producto, 
            // aunque se podría valorar.
            RecalcularYNotificar();
            return;
        }

        NombreProducto = value.Nombre;
        Notas = value.Notas;
        PrecioUnitario = value.PrecioVenta;

        if (DocumentoVenta?.Cliente != null)
        {
            var precioEspecial = value.PreciosEspeciales.FirstOrDefault(p => p.Tercero?.Oid == DocumentoVenta.Cliente.Oid);
            if (precioEspecial != null)
            {
                PrecioUnitario = precioEspecial.Precio;
                Descuento1 = precioEspecial.Descuento1;
                Descuento2 = precioEspecial.Descuento2;
                Descuento3 = precioEspecial.Descuento3;
            }
        }

        CuentaContable = value.CuentaVentas ?? CuentaContable;
        UnidadFacturacion = value.UnidadFacturacion ?? UnidadFacturacion;

        if (value.ImpuestosVentas.Count > 0)
        {
            BorrarImpuestosProducto();
            foreach (var tax in value.ImpuestosVentas.OrderBy(t => t.Secuencia))
                TiposImpuestoVenta.Add(tax);
        }

        if (Cantidad == 0m)
            Cantidad = 1m;

        RecalcularYNotificar();
        OnAsignarProductoFinished();
    }

    protected virtual void OnAsignarProductoFinished()
    {
    }

    protected virtual void OnCantidadChanged()
    {
    }

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
            RecalcularYNotificar();
            OnCantidadChanged();
        }
    }

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
            RecalcularYNotificar();
        }
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("% Descuento 1")]
    public decimal Descuento1
    {
        get => _descuento1;
        set
        {
            var modified = SetPropertyValue(nameof(Descuento1), ref _descuento1, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            RecalcularYNotificar();
            OnChanged(nameof(TextoDescuento));
        }
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("% Descuento 2")]
    public decimal Descuento2
    {
        get => _descuento2;
        set
        {
            var modified = SetPropertyValue(nameof(Descuento2), ref _descuento2, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            RecalcularYNotificar();
            OnChanged(nameof(TextoDescuento));
        }
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("% Descuento 3")]
    public decimal Descuento3
    {
        get => _descuento3;
        set
        {
            var modified = SetPropertyValue(nameof(Descuento3), ref _descuento3, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            RecalcularYNotificar();
            OnChanged(nameof(TextoDescuento));
        }
    }

    [XafDisplayName("Descuento")]
    [ToolTip("Ejemplo: 10+5+2")]
    [NonPersistent]
    public string? TextoDescuento
    {
        get => DiscountParser.Format(Descuento1, Descuento2, Descuento3);
        set
        {
            var (d1, d2, d3) = DiscountParser.Parse(value);
            Descuento1 = d1;
            Descuento2 = d2;
            Descuento3 = d3;
            OnChanged(nameof(TextoDescuento));
        }
    }

    [Obsolete("Use Descuento1")]
    [Browsable(false)]
    public decimal PorcentajeDescuento
    {
        get => Descuento1;
        set => Descuento1 = value;
    }

    [XafDisplayName("% Comisión")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal PorcentajeComision
    {
        get => _porcentajeComision;
        set => SetPropertyValue(nameof(PorcentajeComision), ref _porcentajeComision, value);
    }

    [XafDisplayName("Importe Fijo Comisión")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal ImporteComisionFijo
    {
        get => _importeComisionFijo;
        set => SetPropertyValue(nameof(ImporteComisionFijo), ref _importeComisionFijo, value);
    }

    [PersistentAlias("BaseImponible * (PorcentajeComision / 100) + ImporteComisionFijo")]
    [XafDisplayName("Comisión Calculada")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ComisionCalculada => Convert.ToDecimal(EvaluateAlias(nameof(ComisionCalculada)));

    [Persistent(nameof(BaseImponible))]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Base Imponible")]
    public decimal BaseImponible
    {
        get => _baseImponible;
        protected set => SetPropertyValue(nameof(BaseImponible), ref _baseImponible, value);
    }

    [Persistent(nameof(ImporteImpuestos))]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Impuestos")]
    public decimal ImporteImpuestos
    {
        get => _importeImpuestos;
        protected set => SetPropertyValue(nameof(ImporteImpuestos), ref _importeImpuestos, value);
    }

    [Persistent(nameof(ImporteTotal))]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Total")]
    public decimal ImporteTotal
    {
        get => _importeTotal;
        protected set => SetPropertyValue(nameof(ImporteTotal), ref _importeTotal, value);
    }

    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("DocumentoVentaLinea-TipoImpuestos")]
    [DataSourceCriteria("DisponibleEnVentas = True AND EstaActivo = True")]
    [XafDisplayName("Impuestos")]
    public XPCollection<TipoImpuesto> TiposImpuestoVenta => GetCollection<TipoImpuesto>();

    protected override XPCollection<T> CreateCollection<T>(XPMemberInfo property)
    {
        var collection = base.CreateCollection<T>(property);
        if (property.Name == nameof(TiposImpuestoVenta))
        {
            collection.CollectionChanged += TiposImpuestoVenta_CollectionChanged;
        }
        return collection;
    }

    [DevExpress.Xpo.Aggregated]
    [VisibleInDetailView(false)]
    [Association("DocumentoVentaLinea-Impuestos")]
    [XafDisplayName("Detalle Impuestos")]
    public XPCollection<DocumentoVentaLineaImpuesto> Impuestos => GetCollection<DocumentoVentaLineaImpuesto>();

    private void TiposImpuestoVenta_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
    {
        if (IsLoading || IsSaving || IsDeleted) return;
        RecalcularYNotificar();
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (DocumentoVenta?.Vendedor != null)
        {
            PorcentajeComision = DocumentoVenta.Vendedor.PorcentajeComision;
            ImporteComisionFijo = DocumentoVenta.Vendedor.ImporteComisionFijo;
        }

        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        CuentaContable ??= companyInfo.CuentaVentasPorDefecto;
        foreach (var tax in companyInfo.ImpuestosVentas.OrderBy(t => t.Secuencia)) 
            TiposImpuestoVenta.Add(tax);
    }

    private void RecalcularYNotificar()
    {
        EstablecerBaseImponible();
        ReconstruirImpuestos();
        DocumentoVenta?.InvalidadCacheTotales();
        DocumentoVenta?.RecalcularTotales();
    }

    private void EstablecerBaseImponible()
    {
        BaseImponible = AmountCalculator.GetTaxableAmountCascading(Cantidad, PrecioUnitario, Descuento1, Descuento2, Descuento3);
    }

    private void ReconstruirImpuestos()
    {
        for (var i = Impuestos.Count - 1; i >= 0; i--) Impuestos[i].Delete();

        foreach (var tax in TiposImpuestoVenta)
            _ = new DocumentoVentaLineaImpuesto(Session)
            {
                DocumentoVentaLinea = this,
                TipoImpuesto = tax,
                BaseImponible = BaseImponible,
                ImporteImpuestos = AmountCalculator.GetTaxAmount(BaseImponible, tax.Tipo, tax.EsRetencion)
            };

        ImporteImpuestos = Impuestos.Sum(t => t.ImporteImpuestos);
        ImporteTotal = BaseImponible + ImporteImpuestos;
    }

    private void BorrarImpuestosProducto()
    {
        var impuestosVentasToRemove = TiposImpuestoVenta.ToList();
        foreach (var tax in impuestosVentasToRemove) TiposImpuestoVenta.Remove(tax);
    }
}