using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Common;
using Task = erp.Module.BusinessObjects.Planning.Task;

namespace erp.Module.BusinessObjects.Base.Sales;

public abstract class SalesDocument(Session session) : BaseEntity(session)
{
    private decimal _taxableAmount;
    private decimal _taxAmount;
    private decimal _totalAmount;
    
    [ModelDefault("AllowEdit","False")]
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
    public XPCollection<SalesDocumentLine> Lines
    {
        get
        {
            var collection = GetCollection<SalesDocumentLine>(nameof(Lines));
            if (!collection.IsLoaded)
            {
                collection.CollectionChanged += Lines_CollectionChanged;
            }
            return collection;
        }
    }
    
    [Aggregated]
    [Association("SalesDocument-Taxes")]
    public XPCollection<SalesDocumentTax> Taxes => GetCollection<SalesDocumentTax>();
    
    private void Lines_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
    {
        if (IsLoading || IsSaving || IsDeleted) return;
        if (e.CollectionChangedType != XPCollectionChangedType.AfterRemove) return;
        DeleteTaxesSummary();
        RebuildTaxSummary();
    }

    [Aggregated]
    [Association("SalesDocument-Tasks")]
    public XPCollection<Task> Tasks => GetCollection<Task>();

    [Aggregated]
    [Association("SalesDocument-Pictures")]
    public XPCollection<Picture> Pictures => GetCollection<Picture>();

    [Aggregated]
    [Association("SalesDocument-Attachments")]
    public XPCollection<Attachment> Attachments => GetCollection<Attachment>();

    public void DeleteTaxesSummary()
    {
        for (var i = Taxes.Count - 1; i >= 0; i--)
            Taxes[i].Delete();
    }

    public void RebuildTaxSummary()
    {
        var groups = Lines.SelectMany(l => l.Taxes)
            .GroupBy(t => t.TaxKind)
            .Select(g => new
            {
                TaxType = g.Key,
                BaseSum = g.Sum(x => x.TaxableAmount)
            })
            .OrderBy(x => x.TaxType.Sequence)
            .ToList();

        var newTaxes = groups.Select(g => new SalesDocumentTax(this.Session)
        {
            SalesDocument = this,
            TaxKind = g.TaxType,
            Sequence = g.TaxType.Sequence,
            TaxableAmount = g.BaseSum
        });

        Taxes.AddRange(newTaxes);

        TaxableAmount = Lines.Sum(t => t.TaxableAmount);
        TaxAmount = Lines.Sum(t => t.TaxAmount);
        TotalAmount = TaxableAmount + TaxAmount;
    }
    
}