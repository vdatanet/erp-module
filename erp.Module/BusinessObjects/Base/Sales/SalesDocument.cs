using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Base.Sales;

public abstract class SalesDocument(Session session) : BaseEntity(session)
{
    private decimal _taxableAmount;
    private decimal _taxAmount;
    private decimal _totalAmount;

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal TaxableAmount
    {
        get => _taxableAmount;
        set => SetPropertyValue(nameof(TaxableAmount), ref _taxableAmount, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal TaxAmount
    {
        get => _taxAmount;
        set => SetPropertyValue(nameof(TaxAmount), ref _taxAmount, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetPropertyValue(nameof(TotalAmount), ref _totalAmount, value);
    }

    [Aggregated]
    [Association("SalesDocument-Lines")]
    public XPCollection<SalesDocumentLine> Lines => GetCollection<SalesDocumentLine>();
    
    [Association("SalesDocument-Taxes")]
    public XPCollection<SalesDocumentTax> Taxes => GetCollection<SalesDocumentTax>();
    
    public void RecalculateTotals()
    {
        if (IsLoading || Session?.IsObjectsLoading == true)
            return;

        TaxableAmount = Lines.Sum(l => l.TaxableAmount);
        TaxAmount = Lines.Sum(l => l.TaxAmount);
        TotalAmount = Lines.Sum(l => l.TotalAmount);
    }

    protected override void OnSaving()
    {
        base.OnSaving();

        foreach (var line in Lines) line.Recalculate();

        RecalculateTotals();
    }
}