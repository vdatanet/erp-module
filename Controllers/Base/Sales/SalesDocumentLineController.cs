using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.Services.Interfaces.Base.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.Controllers.Base.Sales;

public class SalesDocumentLineController : ViewController
{
    public SalesDocumentLineController()
    {
        TargetObjectType = typeof(SalesDocumentLine);
        TargetViewType = ViewType.DetailView;
    }

    [ActivatorUtilitiesConstructor]
    public SalesDocumentLineController(
        ISalesDocumentLineService lineService,
        ISalesDocumentService documentService) : this()
    {
        _lineService = lineService;
        _documentService = documentService;
    }

    private readonly ISalesDocumentLineService _lineService;
    private readonly ISalesDocumentService _documentService;

    protected override void OnActivated()
    {
        base.OnActivated();
        //View.ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
    }

    protected override void OnDeactivated()
    {
        //View.ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
        base.OnDeactivated();
    }

    private void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
    {
        // if (e.Object is SalesDocumentLine line &&
        //     (e.PropertyName == nameof(SalesDocumentLine.Quantity) ||
        //      e.PropertyName == nameof(SalesDocumentLine.UnitPrice) ||
        //      e.PropertyName == nameof(SalesDocumentLine.DiscountPercent)))
        // {
        //     _lineService.CalculateLineTaxableAmount(line);
        //     _documentService.ComputeTotals(line.SalesDocument);
        // }
    }
}