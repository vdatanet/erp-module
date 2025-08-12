using System.ComponentModel;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Products;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Accounting;

[DefaultClassOptions]
[NavigationItem("Accounting")]
[ImageName("Top10Percent")]
[DefaultProperty(nameof(Code))]
public class TaxKind(Session session) : BaseEntity(session)
{
    private string _code;
    private string _name;
    private string _notes;
    private int _sequence;
    private Account _account;
    private decimal _rate;
    private bool _isActive;
    private bool _isAvailableInSales;
    private bool _isAvailableInPurchases;
    private bool _isWithHolding;
    private Impuesto _tax;
    private ClaveRegimen _taxScheme;
    private CalificacionOperacion _taxType;
    private CausaExencion _taxExemption;

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
    
    public int Sequence
    {
        get => _sequence;
        set => SetPropertyValue(nameof(Sequence), ref _sequence, value);
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

    public bool IsWithHolding
    {
        get => _isWithHolding;
        set => SetPropertyValue(nameof(IsWithHolding), ref _isWithHolding, value);
    }
    public Impuesto Tax
    {
        get => _tax;
        set => SetPropertyValue(nameof(Tax), ref _tax, value);
    }
    public ClaveRegimen TaxScheme
    {
        get => _taxScheme;
        set => SetPropertyValue(nameof(TaxScheme), ref _taxScheme, value);
    }
    
    public CalificacionOperacion TaxType
    {
        get => _taxType;
        set => SetPropertyValue(nameof(TaxType), ref _taxType, value);
    }
    
    public CausaExencion TaxExemption
    {
        get => _taxExemption;
        set => SetPropertyValue(nameof(TaxExemption), ref _taxExemption, value);
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
        IsWithHolding = false;
        Rate = 0;
        Account = null;
    }
}