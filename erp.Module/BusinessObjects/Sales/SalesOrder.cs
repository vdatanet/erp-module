using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Crm;

namespace erp.Module.BusinessObjects.Sales;

[DefaultClassOptions]
[NavigationItem("Sales")]
[ImageName("BO_Order")]
[DefaultProperty(nameof(OrderNumber))]
public class SalesOrder(Session session): BaseEntity(session)
{
    private string _prefix;
    private string _orderNumber;
    private DateTime _orderDate;
    private Lead _lead;
    private Customer _customer;
    private decimal _totalAmount;

    public string Prefix
    {
        get => _prefix;
        set => SetPropertyValue(nameof(Prefix), ref _prefix, value); 
    }

    public string OrderNumber
    {
        get => _orderNumber;
        set => SetPropertyValue(nameof(OrderNumber), ref _orderNumber, value);
    }
    
    public DateTime OrderDate
    {
        get => _orderDate;
        set => SetPropertyValue(nameof(OrderDate), ref _orderDate, value);
    }
    
    [Association("Lead-SalesOrders")]
    public Lead Lead
    {
        get => _lead;
        set => SetPropertyValue(nameof(Lead), ref _lead, value);
    }
    
    [Association("Customer-SalesOrders")]
    public Customer Customer
    {
        get => _customer;
        set => SetPropertyValue(nameof(Customer), ref _customer, value);
    }
    
    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetPropertyValue(nameof(TotalAmount), ref _totalAmount, value);
    }
    
    [Aggregated]
    [Association("SalesOrder-OrderLines")]
    public XPCollection<OrderLine> OrderLines => GetCollection<OrderLine>(nameof(OrderLines));
}