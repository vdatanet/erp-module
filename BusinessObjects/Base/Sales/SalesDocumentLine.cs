using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
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
        set
        {
            var modified = SetPropertyValue(nameof(Product), ref _product, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            DeleteProductTaxes();
            ApplyProductSnapshot();
            RebuildTaxes();
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
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
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
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
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
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            SetTaxableAmount();
        }
    }

    [Persistent(nameof(TaxableAmount))]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal TaxableAmount
    {
        get => _taxableAmount;
        protected set => SetPropertyValue(nameof(TaxableAmount), ref _taxableAmount, value);
    }

    [Persistent(nameof(TaxAmount))]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal TaxAmount
    {
        get => _taxAmount;
        protected set => SetPropertyValue(nameof(TaxAmount), ref _taxAmount, value);
    }

    [Persistent(nameof(TotalAmount))]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal TotalAmount
    {
        get => _totalAmount;
        protected set => SetPropertyValue(nameof(TotalAmount), ref _totalAmount, value);
    }

    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("SalesDocumentLines-TaxKinds")]
    [DataSourceCriteria("IsAvailableInSales = True AND IsActive = True")]
    public XPCollection<TaxKind> SalesTaxes
    {
        get
        {
            var collection = GetCollection<TaxKind>(nameof(SalesTaxes));
            if (!collection.IsLoaded)
            {
                collection.CollectionChanged += SalesTaxes_CollectionChanged;
            }
            return collection;
        }
    }

    private void SalesTaxes_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
    {
        if (IsLoading || IsSaving || IsDeleted) return;
        RebuildTaxes();
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
            SalesTaxes.Add(tax);
        }
    }

    private void SetTaxableAmount()
    {
        TaxableAmount = AmountCalculator.GetTaxableAmount(Quantity, UnitPrice, DiscountPercent);
        RebuildTaxes();
    }

    private void RebuildTaxes()
    {
        for (var i = Taxes.Count - 1; i >= 0; i--) Taxes[i].Delete();
        
        foreach (var tax in SalesTaxes)
        {
            _ = new SalesDocumentLineTax(Session)
            {
                SalesDocumentLine = this,
                TaxKind = tax,
                TaxableAmount = TaxableAmount,
                TaxAmount = AmountCalculator.GetTaxAmount(TaxableAmount, tax.Rate, tax.IsWithHolding)
            };
            
        }

        TaxAmount = Taxes.Sum(t => t.TaxAmount);
        TotalAmount = TaxableAmount + TaxAmount;
    }

    private void DeleteProductTaxes()
    { 
        var salesTaxesToRemove = SalesTaxes.ToList();
        foreach (var tax in salesTaxesToRemove)
        {
            SalesTaxes.Remove(tax);
        }
    }
}