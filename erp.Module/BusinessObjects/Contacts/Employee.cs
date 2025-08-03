using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects.Contacts;

[DefaultClassOptions]
[NavigationItem("Contacts")]
[ImageName("BO_Employee")]
public class Employee(Session session) : Contact(session)
{
}