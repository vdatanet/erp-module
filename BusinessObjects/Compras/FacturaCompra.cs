using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Compras;

namespace erp.Module.BusinessObjects.Compras;

[DefaultClassOptions]
[NavigationItem("Compras")]
[ImageName("BO_Invoice")]
[DefaultProperty(nameof(Secuencia))]
public class FacturaCompra(Session session) : DocumentoCompra(session);