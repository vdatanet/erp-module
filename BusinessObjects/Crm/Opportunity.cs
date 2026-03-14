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
    private string _descripcion;
    private Customer _cliente;
    private Campaign _campana;
    private Medium _medio;
    private Source _fuente;

    [Size(1000)]
    public string Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [Association("Customer-Opportunities")]
    public Customer Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }
    
    public Campaign Campana
    {
        get => _campana;
        set => SetPropertyValue(nameof(Campana), ref _campana, value);
    }
    
    public Medium Medio
    {
        get => _medio;
        set => SetPropertyValue(nameof(Medio), ref _medio, value);
    }
    
    public Source Fuente
    {
        get => _fuente;
        set => SetPropertyValue(nameof(Fuente), ref _fuente, value);
    }
    
    [Association("Opportunity-SalesOrders")]
    public XPCollection<SalesOrder> SalesOrders => GetCollection<SalesOrder>();
}