using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Common;
using SequenceFactory = erp.Module.Factories.SequenceFactory;

namespace erp.Module.BusinessObjects.Base.Sales;

//[DefaultClassOptions]
//[NavigationItem("Invoicing")]
//[ImageName("BO_Invoice")]
//[DefaultProperty(nameof(InvoiceNumber))]
//[Appearance("InvoicePrefixDisabled", AppearanceItemType = "ViewItem", TargetItems = nameof(Prefix),
    //Criteria = "This is not null and !IsNewObject(This)", Context = "DetailView", Enabled = false)]
//[Appearance("InvoiceNumberDisabled", AppearanceItemType = "ViewItem", TargetItems = nameof(InvoiceNumber),
    //Criteria = "This is not null and !IsNewObject(This)", Context = "DetailView", Enabled = false)]
public abstract class SalesDocument(Session session): BaseEntity(session)
{
    private decimal _baseAmount;
    private decimal _taxAmount;
    private decimal _totalAmount;
    
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal BaseAmount
    {
        get => _baseAmount;
        set => SetPropertyValue(nameof(BaseAmount), ref _baseAmount, value);
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
    
    //public void RecalculateTotals()
    //{
        //if (IsLoading || Session?.IsObjectsLoading == true)
            //return;
        
        //BaseAmount = InvoiceLines.Sum(l => l.BaseAmount);
        //TaxAmount = InvoiceLines.Sum(l => l.TaxAmount);
        //TotalAmount = InvoiceLines.Sum(l => l.TotalAmount);
    //}
    
    protected override void OnSaving()
    {
        base.OnSaving();
        
        //foreach (var invoiceLine in InvoiceLines)
        //{
            //invoiceLine.Recalculate();
        //}
        
        //RecalculateTotals();
        
        //if (!Session.IsNewObject(this) || !string.IsNullOrEmpty(InvoiceNumber) || Session is NestedUnitOfWork) return;
        //InvoiceNumber = SequenceFactory.GetNextSequence(Session, $"{typeof(Invoice).FullName}.{Prefix}", Prefix, 5);
    }
}