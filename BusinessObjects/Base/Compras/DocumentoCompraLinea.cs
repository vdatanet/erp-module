using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Productos;
using erp.Module.Helpers.Comun;
using erp.Module.Helpers.Contactos;
using erp.Module.Services.Productos;

namespace erp.Module.BusinessObjects.Base.Compras;

public class DocumentoCompraLinea(Session session) : EntidadBase(session)
{
    private decimal _baseImponible;
    private decimal _cantidad;
    private string? _descripcion;
    private decimal _descuento1;
    private decimal _descuento2;
    private decimal _descuento3;
    private DocumentoCompra? _documentoCompra;
    private decimal _importeDescuento;
    private string? _notas;
    private decimal _precio;
    private Producto? _producto;
    private CuentaContable? _cuentaContable;
    private int _secuencia;
    private UnidadFacturacion? _unidadFacturacion;

    [Association("DocumentoCompra-DocumentoCompraLineas")]
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
    [XafDisplayName("% Descuento 1")]
    public decimal Descuento1
    {
        get => _descuento1;
        set
        {
            var modified = SetPropertyValue(nameof(Descuento1), ref _descuento1, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            EstablecerBaseImponible();
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
            EstablecerBaseImponible();
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
            EstablecerBaseImponible();
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
    public decimal Descuento
    {
        get => Descuento1;
        set => Descuento1 = value;
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
    [Association("DocumentoCompraLinea-DocumentoCompraLineaImpuestos")]
    [XafDisplayName("Impuestos")]
    public XPCollection<DocumentoCompraLineaImpuesto> Impuestos => GetCollection<DocumentoCompraLineaImpuesto>();

    private void OnProductoChanged()
    {
        if (Producto == null) return;
        Descripcion = Producto.Nombre;
        Precio = Producto.CosteEstandar;
        UnidadFacturacion = Producto.UnidadFacturacion;

        if (DocumentoCompra?.Proveedor != null)
        {
            var localTime = InformacionEmpresaHelper.GetLocalTime(Session);
            var precioEspecial = PrecioEspecialService.GetPrecioEspecialActivo(Producto, DocumentoCompra.Proveedor, ContextoPrecio.Compra, localTime);
            if (precioEspecial != null)
            {
                Precio = precioEspecial.Precio;
                Descuento1 = precioEspecial.Descuento1;
                Descuento2 = precioEspecial.Descuento2;
                Descuento3 = precioEspecial.Descuento3;
            }
        }

        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        CuentaContable = Producto.CuentaCompras ?? companyInfo?.CuentaComprasPorDefecto;
        UnidadFacturacion ??= companyInfo?.UnidadFacturacionPredeterminada;

        BorrarImpuestosProducto();
        foreach (var t in Producto.ImpuestosCompras)
        {
            var tax = new DocumentoCompraLineaImpuesto(Session)
            {
                DocumentoCompraLinea = this,
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
        BaseImponible = AmountCalculator.GetTaxableAmountCascading(Cantidad, Precio, Descuento1, Descuento2, Descuento3);
        ImporteDescuento = MoneyMath.RoundMoney(Cantidad * Precio - BaseImponible);
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