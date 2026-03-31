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
    private readonly SimpleAction _actualizarEstadoVerifactuAction;
    private VeriFactuService? _veriFactuService;
    private FacturaOrchestrator? _facturaOrchestrator;

    public FacturaLifecycleController()
    {
        TargetObjectType = typeof(FacturaBase);
        TargetViewType = ViewType.DetailView;

        _validarAction = new SimpleAction(this, "Factura_Validar", PredefinedCategory.Edit)
        {
            Caption = "Validar",
            ConfirmationMessage = "¿Desea validar la factura seleccionada?",
            ImageName = "BO_Validation",
            TargetObjectsCriteria = "EstadoFactura = 'Borrador'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _validarAction.Execute += ValidarAction_Execute;

        _revertirABorradorAction = new SimpleAction(this, "Factura_RevertirABorrador", PredefinedCategory.Edit)
        {
            Caption = "Revertir a Borrador",
            ConfirmationMessage = "¿Desea revertir la factura seleccionada a borrador?",
            ImageName = "Undo",
            TargetObjectsCriteria = "EstadoFactura = 'Validada'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _revertirABorradorAction.Execute += RevertirABorradorAction_Execute;

        _emitirAction = new SimpleAction(this, "Factura_Emitir", PredefinedCategory.Edit)
        {
            Caption = "Emitir",
            ConfirmationMessage = "¿Desea emitir la factura seleccionada? (Se asignará número definitivo y fecha de emisión)",
            ImageName = "Redo",
            TargetObjectsCriteria = "EstadoFactura = 'Validada'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _emitirAction.Execute += EmitirAction_Execute;

        _enviarVerifactuAction = new SimpleAction(this, "Factura_EnviarVerifactu", PredefinedCategory.Edit)
        {
            Caption = "Enviar a VeriFactu",
            ConfirmationMessage = "¿Desea enviar la factura seleccionada a VeriFactu?",
            ImageName = "Actions_Send",
            TargetObjectsCriteria = "EstadoFactura = 'Emitida'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _enviarVerifactuAction.Execute += EnviarVerifactuAction_Execute;

        _contabilizarAction = new SimpleAction(this, "Factura_Contabilizar", PredefinedCategory.Edit)
        {
            Caption = "Contabilizar",
            ConfirmationMessage = "¿Desea generar el asiento contable para la factura seleccionada?",
            ImageName = "Accounting",
            TargetObjectsCriteria = "EstadoFactura = 'Enviada' OR EstadoFactura = 'VeriFactuNoNecesario' OR (EstadoVeriFactu = 'Correcto' OR EstadoVeriFactu = 'EnviadaVeriFactu' OR EstadoVeriFactu = 'Pendiente')",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _contabilizarAction.Execute += ContabilizarAction_Execute;

        _procesarFlujoCompletoAction = new SimpleAction(this, "Factura_ProcesarFlujoCompleto", PredefinedCategory.Edit)
        {
            Caption = "Procesar",
            ConfirmationMessage = "¿Desea procesar la factura seleccionada hasta su contabilización final? (Validar -> Emitir -> VeriFactu -> Contabilizar)",
            ImageName = "Icon_PageNext",
            TargetObjectsCriteria = "EstadoFactura != 'Contabilizada'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _procesarFlujoCompletoAction.Execute += ProcesarFlujoCompletoAction_Execute;

        _actualizarEstadoVerifactuAction = new SimpleAction(this, "Factura_ActualizarEstadoVerifactu", PredefinedCategory.Edit)
        {
            Caption = "Actualizar Estado VeriFactu",
            ImageName = "Action_Refresh",
            TargetObjectsCriteria = "Uuid IS NOT NULL AND (EstadoVeriFactu = 'EnviadaVeriFactu' OR EstadoVeriFactu = 'Pendiente' OR EstadoVeriFactu = 'Incorrecto' OR EstadoVeriFactu = 'ErrorServidorAEAT')",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _actualizarEstadoVerifactuAction.Execute += ActualizarEstadoVerifactuAction_Execute;
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
        var factura = View.CurrentObject as FacturaBase;
        
        _procesarFlujoCompletoAction.Active["EstadoValido"] = factura != null && factura.EstadoFactura != EstadoFactura.Contabilizada;
        _validarAction.Active["EstadoValido"] = factura?.PuedeValidar ?? false;
        _revertirABorradorAction.Active["EstadoValido"] = factura?.PuedeRevertirABorrador ?? false;
        _emitirAction.Active["EstadoValido"] = factura?.PuedeEmitir ?? false;
        _enviarVerifactuAction.Active["EstadoValido"] = factura?.PuedeEnviarVerifactu ?? false;
        _contabilizarAction.Active["EstadoValido"] = factura?.PuedeContabilizar ?? false;
    }

    private void ValidarAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_facturaOrchestrator == null) return;
        var factura = e.CurrentObject as FacturaBase;
        if (factura == null) return;
        
        _facturaOrchestrator.Validar(factura);
        
        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private void EmitirAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_facturaOrchestrator == null) return;
        var factura = e.CurrentObject as FacturaBase;
        if (factura == null) return;
        
        _facturaOrchestrator.Emitir(factura);
        
        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private void RevertirABorradorAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_facturaOrchestrator == null) return;
        var factura = e.CurrentObject as FacturaBase;
        if (factura == null) return;
        
        _facturaOrchestrator.RevertirABorrador(factura);
        
        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private async void EnviarVerifactuAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_veriFactuService == null || _facturaOrchestrator == null) return;

        var factura = e.CurrentObject as FacturaBase;
        if (factura == null) return;

        var result = await _facturaOrchestrator.EnviarAVerifactuAsync(ObjectSpace, factura, _veriFactuService);

        var message = result.Success ? "Factura enviada a VeriFactu correctamente." : $"Error al enviar la factura: {result.Message}";
        MostrarMensaje(message, result.Success ? InformationType.Success : InformationType.Error);

        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private void ContabilizarAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_facturaOrchestrator == null) return;
        var factura = e.CurrentObject as FacturaBase;
        if (factura == null) return;
        
        try 
        {
            _facturaOrchestrator.Contabilizar(factura);
            MostrarMensaje("Factura contabilizada correctamente.", InformationType.Success);
        }
        catch (Exception ex)
        {
            MostrarMensaje($"Error al contabilizar: {ex.Message}", InformationType.Error);
        }

        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private async void ProcesarFlujoCompletoAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_veriFactuService == null || _facturaOrchestrator == null) return;

        var factura = e.CurrentObject as FacturaBase;
        if (factura == null) return;

        var result = await _facturaOrchestrator.ProcesarHastaContabilizadaAsync(ObjectSpace, factura, _veriFactuService);
        
        var message = result.Success ? "Factura procesada correctamente." : $"Error al procesar la factura: {result.Message}";
        MostrarMensaje(message, result.Success ? InformationType.Success : InformationType.Error);

        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private async void ActualizarEstadoVerifactuAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var factura = (FacturaBase)e.CurrentObject;
        if (factura == null || _veriFactuService == null) return;

        var result = await _veriFactuService.GetStatusAsync(ObjectSpace, factura);

        if (result.Success)
        {
            MostrarMensaje(result.Message, InformationType.Success);
        }
        else
        {
            MostrarMensaje(result.Message, InformationType.Error);
        }
        
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
