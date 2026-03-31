using System.Linq;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.Services.Facturacion;
using erp.Module.Services.Ventas;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.Controllers.Ventas;

public class FacturaLifecycleController : ViewController
{
    private readonly SimpleAction _validarAction;
    private readonly SimpleAction _emitirAction;
    private readonly SimpleAction _revertirABorradorAction;
    private readonly SimpleAction _enviarVerifactuAction;
    private readonly SimpleAction _contabilizarAction;
    private readonly SimpleAction _procesarFlujoCompletoAction;
    private VeriFactuService? _veriFactuService;
    private FacturaOrchestrator? _facturaOrchestrator;

    public FacturaLifecycleController()
    {
        TargetObjectType = typeof(FacturaBase);
        TargetViewType = ViewType.Any;

        _validarAction = new SimpleAction(this, "Factura_Validar", PredefinedCategory.Edit)
        {
            Caption = "Validar",
            ConfirmationMessage = "¿Desea validar las facturas seleccionadas?",
            ImageName = "BO_Validation",
            TargetObjectsCriteria = "EstadoFactura = 'Borrador'",
            SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
        };
        _validarAction.Execute += ValidarAction_Execute;

        _revertirABorradorAction = new SimpleAction(this, "Factura_RevertirABorrador", PredefinedCategory.Edit)
        {
            Caption = "Revertir a Borrador",
            ConfirmationMessage = "¿Desea revertir las facturas seleccionadas a borrador?",
            ImageName = "Undo",
            TargetObjectsCriteria = "EstadoFactura = 'Validada'",
            SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
        };
        _revertirABorradorAction.Execute += RevertirABorradorAction_Execute;

        _emitirAction = new SimpleAction(this, "Factura_Emitir", PredefinedCategory.Edit)
        {
            Caption = "Emitir",
            ConfirmationMessage = "¿Desea emitir las facturas seleccionadas? (Se asignará número definitivo y fecha de emisión)",
            ImageName = "Redo",
            TargetObjectsCriteria = "EstadoFactura = 'Validada'",
            SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
        };
        _emitirAction.Execute += EmitirAction_Execute;

        _enviarVerifactuAction = new SimpleAction(this, "Factura_EnviarVerifactu", PredefinedCategory.Edit)
        {
            Caption = "Enviar a VeriFactu",
            ConfirmationMessage = "¿Desea enviar las facturas seleccionadas a VeriFactu?",
            ImageName = "Actions_Send",
            TargetObjectsCriteria = "EstadoFactura = 'Emitida'",
            SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
        };
        _enviarVerifactuAction.Execute += EnviarVerifactuAction_Execute;

        _contabilizarAction = new SimpleAction(this, "Factura_Contabilizar", PredefinedCategory.Edit)
        {
            Caption = "Contabilizar",
            ConfirmationMessage = "¿Desea generar el asiento contable para las facturas seleccionadas?",
            ImageName = "Accounting",
            TargetObjectsCriteria = "EstadoFactura = 'Enviada' OR EstadoFactura = 'VeriFactuNoNecesario' OR (EstadoVeriFactu = 'AceptadaVeriFactu' OR EstadoVeriFactu = 'EnviadaVeriFactu' OR EstadoVeriFactu = 'PendienteVeriFactu')",
            SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
        };
        _contabilizarAction.Execute += ContabilizarAction_Execute;

        _procesarFlujoCompletoAction = new SimpleAction(this, "Factura_ProcesarFlujoCompleto", PredefinedCategory.Edit)
        {
            Caption = "Procesar",
            ConfirmationMessage = "¿Desea procesar las facturas seleccionadas hasta su contabilización final? (Validar -> Emitir -> VeriFactu -> Contabilizar)",
            ImageName = "Icon_PageNext",
            TargetObjectsCriteria = "EstadoFactura != 'Contabilizada'",
            SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
        };
        _procesarFlujoCompletoAction.Execute += ProcesarFlujoCompletoAction_Execute;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        _veriFactuService = Application.ServiceProvider.GetRequiredService<VeriFactuService>();
        _facturaOrchestrator = Application.ServiceProvider.GetRequiredService<FacturaOrchestrator>();

        // Ocultar acciones de validación nativas para no confundir al usuario
        // ValidateFactura se usa en FacturaBase.cs en un atributo Appearance
        var actionIdsToHide = new[] { "ValidateFactura", "Validation" };
        foreach (var actionId in actionIdsToHide)
        {
            var action = Frame.Controllers.Cast<Controller>()
                .SelectMany(c => c.Actions)
                .FirstOrDefault(a => a.Id == actionId);

            if (action != null)
            {
                action.Active["OcultarValidacionNativa"] = false;
            }
        }

        View.CurrentObjectChanged += View_CurrentObjectChanged;
        View.SelectionChanged += View_SelectionChanged;
        ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
        ActualizarVisibilidadAcciones();
    }

