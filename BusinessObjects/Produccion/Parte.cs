using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;

namespace erp.Module.BusinessObjects.Produccion;

[DefaultClassOptions]
[NavigationItem("Producción")]
[ImageName("BO_Order")]
public class Parte(Session session) : DocumentoVenta(session);