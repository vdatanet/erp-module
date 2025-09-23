using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Sales;

namespace erp.Module.BusinessObjects.Sales;

[DefaultClassOptions]
[NavigationItem("Sales")]
[ImageName("BO_Order")]
public class Quote(Session session): SalesDocument(session)
{
    
}