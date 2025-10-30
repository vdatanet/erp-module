using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.Factories;
using erp.Module.Helpers.Contacts;
using System.ComponentModel;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Invoicing;

[DefaultClassOptions]
[NavigationItem("Invoicing")]
[ImageName("BO_Invoice")]
[DefaultProperty(nameof(InvoiceNumber))]
//[Appearance("InvoicePrefixDisabled", AppearanceItemType = "ViewItem", TargetItems = nameof(InvoicePrefix),
    //Criteria = "This is not null and !IsNewObject(This)", Context = "DetailView", Enabled = false)]
//[Appearance("InvoiceNumberDisabled", AppearanceItemType = "ViewItem", TargetItems = nameof(InvoiceNumber),
    //Criteria = "This is not null and !IsNewObject(This)", Context = "DetailView", Enabled = false)]
public class Invoice(Session session) : SalesDocument(session)
{
    private string _invoicePrefix;
    private string _invoiceNumber;
    private DateTime _invoiceDate;
    private Customer _customer;
    private VeriFactuStatusValues _veriFactuStatus;
    private string _invoiceEntryStatus;
    private string _invoiceEntryErrorCode;
    private TipoFactura _invoiceType;
    private TipoRectificativa _rectificationType;
    private IDType _relatedPartyIdType;
    private bool _isInvoiceFix;
    private string _text;
    private string _taxAgencyResponse;
    private string _csv;
    private string _validationUrl;
    private MediaDataObject _qr;

    [RuleRequiredField]
    [NonCloneable]
    public string InvoicePrefix
    {
        get => _invoicePrefix;
        set => SetPropertyValue(nameof(InvoicePrefix), ref _invoicePrefix, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    public string InvoiceNumber
    {
        get => _invoiceNumber;
        set => SetPropertyValue(nameof(InvoiceNumber), ref _invoiceNumber, value);
    }
    
    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    public DateTime InvoiceDate
    {
        get => _invoiceDate;
        set => SetPropertyValue(nameof(InvoiceDate), ref _invoiceDate, value);
    }
    
    [RuleRequiredField]
    [Association("Customer-Invoices")]
    public Customer Customer
    {
        get => _customer;
        set => SetPropertyValue(nameof(Customer), ref _customer, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public VeriFactuStatusValues VeriFactuStatus
    {
        get => _veriFactuStatus;
        set => SetPropertyValue(nameof(VeriFactuStatus), ref _veriFactuStatus, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public string InvoiceEntryStatus
    {
        get => _invoiceEntryStatus;
        set => SetPropertyValue(nameof(InvoiceEntryStatus), ref _invoiceEntryStatus, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public string InvoiceEntryErrorCode
    {
        get => _invoiceEntryErrorCode;
        set => SetPropertyValue(nameof(InvoiceEntryErrorCode), ref _invoiceEntryErrorCode, value);
    }

    [NonCloneable]
    public TipoFactura InvoiceType
    {
        get => _invoiceType;
        set => SetPropertyValue(nameof(InvoiceType), ref _invoiceType, value);
    }

    [NonCloneable]
    public TipoRectificativa RectificationType
    {
        get => _rectificationType;
        set => SetPropertyValue(nameof(RectificationType), ref _rectificationType, value);
    }

    [NonCloneable]
    public bool IsInvoiceFix
    {
        get => _isInvoiceFix;
        set => SetPropertyValue(nameof(IsInvoiceFix), ref _isInvoiceFix, value);
    }

    [NonCloneable]
    public IDType RelatedPartyIdType
    {
        get => _relatedPartyIdType;
        set => SetPropertyValue(nameof(RelatedPartyIdType), ref _relatedPartyIdType, value);
    }
    
    [Size(500)]
    public string Text
    {
        get => _text;
        set => SetPropertyValue(nameof(Text), ref _text, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("AllowEdit","False")]
    [NonCloneable]
    public string TaxAgencyResponse
    {
        get => _taxAgencyResponse;
        set => SetPropertyValue(nameof(TaxAgencyResponse), ref _taxAgencyResponse, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public string Csv
    {
        get => _csv;
        set => SetPropertyValue(nameof(Csv), ref _csv, value);
    }

    [Size(255)]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public string ValidationUrl
    {
        get => _validationUrl;
        set => SetPropertyValue(nameof(ValidationUrl), ref _validationUrl, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public MediaDataObject Qr
    {
        get => _qr;
        set => SetPropertyValue(nameof(Qr), ref _qr, value);
    }

    public enum VeriFactuStatusValues
    {
        Draft,
        Send
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        //InvoiceDate = DateTime.Now.Date;
        VeriFactuStatus = VeriFactuStatusValues.Draft;
        InvoiceType = TipoFactura.F1;
        RectificationType = TipoRectificativa.I;
        IsInvoiceFix = false;
        Text = "Descripción operación";
        RelatedPartyIdType = IDType.NIF_IVA;
        var companyInfo = CompanyInfoHelper.GetCompanyInfo(Session);
        if (companyInfo == null) return;
        //if (companyInfo.DefaultSalesAccount != null) SalesAccount = companyInfo.DefaultSalesAccount;
        //if (companyInfo.DefaultPurchaseAccount != null) PurchaseAccount = companyInfo.DefaultPurchaseAccount;
    }
    
    public void GetInvoiceNumber()
    {
        //if (!Session.IsNewObject(this) || !string.IsNullOrEmpty(InvoiceNumber) || Session is NestedUnitOfWork) return;
        InvoiceNumber =
            SequenceFactory.GetNextSequence(Session, $"{typeof(Invoice).FullName}.{InvoicePrefix}", InvoicePrefix, 5);
    }
    
    protected override void OnSaving()
    {
        base.OnSaving();
        //if (!Session.IsNewObject(this) || !string.IsNullOrEmpty(InvoiceNumber) || Session is NestedUnitOfWork) return;
        //InvoiceNumber =
            //SequenceFactory.GetNextSequence(Session, $"{typeof(Invoice).FullName}.{InvoicePrefix}", InvoicePrefix, 5);
    }
}