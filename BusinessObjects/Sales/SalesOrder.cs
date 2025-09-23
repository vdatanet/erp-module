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
    private Opportunity _opportunity;

    [Association("Opportunity-SalesOrders")]
    public Opportunity Opportunity
    {
        get => _opportunity;
        set => SetPropertyValue(nameof(Opportunity), ref _opportunity, value);
    }
    
}