using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Products;

namespace erp.Module.BusinessObjects.Invoicing;

[ImageName("BO_Invoice")]
public class InvoiceLine(Session session): BaseEntity(session)
{
    private Invoice _invoice;
    private Invoice _invoiceAtDelete;
    private Product _product;
    private string _productName;
    private string _description;
    private string _notes;
    private decimal _quantity;
    private decimal _unitPrice;
    private decimal _discountPercent;
    private decimal _baseAmount;
    //private decimal _discount;
    //private decimal _tax;
    //private decimal _taxAmount;
    //private decimal _totalTaxAmount;
    //private decimal _totalAmountAfterDiscount;
    //private decimal _totalAmountAfterTax;
    //private decimal _totalAmountAfterDiscountAndTax;

    [Association("Invoice-InvoiceLines")]
    public Invoice Invoice
    {
        get => _invoice;
        set => SetPropertyValue(nameof(Invoice), ref _invoice, value);
    }
    
    public Product Product
    {
        get => _product;
        set => SetPropertyValue(nameof(Product), ref _product, value);
    }
    
    [Size(255)]
    public string ProductName
    {
        get => _productName;
        set => SetPropertyValue(nameof(ProductName), ref _productName, value);
    }
    
    [Size(1000)]
    public string Description
    {
        get => _description;    
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }

    [Size(1000)]
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
    public decimal DiscountPercent {
        get => _discountPercent;
        set {
            if (SetPropertyValue(nameof(DiscountPercent), ref _discountPercent, value))
                Recalculate();
        }
    }
    
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal BaseAmount {
        get => _baseAmount;
        set => SetPropertyValue(nameof(BaseAmount), ref _baseAmount, value);
    }

    public void Recalculate()
    {
        var gross = Quantity * UnitPrice;
        var discount = MoneyMath.RoundMoney(gross * (DiscountPercent / 100m));
        BaseAmount = MoneyMath.RoundMoney(gross - discount);

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

        //TaxAmount = RoundMoney(Taxes.Sum(tt => tt.Amount));
        //LineTotal = RoundMoney(BaseAmount + TaxAmount);

        // 3) Actualizar totales de la factura
        Invoice?.RecalculateTotals();
        //Invoice.TotalAmount    = Invoice.InvoiceLines.Sum(l => l.BaseAmount);
    }
    
    protected override void OnDeleting()
    {
        _invoiceAtDelete = Invoice;
        base.OnDeleting();
    }

    protected override void OnDeleted()
    {
        base.OnDeleted();
        _invoiceAtDelete?.RecalculateTotals();
        _invoiceAtDelete = null;
    }
}