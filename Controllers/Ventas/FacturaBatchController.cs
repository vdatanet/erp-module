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

    private async void ProcesarLoteAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var facturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        if (!facturas.Any()) return;

        if (_facturaOrchestrator == null || _veriFactuService == null)
        {
            MostrarMensaje("Servicios necesarios no inicializados.", InformationType.Error);
            return;
        }

        var result = await _facturaOrchestrator.ProcesarHastaContabilizadaLoteAsync(ObjectSpace, facturas, _veriFactuService);
        
        ObjectSpace.CommitChanges();

        string mensaje = $"Procesadas {result.Success} de {result.Total} facturas.";
        if (result.Success < result.Total)
        {
            mensaje += $" Último error: {result.LastErrorMessage}";
            MostrarMensaje(mensaje, InformationType.Warning);
        }
        else
        {
            MostrarMensaje(mensaje, InformationType.Success);
        }

        View.ObjectSpace.Refresh();
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
