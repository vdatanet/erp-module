using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Sales;

namespace erp.Module.BusinessObjects.Crm;

[DefaultClassOptions]
[NavigationItem("Crm")]
[ImageName("BO_Opportunity")]
public class Opportunity(Session session) : Contact(session)
{
    private string _description;
    private Customer _customer;
    private Campaign _campaign;
    private Medium _medium;
    private Source _source;

    [Size(1000)]
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }

    [Association("Customer-Opportunities")]
    public Customer Customer
    {
        get => _customer;
        set => SetPropertyValue(nameof(Customer), ref _customer, value);
    }
    
    public Campaign Campaign
    {
        get => _campaign;
        set => SetPropertyValue(nameof(Campaign), ref _campaign, value);
    }
    
    public Medium Medium
    {
        get => _medium;
        set => SetPropertyValue(nameof(Medium), ref _medium, value);
    }
    
    public Source Source
    {
        get => _source;
        set => SetPropertyValue(nameof(Source), ref _source, value);
    }
    
    [Association("Opportunity-SalesOrders")]
    public XPCollection<SalesOrder> SalesOrders => GetCollection<SalesOrder>();
}