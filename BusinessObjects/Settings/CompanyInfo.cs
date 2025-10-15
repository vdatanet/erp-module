using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Taxes;

namespace erp.Module.BusinessObjects.Settings;

[DefaultClassOptions]
[NavigationItem("Settings")]
[ImageName("Actions_Settings")]
[RuleObjectExists("CompanyInfoExists", DefaultContexts.Save, "True", InvertResult = true,
    CustomMessageTemplate = "Company Info already exists.")]
[RuleCriteria("NotDeleteCompanyInfo", DefaultContexts.Delete, "False",
    CustomMessageTemplate = "Can't delete Company Info.")]
public class CompanyInfo(Session session) : Contact(session)
{
    private Journal _defaultSalesJournal;
    private Journal _defaultPurchaseJournal;
    private Account _defaultSalesAccount;
    private Account _defaultPurchaseAccount;
    private Account _defaultCustomerAccount;
    private Account _defaultSupplierAccount;
    private string _defaultInvoicePrefix;
    private string _defaultBillPrefix;
    private string _defaultDailyTimeSheetPrefix;
    private string _veriFactuConfigFileName;
    private string _veriFactuCertificateSerial;
    private string _veriFactuEndPointPrefix;
    private string _veriFactuEndPointValidatePrefix;
    private string _veriFactuSystemName;
    private string _veriFactuSystemVersion;
    private string _veriFactuSystemAdministratorName;
    private string _veriFactuSystemAdministratorFiscalNumber;

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

    public string VeriFactuConfigFileName
    {
        get => _veriFactuConfigFileName;
        set => SetPropertyValue(nameof(VeriFactuConfigFileName), ref _veriFactuConfigFileName, value);
    }

    public string VeriFactuCertificateSerial
    {
        get => _veriFactuCertificateSerial;
        set => SetPropertyValue(nameof(VeriFactuCertificateSerial), ref _veriFactuCertificateSerial, value);
    }
    
    public string VeriFactuEndPointPrefix
    {
        get => _veriFactuEndPointPrefix;
        set => SetPropertyValue(nameof(VeriFactuEndPointPrefix), ref _veriFactuEndPointPrefix, value);
    }
    
    public string VeriFactuEndPointValidatePrefix
    {
        get => _veriFactuEndPointValidatePrefix;
        set => SetPropertyValue(nameof(VeriFactuEndPointValidatePrefix), ref _veriFactuEndPointValidatePrefix, value);
    }

    public string VeriFactuSystemName
    {
        get => _veriFactuSystemName;
        set => SetPropertyValue(nameof(VeriFactuSystemName), ref _veriFactuSystemName, value);
    }
    
    public string VeriFactuSystemVersion
    {
        get => _veriFactuSystemVersion;
        set => SetPropertyValue(nameof(VeriFactuSystemVersion), ref _veriFactuSystemVersion, value);
    }
    
    public string VeriFactuSystemAdministratorName
    {
        get => _veriFactuSystemAdministratorName;
        set => SetPropertyValue(nameof(VeriFactuSystemAdministratorName), ref _veriFactuSystemAdministratorName, value);
    }
    
    public string VeriFactuSystemAdministratorFiscalNumber
    {
        get => _veriFactuSystemAdministratorFiscalNumber;
        set => SetPropertyValue(nameof(VeriFactuSystemAdministratorFiscalNumber), ref _veriFactuSystemAdministratorFiscalNumber, value);
    }
    
    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("CompanyInfos-SalesTaxes")]
    [DataSourceCriteria("IsAvailableInSales = True AND IsActive = True")]
    public XPCollection<TaxKind> SalesTaxes => GetCollection<TaxKind>(nameof(SalesTaxes));
    
    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("CompanyInfos-PurchaseTaxes")]
    [DataSourceCriteria("IsAvailableInPurchases = True AND IsActive = True")]
    public XPCollection<TaxKind> PurchaseTaxes => GetCollection<TaxKind>(nameof(PurchaseTaxes));
}