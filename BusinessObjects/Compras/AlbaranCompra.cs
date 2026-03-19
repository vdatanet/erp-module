using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Compras;

namespace erp.Module.BusinessObjects.Compras;

[DefaultClassOptions]
[NavigationItem("Compras")]
[ImageName("BO_Order")]
public class AlbaranCompra(Session session) : DocumentoCompra(session);