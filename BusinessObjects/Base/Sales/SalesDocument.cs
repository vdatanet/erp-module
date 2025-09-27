using System.Collections;
using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Common;
using Task = erp.Module.BusinessObjects.Common.Task;

namespace erp.Module.BusinessObjects.Base.Sales;

public abstract class SalesDocument(Session session) : BaseEntity(session)
{
    // private decimal _taxAmount;
    // private decimal _totalAmount;
    
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [PersistentAlias("Lines.Sum(TaxableAmount)")]
    public decimal TaxableAmount => Convert.ToDecimal(EvaluateAlias());

    // [ModelDefault("DisplayFormat", "{0:n2}")]
    // [ModelDefault("EditMask", "n2")]
    // [ModelDefault("AllowEdit", "False")]
    // public decimal TaxAmount
    // {
    //     get => _taxAmount;
    //     set => SetPropertyValue(nameof(TaxAmount), ref _taxAmount, value);
    // }

    // [ModelDefault("DisplayFormat", "{0:n2}")]
    // [ModelDefault("EditMask", "n2")]
    // [ModelDefault("AllowEdit", "False")]
    // public decimal TotalAmount
    // {
    //     get => _totalAmount;
    //     set => SetPropertyValue(nameof(TotalAmount), ref _totalAmount, value);
    // }

    [Aggregated]
    [Association("SalesDocument-Lines")]
    public XPCollection<SalesDocumentLine> Lines => GetCollection<SalesDocumentLine>();

    [Aggregated]
    [Association("SalesDocument-Taxes")]
    public XPCollection<SalesDocumentTax> Taxes => GetCollection<SalesDocumentTax>();
    
    [Aggregated]
    [Association("SalesDocument-Tasks")]
    public XPCollection<Task> Tasks => GetCollection<Task>(nameof(Tasks));
    
    [Aggregated]
    [Association("SalesDocument-Pictures")]
    public XPCollection<Picture> Pictures => GetCollection<Picture>(nameof(Pictures));
    
    [Aggregated]
    [Association("SalesDocument-Attachments")]
    public XPCollection<Attachment> Attachments => GetCollection<Attachment>(nameof(Attachments));

    // public void RecalculateTotals()
    // {
    //     if (IsLoading || Session?.IsObjectsLoading == true)
    //         return;
    //
    //     TaxableAmount = Lines.Sum(l => l.TaxableAmount);
    //     TaxAmount = Lines.Sum(l => l.TaxAmount);
    //     TotalAmount = Lines.Sum(l => l.TotalAmount);
    //     RebuildTaxSummaryByTaxType();
    // }

    public void RebuildTaxSummaryByTaxType()
    {
        foreach (var row in Taxes.ToList())
            row.Delete();

        var groups = Lines.SelectMany(l => l.Taxes)
            .GroupBy(t => t.TaxKind)
            .Select(g => new
            {
                TaxType = g.Key,
                BaseSum = g.Sum(x => x.TaxableAmount),
                AmountSum = g.Sum(x => x.TaxAmount)
            })
            .OrderBy(x => x.TaxType.Sequence)
            .ToList();

        foreach (var row in groups.Select(g => new SalesDocumentTax(Session)
                 {
                     SalesDocument = this,
                     TaxKind = g.TaxType,
                     Sequence = g.TaxType.Sequence,
                     TaxableAmount = g.BaseSum,
                     TaxAmount = g.AmountSum
                 }))
            Taxes.Add(row);
    }

    // protected override void OnSaving()
    // {
    //     base.OnSaving();
    //
    //     foreach (var line in Lines) line.Recalculate();
    //
    //     RecalculateTotals();
    // }

    // protected override void OnDeleting()
    // {
    //     base.OnDeleting();
    //
    //     foreach (var aggregated in new ArrayList(Taxes)) Session.Delete(aggregated);
    //
    //     foreach (var aggregated in new ArrayList(Lines)) Session.Delete(aggregated);
    // }
}