using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects.Contacts;

[DefaultClassOptions]
[NavigationItem("Contacts")]
[ImageName("BO_Company")]
[RuleObjectExists("CompanyInfoExists", DefaultContexts.Save, "True", InvertResult = true,
    CustomMessageTemplate = "Company Info already exists.")]
[RuleCriteria("NotDeleteCompanyInfo", DefaultContexts.Delete, "False",
    CustomMessageTemplate = "Can't delete Company Info.")]
public class CompanyInfo(Session session) : Contact(session)
{
}