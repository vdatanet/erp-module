using System.ComponentModel;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Taxes;
using erp.Module.Helpers.Contacts;
using Task = erp.Module.BusinessObjects.Planning.Task;

namespace erp.Module.BusinessObjects.Products;

[DefaultClassOptions]
[NavigationItem("Products")]
[ImageName("BO_Product")]
[DefaultProperty(nameof(Codigo))]
public class Product(Session session) : BaseEntity(session)
{
    private string _codigo;
    private string _codigoBarras;
    private string _nombre;
    private Category _categoria;
    private decimal _costeEstandar;
    private decimal _precioVenta;
    private Account _cuentaVentas;
    private Account _cuentaCompras;
    private bool _estaActivo;
    private bool _disponibleEnVentas;
    private bool _disponibleEnCompras;
    private bool _disponibleEnTpv;
    private string _notas;
    private MediaDataObject _foto;
    
    [RuleUniqueValue]
    public string Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [RuleUniqueValue]
    public string CodigoBarras
    {
        get => _codigoBarras;
        set => SetPropertyValue(nameof(CodigoBarras), ref _codigoBarras, value);
    }

    [RuleUniqueValue]
    [RuleRequiredField]
    [Size(SizeAttribute.Unlimited)]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }
    
    [Association("Category-Products")]
    [DataSourceCriteria("EstaActivo = True")]
    public Category Categoria
    {
        get => _categoria;
        set => SetPropertyValue(nameof(Categoria), ref _categoria, value);
    }

    public decimal CosteEstandar
    {
        get => _costeEstandar;
        set => SetPropertyValue(nameof(CosteEstandar), ref _costeEstandar, value);
    }

    public decimal PrecioVenta
    {
        get => _precioVenta;
        set => SetPropertyValue(nameof(PrecioVenta), ref _precioVenta, value);
    }

    public Account CuentaVentas
    {
        get => _cuentaVentas;
        set => SetPropertyValue(nameof(CuentaVentas), ref _cuentaVentas, value);
    }

    public Account CuentaCompras
    {
        get => _cuentaCompras;
        set => SetPropertyValue(nameof(CuentaCompras), ref _cuentaCompras, value);
    }

    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    public bool DisponibleEnVentas
    {
        get => _disponibleEnVentas;
        set => SetPropertyValue(nameof(DisponibleEnVentas), ref _disponibleEnVentas, value);
    }
    
    public bool DisponibleEnCompras
    {
        get => _disponibleEnCompras;
        set => SetPropertyValue(nameof(DisponibleEnCompras), ref _disponibleEnCompras, value);
    }

    public bool DisponibleEnTpv
    {
        get => _disponibleEnTpv;
        set => SetPropertyValue(nameof(DisponibleEnTpv), ref _disponibleEnTpv, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    public MediaDataObject Foto
    {
        get => _foto;
        set => SetPropertyValue(nameof(Foto), ref _foto, value);
    }
    
    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("Products-SalesTaxes")]
    [DataSourceCriteria("IsAvailableInSales = True AND EstaActivo = True")]
    public XPCollection<TaxKind> SalesTaxes => GetCollection<TaxKind>(nameof(SalesTaxes));
    
    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("Products-PurchaseTaxes")]
    [DataSourceCriteria("IsAvailableInPurchases = True AND EstaActivo = True")]
    public XPCollection<TaxKind> PurchaseTaxes => GetCollection<TaxKind>(nameof(PurchaseTaxes));
    
    [Aggregated]
    [Association("Product-Tasks")]
    public XPCollection<Task> Tasks => GetCollection<Task>(nameof(Tasks)); 
    
    [Aggregated]
    [Association("Product-Pictures")]
    public XPCollection<Picture> Pictures => GetCollection<Picture>(nameof(Pictures));
    
    [Aggregated]
    [Association("Product-Attachments")]
    public XPCollection<Attachment> Attachments => GetCollection<Attachment>(nameof(Attachments));
    
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        EstaActivo = true;
        DisponibleEnVentas = false;
        DisponibleEnCompras = false;
        DisponibleEnTpv = false;
        var companyInfo = CompanyInfoHelper.GetCompanyInfo(Session);
        if (companyInfo == null) return;
        if (companyInfo.CuentaVentasPorDefecto != null) CuentaVentas = companyInfo.CuentaVentasPorDefecto;
        if (companyInfo.CuentaComprasPorDefecto != null) CuentaCompras = companyInfo.CuentaComprasPorDefecto;
    }
}