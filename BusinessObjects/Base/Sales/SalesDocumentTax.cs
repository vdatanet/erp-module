using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Taxes;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Base.Sales;

[ImageName("Top10Percent")]
[DefaultProperty(nameof(Sequence))]
public class SalesDocumentTax(Session session): BaseEntity(session)
{
    private SalesDocument _salesDocument;
    private int _sequence;
    private TaxKind _taxKind;
    private Impuesto _tax;
    private ClaveRegimen _taxScheme;
    private CalificacionOperacion _taxType;
    private CausaExencion _taxExemption;
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
    
    public TaxKind TaxKind
    {
        get => _taxKind;
        set => SetPropertyValue(nameof(TaxKind), ref _taxKind, value);
    }
    
    public Impuesto Tax
    {
        get => _tax;
        set => SetPropertyValue(nameof(Tax), ref _tax, value);
    }
    public ClaveRegimen TaxScheme
    {
        get => _taxScheme;
        set => SetPropertyValue(nameof(TaxScheme), ref _taxScheme, value);
    }
    
    public CalificacionOperacion TaxType
    {
        get => _taxType;
        set => SetPropertyValue(nameof(TaxType), ref _taxType, value);
    }
    
    public CausaExencion TaxExemption
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