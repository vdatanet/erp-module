using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Products;

namespace erp.Module.BusinessObjects.Base.Sales;

//[ImageName("BO_Invoice")]
//[DefaultProperty(nameof(Product))]
public abstract class SalesDocumentLine(Session session) : BaseEntity(session)
{
    private SalesDocument _salesDocument;
    private SalesDocument _documentAtDelete;
    private Product _product;
    private string _productName;
    private string _notes;
    private decimal _quantity;
    private decimal _unitPrice;
    private decimal _discountPercent;
    private decimal _baseAmount;
    private decimal _taxAmount;
    private decimal _totalAmount;
    
    [Association("SalesDocument-Lines")]
    public SalesDocument SalesDocument
    {
        get => _salesDocument;
        set => SetPropertyValue(nameof(SalesDocument), ref _salesDocument, value);
    }

    public Product Product
    {
        get => _product;
        set
        {
            if (SetPropertyValue(nameof(Product), ref _product, value))
                ApplyProductSnapshot(value);
        }
    }

    private void ApplyProductSnapshot(Product p)
    {
        if (IsLoading || IsSaving) return;

        if (p is null)
        {
            ProductName = null;
            Notes = null;
            UnitPrice = 0m;
            DiscountPercent = 0m;
            //DeleteAllTaxes();
            Recalculate();
            return;
        }

        ProductName = p.Name;
        Notes = p.Notes;
        UnitPrice = p.PriceList;

        if (Quantity == 0m) Quantity = 1m;

        //DeleteAllTaxes();

        //foreach (var tax in p.SalesTaxes)
        //{
            //var invoiceLineTax = new InvoiceLineTax(Session)
            //{
                //InvoiceLine = this,
                //TaxType = tax
            //};
        //}

        Recalculate();
        return;

        //void DeleteAllTaxes()
        //{
            //for (var i = Taxes.Count - 1; i >= 0; i--) Taxes[i].Delete();
        //}
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
                Recalculate();
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
                Recalculate();
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
                Recalculate();
        }
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal BaseAmount
    {
        get => _baseAmount;
        set => SetPropertyValue(nameof(BaseAmount), ref _baseAmount, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal TaxAmount
    {
        get => _taxAmount;
        set => SetPropertyValue(nameof(TaxAmount), ref _taxAmount, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetPropertyValue(nameof(TotalAmount), ref _totalAmount, value);
    }

    [Aggregated]
    [Association("SalesDocumentLine-Taxes")]
    public XPCollection<SalesDcoumentLineTax> Taxes => GetCollection<SalesDcoumentLineTax>();

    public void Recalculate()
    {
        if (IsLoading || IsSaving)
        {
            return;
        }
        
        var gross = Quantity * UnitPrice;
        var discount = MoneyMath.RoundMoney(gross * (DiscountPercent / 100m));
        BaseAmount = MoneyMath.RoundMoney(gross - discount);
        
        var runningTaxSum = 0m;

        //foreach (var tax in Taxes)
        //{
            //tax.TaxBase = BaseAmount;
            //tax.Amount = tax.TaxBase * (tax.Rate / 100m);
            //runningTaxSum += tax.Amount;
        //}

        // 2) Impuestos por línea (en orden)
        //decimal runningTaxSum = 0m;
        //foreach (var t in Taxes.OrderBy(t => t.Sequence).ThenBy(t => t.Oid))
        //{
        //var taxableBase = BaseAmount + (t.IsCompound ? runningTaxSum : 0m);
        //t.Base = RoundMoney(taxableBase);
        //var sign = t.IsWithholding ? -1m : 1m;
        //t.Amount = RoundMoney(t.Base * (t.Rate / 100m) * sign);
        //runningTaxSum += t.Amount;
        //}

        //TaxAmount = MoneyMath.RoundMoney(Taxes.Sum(tt => tt.Amount));
        //TotalAmount = MoneyMath.RoundMoney(BaseAmount + TaxAmount);

        // 3) Actualizar totales de la factura
        //Invoice?.RecalculateTotals();
        //Invoice.TotalAmount    = Invoice.InvoiceLines.Sum(l => l.BaseAmount);
    }

    protected override void OnDeleting()
    {
        //_invoiceAtDelete = Invoice;
        base.OnDeleting();
    }

    protected override void OnDeleted()
    {
        base.OnDeleted();
        //_invoiceAtDelete?.RecalculateTotals();
        //_invoiceAtDelete = null;
    }
}