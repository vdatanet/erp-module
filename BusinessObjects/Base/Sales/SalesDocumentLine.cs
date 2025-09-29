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
    //[Persistent(nameof(TaxableAmount))]
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
        set
        {
            var modified = SetPropertyValue(nameof(Product), ref _product, value);
            if (!modified || IsLoading || IsSaving) return;
            DeleteTaxes();
            ApplyProductSnapshot();
            RebuildTaxes(this);
        }
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
            var modified = SetPropertyValue(nameof(Quantity), ref _quantity, value);
            if (!modified || IsLoading || IsSaving) return;
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
            var modified = SetPropertyValue(nameof(UnitPrice), ref _unitPrice, value);
            if (!modified || IsLoading || IsSaving) return;
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
           var modified = SetPropertyValue(nameof(DiscountPercent), ref _discountPercent, value);
           if (!modified || IsLoading || IsSaving) return;
           SetTaxableAmount();
        }
    }

    //[PersistentAlias(nameof(_taxableAmount))]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal TaxableAmount
    {
        get => _taxableAmount;
        protected set => SetPropertyValue(nameof(TaxableAmount), ref _taxableAmount, value);
    }

    //protected set => SetPropertyValue(nameof(TaxableAmount), ref _taxableAmount, value);
    //[ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal TaxAmount
    {
        get => _taxAmount;
        protected set => SetPropertyValue(nameof(TaxAmount), ref _taxAmount, value);
    }

    //[ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal TotalAmount
    {
        get => _totalAmount;
        protected set => SetPropertyValue(nameof(TotalAmount), ref _totalAmount, value);
    }

    [Aggregated]
    [Association("SalesDocumentLine-Taxes")]
    public XPCollection<SalesDocumentLineTax> Taxes => GetCollection<SalesDocumentLineTax>();

    private void ApplyProductSnapshot()
    {
        if (Product is null)
        {
            ProductName = null;
            Notes = null;
            Quantity = 0;
            UnitPrice = 0m;
            DiscountPercent = 0m;
            return;
        }

        ProductName = Product.Name;
        Notes = Product.Notes;
        UnitPrice = Product.PriceList;

        if (Quantity == 0m)
            Quantity = 1m;

        foreach (var tax in Product.SalesTaxes.OrderBy(t => t.Sequence))
        {
            _ = new SalesDocumentLineTax(Session)
            {
                SalesDocumentLine = this,
                TaxKind = tax
            };
        }
    }

    private void SetTaxableAmount()
    {
        _taxableAmount = AmountCalculator.GetTaxableAmount(Quantity, UnitPrice, DiscountPercent);
    }

    private static void RebuildTaxes(SalesDocumentLine line)
    {
        foreach (var tax in line.Taxes)
        {
            tax.TaxableAmount = line.TaxableAmount;
            tax.TaxAmount =
                AmountCalculator.GetTaxAmount(tax.TaxableAmount, tax.TaxKind.Rate, tax.TaxKind.IsWithHolding);
        }

        line.TaxAmount = line.Taxes.Sum(t => t.TaxAmount);
        line.TotalAmount = line.TaxableAmount + line.TaxAmount;
    }

    private void DeleteTaxes()
    {
        for (var i = Taxes.Count - 1; i >= 0; i--)
            Taxes[i].Delete();
    }
}