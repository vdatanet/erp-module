using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Invoicing;
using erp.Module.BusinessObjects.Sales;

namespace erp.Module.BusinessObjects.Contacts;

[DefaultClassOptions]
[NavigationItem("Contacts")]
[ImageName("BO_Customer")]
public class Customer(Session session) : Partner(session)
{
    [Association("Customer-Leads")]
    public XPCollection<Lead> Leads => GetCollection<Lead>(nameof(Leads));
    
    [Association("Customer-SalesOrders")]
    public XPCollection<SalesOrder> SalesOrders => GetCollection<SalesOrder>(nameof(SalesOrders));

    [Association("Customer-Invoices")]
    public XPCollection<Invoice> Invoices => GetCollection<Invoice>(nameof(Invoices));
}