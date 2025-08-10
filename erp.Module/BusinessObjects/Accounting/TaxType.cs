using System.ComponentModel;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Products;

namespace erp.Module.BusinessObjects.Accounting;

[DefaultClassOptions]
[NavigationItem("Accounting")]
[ImageName("Top10Percent")]
[DefaultProperty(nameof(Code))]
public class TaxType(Session session): BaseEntity(session)
{
    private string _code;
    private string _name;
    private string _notes;
    private Account _account;
    private decimal _rate;
    private bool _isActive;
    private bool _isAvailableInSales;
    private bool _isAvailableInPurchases;
    private bool _isCompound;
    private bool _isWithHolding;
    
    [RuleRequiredField]
    [RuleUniqueValue]
    public string Code
    {
        get => _code;
        set => SetPropertyValue(nameof(Code), ref _code, value);   
    }
    
    [Size(255)]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    [Size(1000)]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }
    
    public Account Account
    {
        get => _account;
        set => SetPropertyValue(nameof(Account), ref _account, value);
    }
    
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal Rate
    {
        get => _rate;
        set => SetPropertyValue(nameof(Rate), ref _rate, value);
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
    
    public bool IsCompound
    {
        get => _isCompound;
        set => SetPropertyValue(nameof(IsCompound), ref _isCompound, value);
    }

    public bool IsWithHolding
    {
        get => _isWithHolding;
        set => SetPropertyValue(nameof(IsWithHolding), ref _isWithHolding, value);
    }
    
    [Association("Products-SalesTaxes")]
    public XPCollection<Product> ProductSalesTaxes => GetCollection<Product>(nameof(ProductSalesTaxes)); 
    
    [Association("Products-PurchaseTaxes")]
    public XPCollection<Product> ProductPurchaseTaxes => GetCollection<Product>(nameof(ProductPurchaseTaxes));
    
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
        IsCompound = false;
        IsWithHolding = false;
        Rate = 0;
        Account = null;
    }

}