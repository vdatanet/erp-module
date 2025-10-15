using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Products;
using erp.Module.BusinessObjects.Taxes;
using System.ComponentModel;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Base.Sales;

[ImageName("Top10Percent")]
[DefaultProperty(nameof(Sequence))]
[NavigationItem("Taxes")]
public class SalesDocumentTax(Session session): BaseEntity(session)
{
    private SalesDocument _salesDocument;
    private int _sequence;
    private TaxKind _taxKind;
    private Account _account;
    private decimal _rate;
    private Impuesto? _tax;
    private ClaveRegimen? _taxScheme;
    private CalificacionOperacion? _taxType;
    private CausaExencion? _taxExemption;
    
    private decimal _taxableAmount;
    private decimal _taxAmount;
    
    [Association("SalesDocument-Taxes")]
    public SalesDocument SalesDocument
    {
        get => _salesDocument;
        set => SetPropertyValue(nameof(SalesDocument), ref _salesDocument, value);
    }
    
    public int Sequence
    {
        get => _sequence;
        set => SetPropertyValue(nameof(Sequence), ref _sequence, value);
    }

    [ImmediatePostData]
    public TaxKind TaxKind
    {
        get => _taxKind;
        set
        {
            bool modified = SetPropertyValue(nameof(TaxKind), ref _taxKind, value);
            if (!modified || IsLoading || IsSaving || IsDeleted) return;
            ApplyTaxSnapshot();
        }
    }

    private void ApplyTaxSnapshot()
    {
        if (TaxKind is null)
        {
            Sequence = 0;
            Account = null;
            Rate = 0;
            Tax = null;
            TaxScheme = null;
            TaxType = null;
            TaxExemption = null;
            TaxableAmount = 0;
            return;
        }

        Sequence = TaxKind.Sequence;
        Account = TaxKind.Account;
        Rate = TaxKind.Rate;
        Tax = TaxKind.Tax;
        TaxScheme = TaxKind.TaxScheme;
        TaxType = TaxKind.TaxType;
        TaxExemption = TaxKind.TaxExemption;
    }

    public Account Account
    {
        get => _account;
        set => SetPropertyValue(nameof(Account), ref _account, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal Rate
    {
        get => _rate;
        set => SetPropertyValue(nameof(Rate), ref _rate, value);
    }

    public Impuesto? Tax
    {
        get => _tax;
        set => SetPropertyValue(nameof(Tax), ref _tax, value);
    }
    public ClaveRegimen? TaxScheme
    {
        get => _taxScheme;
        set => SetPropertyValue(nameof(TaxScheme), ref _taxScheme, value);
    }
    
    public CalificacionOperacion? TaxType
    {
        get => _taxType;
        set => SetPropertyValue(nameof(TaxType), ref _taxType, value);
    }
    
    public CausaExencion? TaxExemption
    {
        get => _taxExemption;
        set => SetPropertyValue(nameof(TaxExemption), ref _taxExemption, value);
    }
    
    public decimal TaxableAmount
    {
        get => _taxableAmount;
        set => SetPropertyValue(nameof(TaxableAmount), ref _taxableAmount, value);
    }
    
    public decimal TaxAmount
    {
        get => _taxAmount;
        set => SetPropertyValue(nameof(TaxAmount), ref _taxAmount, value);  
    }
}