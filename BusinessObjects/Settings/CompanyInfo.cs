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
    private string _tradeName;
    private Journal _defaultSalesJournal;
    private Journal _defaultPurchaseJournal;
    private Account _defaultSalesAccount;
    private Account _defaultPurchaseAccount;
    private Account _defaultCustomerAccount;
    private Account _defaultSupplierAccount;
    private string _defaultInvoicePrefix;
    private string _defaultBillPrefix;
    private string _defaultDailyTimeSheetPrefix;

    [Size(255)]
    public string TradeName
    {
        get => _tradeName;
        set => SetPropertyValue(nameof(TradeName), ref _tradeName, value);
    }

    [DataSourceCriteria("IsActive = True")]
    public Journal DefaultSalesJournal
    {
        get => _defaultSalesJournal;
        set => SetPropertyValue(nameof(DefaultSalesJournal), ref _defaultSalesJournal, value);
    }

    [DataSourceCriteria("IsActive = True")]
    public Journal DefaultPurchaseJournal
    {
        get => _defaultPurchaseJournal;
        set => SetPropertyValue(nameof(DefaultPurchaseJournal), ref _defaultPurchaseJournal, value);
    }

    [DataSourceCriteria("IsActive = True and IsPostable = True")]
    public Account DefaultSalesAccount
    {
        get => _defaultSalesAccount;
        set => SetPropertyValue(nameof(DefaultSalesAccount), ref _defaultSalesAccount, value);
    }

    [DataSourceCriteria("IsActive = True and IsPostable = True")]
    public Account DefaultPurchaseAccount
    {
        get => _defaultPurchaseAccount;
        set => SetPropertyValue(nameof(DefaultPurchaseAccount), ref _defaultPurchaseAccount, value);
    }

    [DataSourceCriteria("IsActive = True and IsPostable = True")]
    public Account DefaultCustomerAccount
    {
        get => _defaultCustomerAccount;
        set => SetPropertyValue(nameof(DefaultCustomerAccount), ref _defaultCustomerAccount, value);
    }

    [DataSourceCriteria("IsActive = True and IsPostable = True")]
    public Account DefaultSupplierAccount
    {
        get => _defaultSupplierAccount;
        set => SetPropertyValue(nameof(DefaultSupplierAccount), ref _defaultSupplierAccount, value);
    }

    public string DefaultInvoicePrefix
    {
        get => _defaultInvoicePrefix;
        set => SetPropertyValue(nameof(DefaultInvoicePrefix), ref _defaultInvoicePrefix, value);
    }

    public string DefaultBillPrefix
    {
        get => _defaultBillPrefix;
        set => SetPropertyValue(nameof(DefaultBillPrefix), ref _defaultBillPrefix, value);
    }

    public string DefaultDailyTimeSheetPrefix
    {
        get => _defaultDailyTimeSheetPrefix;
        set => SetPropertyValue(nameof(DefaultDailyTimeSheetPrefix), ref _defaultDailyTimeSheetPrefix, value);
    }
}