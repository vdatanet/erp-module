using System.ComponentModel;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
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
    private DateTime _invoceDate;
    private Customer _customer;
    private decimal _baseAmount;
    private decimal _taxAmount;
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
    public DateTime InvoiceDate
    {
        get => _invoceDate;
        set => SetPropertyValue(nameof(InvoiceDate), ref _invoceDate, value);
    }
    
    [Association("Customer-Invoices")]
    public Customer Customer
    {
        get => _customer;
        set => SetPropertyValue(nameof(Customer), ref _customer, value);
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
    [Association("Invoice-InvoiceLines")]
    public XPCollection<InvoiceLine> InvoiceLines => GetCollection<InvoiceLine>();
    
    public void RecalculateTotals()
    {
        if (IsLoading || Session?.IsObjectsLoading == true)
            return;
        
        BaseAmount = InvoiceLines.Sum(l => l.BaseAmount);
        TaxAmount = InvoiceLines.Sum(l => l.TaxAmount);
        TotalAmount = InvoiceLines.Sum(l => l.TotalAmount);
    }
    
    protected override void OnSaving()
    {
        base.OnSaving();
        
        foreach (var invoiceLine in InvoiceLines)
        {
            invoiceLine.Recalculate();
        }
        
        RecalculateTotals();
        
        if (!Session.IsNewObject(this) || !string.IsNullOrEmpty(InvoiceNumber) || Session is NestedUnitOfWork) return;
        InvoiceNumber = SequenceFactory.GetNextSequence(Session, $"{typeof(Invoice).FullName}.{Prefix}", Prefix, 5);
    }
}