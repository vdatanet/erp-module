using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.Services.Facturacion;
using erp.Module.Services.Ventas;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.Controllers.Ventas;

public class FacturaBatchController : ViewController
{
    private readonly SimpleAction _procesarLoteAction;
    private FacturaOrchestrator? _facturaOrchestrator;
    private VeriFactuService? _veriFactuService;

    public FacturaBatchController()
    {
        TargetObjectType = typeof(FacturaBase);
        TargetViewType = ViewType.ListView;

        _procesarLoteAction = new SimpleAction(this, "Factura_ProcesarLote", PredefinedCategory.Edit)
        {
            Caption = "Procesar Lote",
            ConfirmationMessage = "¿Desea procesar las facturas seleccionadas en lote?",
            ImageName = "Action_BatchEdit",
            SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
        };
        _procesarLoteAction.Execute += ProcesarLoteAction_Execute;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        _facturaOrchestrator = Application.ServiceProvider.GetRequiredService<FacturaOrchestrator>();
        _veriFactuService = Application.ServiceProvider.GetRequiredService<VeriFactuService>();
    }

    private void ProcesarLoteAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        // Acción sin implementar por ahora según requerimiento
        // Aquí se llamaría a los métodos de lote de FacturaOrchestrator
        
        var facturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        if (!facturas.Any()) return;

        // Ejemplo de uso futuro:
        // var result = _facturaOrchestrator.ValidarLote(facturas);
        // MostrarMensaje($"Procesadas {result.Success} de {result.Total} facturas.", InformationType.Info);
        
        MostrarMensaje("Acción de procesamiento por lote ejecutada (lógica pendiente de implementación detallada).", InformationType.Info);
    }

    private void MostrarMensaje(string message, InformationType type)
    {
        var options = new MessageOptions
        {
            Duration = 5000,
            Message = message,
            Type = type,
            Web = { Position = InformationPosition.Right }
        };
        Application.ShowViewStrategy.ShowMessage(options);
    }
}
