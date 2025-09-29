using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.Services.Interfaces.Base.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.Controllers.Base.Sales;

public class SalesLineController : ViewController
{
    public SalesLineController()
    {
        TargetObjectType = typeof(SalesDocumentLine);
        TargetViewType = ViewType.DetailView;
    }

    [ActivatorUtilitiesConstructor]
    public SalesLineController(
        ISalesLineService lineService,
        ISalesDocumentService documentService) : this()
    {
        _lineService = lineService;
        _documentService = documentService;
    }

    private readonly ISalesLineService _lineService;
    private readonly ISalesDocumentService _documentService;

    protected override void OnActivated()
    {
        base.OnActivated();
        View.ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
    }

    protected override void OnDeactivated()
    {
        View.ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
        base.OnDeactivated();
    }

    private void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
    {
        // Apply Product Snapshot
        
        if (e.Object is SalesDocumentLine line && e.PropertyName == nameof(SalesDocumentLine.Product))
        {
            line.ProductName = "Hello there";
        }
    }
}