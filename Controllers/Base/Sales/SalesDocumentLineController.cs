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
    public SalesDocumentLineController(ISalesDocumentLineService service) : this()
    {
        _service = service;
    }

    private readonly ISalesDocumentLineService _service;

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
        if (e.Object is SalesDocumentLine line &&
            (e.PropertyName == nameof(SalesDocumentLine.Quantity) ||
             e.PropertyName == nameof(SalesDocumentLine.UnitPrice) ||
             e.PropertyName == nameof(SalesDocumentLine.DiscountPercent)))
        {
            _service.CalculateLineTaxableAmount(line);
        }
    }
}