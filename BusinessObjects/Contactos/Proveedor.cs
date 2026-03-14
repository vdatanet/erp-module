using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Proveedor")]
public class Proveedor(Session session) : Tercero(session)
{
}