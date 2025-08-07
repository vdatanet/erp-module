using System.ComponentModel;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Contacts;
using SequenceFactory = erp.Module.Factories.SequenceFactory;

namespace erp.Module.BusinessObjects.Invoicing;

[DefaultClassOptions]
[NavigationItem("Invoicing")]
[ImageName("BO_Invoice")]
[DefaultProperty(nameof(InvoiceNumber))]
[Appearance("InvoicePrefixDisabled", AppearanceItemType = "ViewItem", TargetItems = nameof(Prefix),
    Criteria = "This is not null and !IsNewObject(This)", Context = "DetailView", Enabled = false)]
[Appearance("InvoiceNumberDisabled", AppearanceItemType = "ViewItem", TargetItems = nameof(InvoiceNumber),
    Criteria = "This is not null and !IsNewObject(This)", Context = "DetailView", Enabled = false)]
public class Invoice(Session session): BaseEntity(session)
{
    private string _prefix;
    private string _invoiceNumber;
    private DateTime _orderDate;
    private Customer _customer;
    private decimal _totalAmount;

    [RuleRequiredField]
    public string Prefix
    {
        get => _prefix;
        set => SetPropertyValue(nameof(Prefix), ref _prefix, value); 
    }

    public string InvoiceNumber
    {
        get => _invoiceNumber;
        set => SetPropertyValue(nameof(InvoiceNumber), ref _invoiceNumber, value);
    }
    
    [RuleRequiredField]
    public DateTime OrderDate
    {
        get => _orderDate;
        set => SetPropertyValue(nameof(OrderDate), ref _orderDate, value);
    }
    
    [Association("Customer-Invoices")]
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
    
    //[Aggregated]
    //[Association("SalesOrder-OrderLines")]
    //public XPCollection<OrderLine> OrderLines => GetCollection<OrderLine>(nameof(OrderLines));
    
    protected override void OnSaving()
    {
        base.OnSaving();
        
        if (!Session.IsNewObject(this) || !string.IsNullOrEmpty(InvoiceNumber) || Session is NestedUnitOfWork) return;
        InvoiceNumber = SequenceFactory.GetNextSequence(Session, $"{typeof(Invoice).FullName}.{Prefix}", Prefix, 5);
    }
}