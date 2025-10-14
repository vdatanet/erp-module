using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Contacts;

namespace erp.Module.BusinessObjects.Settings;

[DefaultClassOptions]
[NavigationItem("Settings")]
[ImageName("BO_MyDetails")]
[RuleObjectExists("CompanyInfoExists", DefaultContexts.Save, "True", InvertResult = true,
    CustomMessageTemplate = "Company Info already exists.")]
[RuleCriteria("NotDeleteCompanyInfo", DefaultContexts.Delete, "False",
    CustomMessageTemplate = "Can't delete Company Info.")]
public class CompanyInfo(Session session) : Contact(session)
{
    private Account _defaultSalesAccount;
    private Account _defaultPurchaseAccount;
    private string _defaultDailyTimeSheetPrefix;
    
    public Account DefaultSalesAccount
    {
        get => _defaultSalesAccount;
        set => SetPropertyValue(nameof(DefaultSalesAccount), ref _defaultSalesAccount, value);
    }
    
    public Account DefaultPurchaseAccount
    {
        get => _defaultPurchaseAccount;
        set => SetPropertyValue(nameof(DefaultPurchaseAccount), ref _defaultPurchaseAccount, value);
    }
    
    public string DefaultDailyTimeSheetPrefix
    {
        get => _defaultDailyTimeSheetPrefix;
        set => SetPropertyValue(nameof(DefaultDailyTimeSheetPrefix), ref _defaultDailyTimeSheetPrefix, value);
    }
}