using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Contacts;

namespace erp.Module.BusinessObjects.Common;

[DefaultClassOptions]
[RuleObjectExists("CompanyInfoExists", DefaultContexts.Save, "True", InvertResult = true,
    CustomMessageTemplate = "Company Info already exists.")]
[RuleCriteria("NotDeleteCompanyInfo", DefaultContexts.Delete, "False",
    CustomMessageTemplate = "Can't delete Company Info.")]
public class CompanyInfo(Session session) : Contact(session)
{
}