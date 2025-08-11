using System.ComponentModel;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Crm;
using SequenceFactory = erp.Module.Factories.SequenceFactory;

namespace erp.Module.BusinessObjects.Sales;

[DefaultClassOptions]
[NavigationItem("Sales")]
[ImageName("BO_Order")]
[DefaultProperty(nameof(OrderNumber))]
[Appearance("OrderPrefixDisabled", AppearanceItemType = "ViewItem", TargetItems = "Prefix",
    Criteria = "This is not null and !IsNewObject(This)", Context = "DetailView", Enabled = false)]
[Appearance("OrderNumberDisabled", AppearanceItemType = "ViewItem", TargetItems = "OrderNumber",
    Criteria = "This is not null and !IsNewObject(This)", Context = "DetailView", Enabled = false)]
public class SalesOrder(Session session): BaseEntity(session)
{
    private string _prefix;
    private string _orderNumber;
    private DateTime _orderDate;
    private Lead _lead;
    private Customer _customer;
    private decimal _totalAmount;

    [RuleRequiredField]
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
    
    [RuleRequiredField]
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
    
    protected override void OnSaving()
    {
        base.OnSaving();
        
        if (!Session.IsNewObject(this) || !string.IsNullOrEmpty(OrderNumber) || Session is NestedUnitOfWork) return;
        OrderNumber = SequenceFactory.GetNextSequence(Session, $"{typeof(SalesOrder).FullName}.{Prefix}", Prefix, 5);
    }
}