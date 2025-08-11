using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Base.Sales;

public class SalesDocumentTax(Session session): BaseEntity(session)
{
    private SalesDocument _salesDocument;
    private int _sequence;
    private TaxType _taxType;
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
    
    public TaxType TaxType
    {
        get => _taxType;
        set => SetPropertyValue(nameof(TaxType), ref _taxType, value);
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