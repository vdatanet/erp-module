using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.Services.Interfaces.Base.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.Controllers.Base.Sales;

public class SalesDocumentController : ViewController
{
    public SalesDocumentController()
    {
        TargetObjectType = typeof(SalesDocument);
        TargetViewType = ViewType.DetailView;
    }

    [ActivatorUtilitiesConstructor]
    public SalesDocumentController(ISalesDocumentService documentService) : this()
    {
        _documentService = documentService;
    }

    private readonly ISalesDocumentService _documentService;

    protected override void OnActivated()
    {
        base.OnActivated();
        View.ObjectSpace.ObjectDeleted += ObjectSpace_ObjectDeleted;
    }

    protected override void OnDeactivated()
    {
        View.ObjectSpace.ObjectDeleted -= ObjectSpace_ObjectDeleted;
        base.OnDeactivated();
    }

    private void ObjectSpace_ObjectDeleted(object sender, ObjectsManipulatingEventArgs e)
    {
        var salesDocument = (SalesDocument)View.CurrentObject;

        foreach (var obj in e.Objects)
        {
            if (obj is not SalesDocumentLine salesDocumentLine) continue;
            _documentService.DeleteTaxes(salesDocument);
            _documentService.RebuildTaxSummary(salesDocument);
        }
    }
}