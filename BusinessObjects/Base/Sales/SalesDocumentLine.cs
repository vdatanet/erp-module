using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Helpers.Common;
using erp.Module.BusinessObjects.Products;

namespace erp.Module.BusinessObjects.Base.Sales;

[ImageName("BO_Order_Item")]
public class SalesDocumentLine(Session session) : BaseEntity(session)
{
    private SalesDocument _salesDocument;
    private Product _product;
    private string _productName;
    private string _notes;
    private decimal _quantity;
    private decimal _unitPrice;
    private decimal _discountPercent;
    private decimal _taxableAmount;
    private decimal _taxAmount;
    private decimal _totalAmount;
    
    [Association("SalesDocument-Lines")]
    public SalesDocument SalesDocument
    {
        get => _salesDocument;
        set => SetPropertyValue(nameof(SalesDocument), ref _salesDocument, value);
    }

    [ImmediatePostData]
    public Product Product
    {
        get => _product;
        set => SetPropertyValue(nameof(Product), ref _product, value);
    }
    
    [Size(SizeAttribute.Unlimited)]
    public string ProductName
    {
        get => _productName;
        set => SetPropertyValue(nameof(ProductName), ref _productName, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal Quantity
    {
        get => _quantity;
        set
        {
            if (SetPropertyValue(nameof(Quantity), ref _quantity, value))
                SetTaxableAmount();
        }
    }
    
    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal UnitPrice
    {
        get => _unitPrice;
        set
        {
            if (SetPropertyValue(nameof(UnitPrice), ref _unitPrice, value))
                SetTaxableAmount();
        }
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal DiscountPercent
    {
        get => _discountPercent;
        set
        {
            if (SetPropertyValue(nameof(DiscountPercent), ref _discountPercent, value))
                SetTaxableAmount();
        }
    }

    [ImmediatePostData]
    [ModelDefault("AllowEdit","False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal TaxableAmount
    {
        get => _taxableAmount;
        set => SetPropertyValue(nameof(TaxableAmount), ref _taxableAmount, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal TaxAmount
    {
        get => _taxAmount;
        set => SetPropertyValue(nameof(TaxAmount), ref _taxAmount, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetPropertyValue(nameof(TotalAmount), ref _totalAmount, value);
    }

    [Aggregated]
    [Association("SalesDocumentLine-Taxes")]
    public XPCollection<SalesDocumentLineTax> Taxes => GetCollection<SalesDocumentLineTax>();
    
    private void SetTaxableAmount()
    {
        if (IsLoading || IsSaving) 
            return;
        
        TaxableAmount = AmountCalculator.GetTaxableAmount(Quantity, UnitPrice, DiscountPercent);
    }
    
    // protected override void OnDeleted()
    // {
    //     base.OnDeleted();
    //     _previousSalesDocument?.RebuildTaxSummaryByTaxType();
    //     _previousSalesDocument = null;
    // }

    // protected override void OnDeleting()
    // {
    //     base.OnDeleting();
    //     _previousSalesDocument = SalesDocument;
    //     foreach (var aggregated in new ArrayList(Taxes)) Session.Delete(aggregated);
    // }
}