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
    //
    // [ModelDefault("DisplayFormat", "{0:n2}")]
    // [PersistentAlias("Lines.Sum(TaxAmount)")]
    // public decimal TaxAmount => Convert.ToDecimal(EvaluateAlias());
    //
    // [ModelDefault("DisplayFormat", "{0:n2}")]
    // [PersistentAlias("Lines.Sum(TotalAmount)")]
    //public decimal TotalAmount => Convert.ToDecimal(EvaluateAlias());

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
    
    // protected override void OnDeleting()
    // {
    //     base.OnDeleting();
    //     foreach (var aggregated in new ArrayList(Taxes)) Session.Delete(aggregated);
    //     foreach (var aggregated in new ArrayList(Lines)) Session.Delete(aggregated);
    // }
}