using DevExpress.Xpo;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Products;

namespace erp.Module.BusinessObjects.Sales;

public class OrderLine(Session session): BaseEntity(session)
{
    private SalesOrder _salesOrder;
    private Product _product;
    private string _productName;
    private string _description;
    private string _notes;
    private decimal _quantity;
    private decimal _unitPrice; 
    private decimal _totalAmount;
    //private decimal _discount;
    //private decimal _tax;
    //private decimal _taxAmount;
    //private decimal _totalTaxAmount;
    //private decimal _totalAmountAfterDiscount;
    //private decimal _totalAmountAfterTax;
    //private decimal _totalAmountAfterDiscountAndTax;

    [Association("SalesOrder-OrderLines")]
    public SalesOrder SalesOrder
    {
        get => _salesOrder;
        set => SetPropertyValue(nameof(SalesOrder), ref _salesOrder, value);
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
    
    public decimal Quantity
    {
        get => _quantity;
        set => SetPropertyValue(nameof(Quantity), ref _quantity, value);
    }
    
    public decimal UnitPrice
    {
        get => _unitPrice;
        set => SetPropertyValue(nameof(UnitPrice), ref _unitPrice, value);
    }
    
    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetPropertyValue(nameof(TotalAmount), ref _totalAmount, value);
    }
}