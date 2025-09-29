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

    // [ActivatorUtilitiesConstructor]
    // public SalesLineController(
    //     ISalesLineService lineService,
    //     ISalesDocumentService documentService) : this()
    // {
    //     _lineService = lineService;
    //     _documentService = documentService;
    // }
    //
    // private readonly ISalesLineService _lineService;
    // private readonly ISalesDocumentService _documentService;

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

    // private void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
    // {
    //     if (e.Object is not SalesDocumentLine line) return;
    //
    //     switch (e.PropertyName)
    //     {
    //         case nameof(SalesDocumentLine.Product):
    //             _lineService.DeleteTaxes(line);
    //             _lineService.ApplyProductSnapshot(line);
    //             _lineService.RebuildTaxes(line);
    //             _documentService.DeleteTaxes(line.SalesDocument);
    //             _documentService.RebuildTaxSummary(line.SalesDocument);
    //             break;
    //         case nameof(SalesDocumentLine.Quantity):
    //         case nameof(SalesDocumentLine.UnitPrice):
    //         case nameof(SalesDocumentLine.DiscountPercent):
    //             _lineService.SetTaxableAmount(line);
    //             break;
    //         case nameof(SalesDocumentLine.TaxableAmount):
    //             _lineService.RebuildTaxes(line);
    //             _documentService.DeleteTaxes(line.SalesDocument);
    //             _documentService.RebuildTaxSummary(line.SalesDocument);
    //             break;
    //     }
    // }
}