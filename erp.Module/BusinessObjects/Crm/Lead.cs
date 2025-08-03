using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Sales;

namespace erp.Module.BusinessObjects.Crm;

[DefaultClassOptions]
[NavigationItem("Crm")]
[ImageName("BO_Lead")]
public class Lead(Session session) : Contact(session)
{
    private string _description;
    private Customer _customer;

    [Size(1000)]
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }

    [Association("Customer-Leads")]
    public Customer Customer
    {
        get => _customer;
        set => SetPropertyValue(nameof(Customer), ref _customer, value);
    }
    
    [Association("Lead-SalesOrders")]
    public XPCollection<SalesOrder> SalesOrders => GetCollection<SalesOrder>(nameof(SalesOrders));
}