using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.Services;
using Microsoft.Extensions.DependencyInjection;
using FacturaBase = erp.Module.BusinessObjects.Facturacion.FacturaBase;

namespace erp.Module.Controllers.Invoicing;

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
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        _veriFactuService = Application.ServiceProvider.GetRequiredService<VeriFactuService>();
    }

    private void ValidateFactura_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (View.CurrentObject is not FacturaBase invoice) return;

        _veriFactuService.SendFactura(ObjectSpace, invoice);

        var options = new MessageOptions
        {
            Duration = 2000,
            Message = "Factura enviada correctamente a VeriFactu",
            Type = InformationType.Success,
            Web = { Position = InformationPosition.Right }
        };
        Application.ShowViewStrategy.ShowMessage(options);
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
    }
}