    protected override void OnDeactivated()
    {
        View.CurrentObjectChanged -= View_CurrentObjectChanged;
        View.SelectionChanged -= View_SelectionChanged;
        ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
        // Reactivar acciones al desactivar el controlador
        var actionIdsToHide = new[] { "ValidateFactura", "Validation" };
        foreach (var actionId in actionIdsToHide)
        {
            var action = Frame.Controllers.Cast<Controller>()
                .SelectMany(c => c.Actions)
                .FirstOrDefault(a => a.Id == actionId);

            if (action != null)
            {
                action.Active.RemoveItem("OcultarValidacionNativa");
            }
        }
        base.OnDeactivated();
    }

    private void View_CurrentObjectChanged(object? sender, EventArgs e)
    {
        ActualizarVisibilidadAcciones();
    }

    private void View_SelectionChanged(object? sender, EventArgs e)
    {
        ActualizarVisibilidadAcciones();
    }

    private void ObjectSpace_ObjectChanged(object? sender, ObjectChangedEventArgs e)
    {
        if (e.Object == View.CurrentObject && 
            (e.PropertyName == nameof(FacturaBase.EstadoFactura) || e.PropertyName == nameof(FacturaBase.EstadoVeriFactu)))
        {
            ActualizarVisibilidadAcciones();
        }
    }

    private void ActualizarVisibilidadAcciones()
    {
        var selectedFacturas = View.SelectedObjects.Cast<FacturaBase>().ToList();
        
        _procesarFlujoCompletoAction.Active["EstadoValido"] = selectedFacturas.Any(f => f.EstadoFactura != EstadoFactura.Contabilizada);
        _validarAction.Active["EstadoValido"] = selectedFacturas.Any(f => f.PuedeValidar);
        _revertirABorradorAction.Active["EstadoValido"] = selectedFacturas.Any(f => f.PuedeRevertirABorrador);
        _emitirAction.Active["EstadoValido"] = selectedFacturas.Any(f => f.PuedeEmitir);
        _enviarVerifactuAction.Active["EstadoValido"] = selectedFacturas.Any(f => f.PuedeEnviarVerifactu);
        _contabilizarAction.Active["EstadoValido"] = selectedFacturas.Any(f => f.PuedeContabilizar);
    }

    private void ValidarAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_facturaOrchestrator == null) return;
        var selectedFacturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        
        _facturaOrchestrator.ValidarLote(selectedFacturas);
        
        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private void EmitirAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_facturaOrchestrator == null) return;
        var selectedFacturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        
        _facturaOrchestrator.EmitirLote(selectedFacturas);
        
        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private void RevertirABorradorAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_facturaOrchestrator == null) return;
        var selectedFacturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        
        _facturaOrchestrator.RevertirABorradorLote(selectedFacturas);
        
        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private async void EnviarVerifactuAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_veriFactuService == null || _facturaOrchestrator == null) return;

        var selectedFacturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        var result = await _facturaOrchestrator.EnviarAVerifactuLoteAsync(ObjectSpace, selectedFacturas, _veriFactuService);

        if (result.Total > 0)
        {
            var message = result.Total == 1 
                ? (result.Success == 1 ? "Factura enviada a VeriFactu correctamente." : $"Error al enviar la factura: {result.LastErrorMessage}")
                : $"Proceso de envío finalizado. {result.Success} de {result.Total} facturas enviadas correctamente.";

            MostrarMensaje(message, result.Success == result.Total ? InformationType.Success : (result.Success > 0 ? InformationType.Warning : InformationType.Error));
        }

        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private void ContabilizarAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_facturaOrchestrator == null) return;
        var selectedFacturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        
        var result = _facturaOrchestrator.ContabilizarLote(selectedFacturas);

        if (result.Total > 0)
        {
            var message = result.Total == 1 
                ? (result.Success == 1 ? "Factura contabilizada correctamente." : $"Error al contabilizar: {result.LastErrorMessage}")
                : $"Contabilización finalizada. {result.Success} de {result.Total} facturas contabilizadas correctamente.";

            if (result.ErrorMessages?.Any() == true && result.Total > 1)
            {
                message += " Revise los errores individuales.";
            }

            MostrarMensaje(message, result.Success == result.Total ? InformationType.Success : (result.Success > 0 ? InformationType.Warning : InformationType.Error));
        }

        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private async void ProcesarFlujoCompletoAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_veriFactuService == null || _facturaOrchestrator == null) return;

        var facturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        if (!facturas.Any()) return;

        var successCount = 0;
        var totalCount = facturas.Count;
        var lastErrorMessage = string.Empty;

        foreach (var factura in facturas)
        {
            var result = await _facturaOrchestrator.ProcesarHastaContabilizadaAsync(ObjectSpace, factura, _veriFactuService);
            if (result.Success)
            {
                successCount++;
            }
            else
            {
                lastErrorMessage = result.Message;
            }
        }

        var message = totalCount == 1 
            ? (successCount == 1 ? "Factura procesada correctamente." : $"Error al procesar la factura: {lastErrorMessage}")
            : $"Proceso finalizado. {successCount} de {totalCount} facturas procesadas correctamente.";

        MostrarMensaje(message, successCount == totalCount ? InformationType.Success : (successCount > 0 ? InformationType.Warning : InformationType.Error));

        ObjectSpace.CommitChanges();
        View.Refresh();
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
