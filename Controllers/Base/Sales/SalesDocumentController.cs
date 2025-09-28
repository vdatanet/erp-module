using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.Services.Interfaces.Base.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.Controllers.Base.Sales;

public class SalesDocumentController : ViewController
{
    public SalesDocumentController()
    {
        TargetObjectType = typeof(SalesDocumentLine);
        TargetViewType = ViewType.DetailView;
    }

    [ActivatorUtilitiesConstructor]
    public SalesDocumentController(ISalesDocumentService documentService) : this()
    {
        _documentService = documentService;
    }
    
    private readonly ISalesDocumentService _documentService;
    private SalesDocument _salesDocument;

    protected override void OnActivated()
    {
        base.OnActivated();

        _salesDocument = (SalesDocument)View.CurrentObject;
        
        View.ObjectSpace.Committing += ObjectSpace_Committing;
        View.ObjectSpace.ObjectDeleted += ObjectSpace_ObjectDeleted;
    }

    protected override void OnDeactivated()
    {
        View.ObjectSpace.Committing -= ObjectSpace_Committing;
        View.ObjectSpace.ObjectDeleted -= ObjectSpace_ObjectDeleted;
        base.OnDeactivated();
    }
    
    private void ObjectSpace_ObjectDeleted(object sender, ObjectsManipulatingEventArgs e)
    {
        foreach (var obj in e.Objects)
        {
            if (obj is SalesDocumentLine salesDocumentLine)
            {
                _documentService.CalculateTaxableAmount(_salesDocument);
            }
        }
    }

    private void ObjectSpace_Committing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _documentService.CalculateTaxableAmount(_salesDocument);
    }
}