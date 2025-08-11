using System.ComponentModel;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.Factories;

namespace erp.Module.BusinessObjects.Invoicing;

[DefaultClassOptions]
[NavigationItem("Invoicing")]
[ImageName("BO_Invoice")]
[DefaultProperty(nameof(InvoiceNumber))]
[Appearance("InvoicePrefixDisabled", AppearanceItemType = "ViewItem", TargetItems = nameof(InvoicePrefix),
    Criteria = "This is not null and !IsNewObject(This)", Context = "DetailView", Enabled = false)]
[Appearance("InvoiceNumberDisabled", AppearanceItemType = "ViewItem", TargetItems = nameof(InvoiceNumber),
    Criteria = "This is not null and !IsNewObject(This)", Context = "DetailView", Enabled = false)]
public class Invoice(Session session) : SalesDocument(session)
{
    private string _invoicePrefix;
    private string _invoiceNumber;
    private DateTime _invoiceDate;
    private Customer _customer;

    [RuleRequiredField]
    public string InvoicePrefix
    {
        get => _invoicePrefix;
        set => SetPropertyValue(nameof(InvoicePrefix), ref _invoicePrefix, value);
    }

    public string InvoiceNumber
    {
        get => _invoiceNumber;
        set => SetPropertyValue(nameof(InvoiceNumber), ref _invoiceNumber, value);
    }

    [RuleRequiredField]
    public DateTime InvoiceDate
    {
        get => _invoiceDate;
        set => SetPropertyValue(nameof(InvoiceDate), ref _invoiceDate, value);
    }

    [Association("Customer-Invoices")]
    public Customer Customer
    {
        get => _customer;
        set => SetPropertyValue(nameof(Customer), ref _customer, value);
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        if (!Session.IsNewObject(this) || !string.IsNullOrEmpty(InvoiceNumber) || Session is NestedUnitOfWork) return;
        InvoiceNumber =
            SequenceFactory.GetNextSequence(Session, $"{typeof(Invoice).FullName}.{InvoicePrefix}", InvoicePrefix, 5);
    }
}