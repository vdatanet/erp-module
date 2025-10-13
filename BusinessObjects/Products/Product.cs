using System.ComponentModel;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Helpers;
using erp.Module.BusinessObjects.Helpers.Contacts;
using Task = erp.Module.BusinessObjects.Common.Task;

namespace erp.Module.BusinessObjects.Products;

[DefaultClassOptions]
[NavigationItem("Products")]
[ImageName("BO_Product")]
[DefaultProperty(nameof(Code))]
public class Product(Session session) : BaseEntity(session)
{
    private string _code;
    private string _barcode;
    private string _name;
    private Category _category;
    private decimal _standardCost;
    private decimal _priceList;
    private Account _salesAccount;
    private Account _purchaseAccount;
    private bool _isActive;
    private bool _isAvailableInSales;
    private bool _isAvailableInPurchases;
    private bool _isAvailableInPos;
    private string _notes;
    private MediaDataObject _picture;
    
    [RuleUniqueValue]
    public string Code
    {
        get => _code;
        set => SetPropertyValue(nameof(Code), ref _code, value);
    }

    [RuleUniqueValue]
    public string Barcode
    {
        get => _barcode;
        set => SetPropertyValue(nameof(Barcode), ref _barcode, value);
    }

    [RuleUniqueValue]
    [RuleRequiredField]
    [Size(SizeAttribute.Unlimited)]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }
    
    [Association("Category-Products")]
    [DataSourceCriteria("IsActive = True")]
    public Category Category
    {
        get => _category;
        set => SetPropertyValue(nameof(Category), ref _category, value);
    }

    public decimal StandardCost
    {
        get => _standardCost;
        set => SetPropertyValue(nameof(StandardCost), ref _standardCost, value);
    }

    public decimal PriceList
    {
        get => _priceList;
        set => SetPropertyValue(nameof(PriceList), ref _priceList, value);
    }

    public Account SalesAccount
    {
        get => _salesAccount;
        set => SetPropertyValue(nameof(SalesAccount), ref _salesAccount, value);
    }

    public Account PurchaseAccount
    {
        get => _purchaseAccount;
        set => SetPropertyValue(nameof(PurchaseAccount), ref _purchaseAccount, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetPropertyValue(nameof(IsActive), ref _isActive, value);
    }

    public bool IsAvailableInSales
    {
        get => _isAvailableInSales;
        set => SetPropertyValue(nameof(IsAvailableInSales), ref _isAvailableInSales, value);
    }
    
    public bool IsAvailableInPurchases
    {
        get => _isAvailableInPurchases;
        set => SetPropertyValue(nameof(IsAvailableInPurchases), ref _isAvailableInPurchases, value);
    }

    public bool IsAvailableInPos
    {
        get => _isAvailableInPos;
        set => SetPropertyValue(nameof(IsAvailableInPos), ref _isAvailableInPos, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }

    public MediaDataObject Picture
    {
        get => _picture;
        set => SetPropertyValue(nameof(Picture), ref _picture, value);
    }
    
    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("Products-SalesTaxes")]
    [DataSourceCriteria("IsAvailableInSales = True AND IsActive = True")]
    public XPCollection<TaxKind> SalesTaxes => GetCollection<TaxKind>(nameof(SalesTaxes));
    
    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("Products-PurchaseTaxes")]
    [DataSourceCriteria("IsAvailableInPurchases = True AND IsActive = True")]
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
        IsActive = true;
        IsAvailableInSales = false;
        IsAvailableInPurchases = false;
        IsAvailableInPos = false;
        var companyInfo = CompanyInfoHelper.GetCompanyInfo(Session);
        if (companyInfo == null) return;
        if (companyInfo.DefaultSalesAccount != null) SalesAccount = companyInfo.DefaultSalesAccount;
        if (companyInfo.DefaultPurchaseAccount != null) PurchaseAccount = companyInfo.DefaultPurchaseAccount;
    }
}