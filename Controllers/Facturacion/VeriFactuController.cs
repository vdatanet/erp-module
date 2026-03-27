using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.Services.Facturacion;
using Microsoft.Extensions.DependencyInjection;
using FacturaBase = erp.Module.BusinessObjects.Base.Facturacion.FacturaBase;

namespace erp.Module.Controllers.Facturacion;

public class VeriFactuController : ViewController
{
    private VeriFactuService? _veriFactuService;

    public VeriFactuController()
    {
        TargetObjectType = typeof(FacturaBase);
        TargetViewType = ViewType.Any;

        var validateFactura = new SimpleAction(this, "ValidateFactura", PredefinedCategory.View)
        {
            Caption = "Validar",
            ImageName = "Action_Validation_Validate",
            TargetViewType = ViewType.DetailView
        };
        validateFactura.Execute += ValidateFactura_Execute;
        Actions.Add(validateFactura);
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        _veriFactuService = Application.ServiceProvider.GetRequiredService<VeriFactuService>();
    }

    private void ValidateFactura_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (View.CurrentObject is not FacturaBase invoice || _veriFactuService == null) return;

        var result = _veriFactuService.SendFactura(ObjectSpace, invoice);

        var options = new MessageOptions
        {
            Duration = 5000,
            Message = result.Message,
            Type = result.Success ? InformationType.Success : InformationType.Error,
            Web = { Position = InformationPosition.Right }
        };
        Application.ShowViewStrategy.ShowMessage(options);
        
        if (View is DetailView)
            View.Refresh();
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
    }
}