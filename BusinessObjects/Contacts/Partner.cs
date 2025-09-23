using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects.Contacts;

[DefaultClassOptions]
[NavigationItem("Contacts")]
[ImageName("BO_Organization")]
public class Partner(Session session) : Contact(session)
{
}