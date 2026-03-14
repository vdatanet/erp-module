using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.BusinessObjects.Crm;

namespace erp.Module.BusinessObjects.Sales;

[DefaultClassOptions]
[NavigationItem("Sales")]
[ImageName("BO_Order")]
public class SalesOrder(Session session): SalesDocument(session)
{
    private Opportunity _oportunidad;

    [Association("Opportunity-SalesOrders")]
    public Opportunity Oportunidad
    {
        get => _oportunidad;
        set => SetPropertyValue(nameof(Oportunidad), ref _oportunidad, value);
    }
    
}