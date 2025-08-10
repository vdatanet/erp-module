using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Products;

namespace erp.Module.BusinessObjects.Accounting;

[DefaultClassOptions]
[NavigationItem("Accounting")]
[ImageName("BO_Tax")]
public class TaxType(Session session): BaseEntity(session)
{
    private string _name;
    private string _description;
    private decimal _rate;
    private bool _isActive;
    
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }
    
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }
    
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
    
    [Association("Products-SalesTaxes")]
    public XPCollection<Product> ProductSalesTaxes => GetCollection<Product>(nameof(ProductSalesTaxes)); 
    
    [Association("Products-PurchaseTaxes")]
    public XPCollection<Product> ProductPurchaseTaxes => GetCollection<Product>(nameof(ProductPurchaseTaxes)); 

}