using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Common;
using Task = erp.Module.BusinessObjects.Common.Task;

namespace erp.Module.BusinessObjects.Base.Sales;

public abstract class SalesDocument(Session session) : BaseEntity(session)
{
    // [ModelDefault("DisplayFormat", "{0:n2}")]
    // [PersistentAlias("Lines.Sum(TaxableAmount)")]
    // public decimal TaxableAmount => Convert.ToDecimal(EvaluateAlias());

    // [ModelDefault("DisplayFormat", "{0:n2}")]
    // [PersistentAlias("Lines.Sum(TaxAmount)")]
    // public decimal TaxAmount => Convert.ToDecimal(EvaluateAlias());

    // [ModelDefault("DisplayFormat", "{0:n2}")]
    // [PersistentAlias("Lines.Sum(TotalAmount)")]
    // public decimal TotalAmount => Convert.ToDecimal(EvaluateAlias());
    
    private decimal _taxableAmount;
    private decimal _taxAmount;
    private decimal _totalAmount;
    
    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal TaxableAmount
    {
        get => _taxableAmount;
        set => SetPropertyValue(nameof(TaxableAmount), ref _taxableAmount, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal TaxAmount
    {
        get => _taxAmount;
        set => SetPropertyValue(nameof(TaxAmount), ref _taxAmount, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal TotalAmount
    {
        get => _totalAmount;
        set => SetPropertyValue(nameof(TotalAmount), ref _totalAmount, value);
    }

    [Aggregated]
    [Association("SalesDocument-Lines")]
    public XPCollection<SalesDocumentLine> Lines => GetCollection<SalesDocumentLine>();

    [Aggregated]
    [Association("SalesDocument-Taxes")]
    public XPCollection<SalesDocumentTax> Taxes => GetCollection<SalesDocumentTax>();

    [Aggregated]
    [Association("SalesDocument-Tasks")]
    public XPCollection<Task> Tasks => GetCollection<Task>();

    [Aggregated]
    [Association("SalesDocument-Pictures")]
    public XPCollection<Picture> Pictures => GetCollection<Picture>();

    [Aggregated]
    [Association("SalesDocument-Attachments")]
    public XPCollection<Attachment> Attachments => GetCollection<Attachment>();

    // public void RebuildTaxSummaryByTaxType()
    // {
    //     if (IsLoading || IsSaving) return;
    //
    //     foreach (var row in Taxes.ToList())
    //         row.Delete();
    //
    //     var groups = Lines.SelectMany(l => l.Taxes)
    //         .GroupBy(t => t.TaxKind)
    //         .Select(g => new
    //         {
    //             TaxType = g.Key,
    //             BaseSum = g.Sum(x => x.TaxableAmount),
    //             AmountSum = g.Sum(x => x.TaxAmount)
    //         })
    //         .OrderBy(x => x.TaxType.Sequence)
    //         .ToList();
    //
    //     var newTaxes = groups.Select(g => new SalesDocumentTax(Session)
    //     {
    //         SalesDocument = this,
    //         TaxKind = g.TaxType,
    //         Sequence = g.TaxType.Sequence,
    //         TaxableAmount = g.BaseSum,
    //         TaxAmount = g.AmountSum
    //     });
    //
    //     Taxes.AddRange(newTaxes);
    // }

    // protected override void OnDeleting()
    // {
    //     base.OnDeleting();
    //     foreach (var aggregated in new ArrayList(Taxes)) Session.Delete(aggregated);
    //     foreach (var aggregated in new ArrayList(Lines)) Session.Delete(aggregated);
    // }
}