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
            ConfirmationMessage = "¿Desea validar esta factura?",
            ImageName = "BO_Validation",
            TargetObjectsCriteria = "EstadoFactura = 'Borrador'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _validarAction.Execute += ValidarAction_Execute;

        _revertirABorradorAction = new SimpleAction(this, "Factura_RevertirABorrador", PredefinedCategory.Edit)
        {
            Caption = "Revertir a Borrador",
            ConfirmationMessage = "¿Desea revertir esta factura a borrador?",
            ImageName = "Undo",
            TargetObjectsCriteria = "EstadoFactura = 'Validada'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _revertirABorradorAction.Execute += RevertirABorradorAction_Execute;

        _emitirAction = new SimpleAction(this, "Factura_Emitir", PredefinedCategory.Edit)
        {
            Caption = "Emitir",
            ConfirmationMessage = "¿Desea emitir esta factura? (Se asignará número definitivo y fecha de emisión)",
            ImageName = "Redo",
            TargetObjectsCriteria = "EstadoFactura = 'Validada'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _emitirAction.Execute += EmitirAction_Execute;

        _enviarVerifactuAction = new SimpleAction(this, "Factura_EnviarVerifactu", PredefinedCategory.Edit)
        {
            Caption = "Enviar a VeriFactu",
            ConfirmationMessage = "¿Desea enviar esta factura a VeriFactu?",
            ImageName = "Actions_Send",
            TargetObjectsCriteria = "EstadoFactura = 'Emitida'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _enviarVerifactuAction.Execute += EnviarVerifactuAction_Execute;

        _contabilizarAction = new SimpleAction(this, "Factura_Contabilizar", PredefinedCategory.Edit)
        {
            Caption = "Contabilizar",
            ConfirmationMessage = "¿Desea generar el asiento contable para esta factura?",
            ImageName = "Accounting",
            TargetObjectsCriteria = "EstadoFactura = 'Enviada' OR (EstadoVeriFactu = 'AceptadaVeriFactu' OR EstadoVeriFactu = 'EnviadaVeriFactu')",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _contabilizarAction.Execute += ContabilizarAction_Execute;

        _procesarFlujoCompletoAction = new SimpleAction(this, "Factura_ProcesarFlujoCompleto", PredefinedCategory.Edit)
        {
            Caption = "Procesar y Contabilizar",
            ConfirmationMessage = "¿Desea procesar la factura hasta su contabilización final? (Validar -> Emitir -> VeriFactu -> Contabilizar)",
            ImageName = "Action_Workflow",
            TargetObjectsCriteria = "EstadoFactura != 'Contabilizada'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
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
        ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
        ActualizarVisibilidadAcciones();
    }

    protected override void OnDeactivated()
    {
        View.CurrentObjectChanged -= View_CurrentObjectChanged;
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
        if (View.CurrentObject is not FacturaBase factura)
        {
            _validarAction.Active["EstadoValido"] = false;
            _emitirAction.Active["EstadoValido"] = false;
            _revertirABorradorAction.Active["EstadoValido"] = false;
            _enviarVerifactuAction.Active["EstadoValido"] = false;
            _contabilizarAction.Active["EstadoValido"] = false;
            _procesarFlujoCompletoAction.Active["EstadoValido"] = false;
            return;
        }

        _validarAction.Active["EstadoValido"] = factura.EstadoFactura == EstadoFactura.Borrador;
        _revertirABorradorAction.Active["EstadoValido"] = factura.EstadoFactura == EstadoFactura.Validada;
        _emitirAction.Active["EstadoValido"] = factura.EstadoFactura == EstadoFactura.Validada;
        _enviarVerifactuAction.Active["EstadoValido"] = factura.EstadoFactura == EstadoFactura.Emitida && 
                                                      factura.EstadoVeriFactu != EstadoVeriFactu.AceptadaVeriFactu && 
                                                      factura.EstadoVeriFactu != EstadoVeriFactu.EnviadaVeriFactu;
        
        _contabilizarAction.Active["EstadoValido"] = (factura.EstadoFactura == EstadoFactura.Enviada || 
                                                      factura.EstadoVeriFactu == EstadoVeriFactu.AceptadaVeriFactu || 
                                                      factura.EstadoVeriFactu == EstadoVeriFactu.EnviadaVeriFactu) &&
                                                     factura.EstadoFactura != EstadoFactura.Contabilizada;

        _procesarFlujoCompletoAction.Active["EstadoValido"] = factura.EstadoFactura != EstadoFactura.Contabilizada;
    }

    private void ValidarAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (e.CurrentObject is not FacturaBase factura) return;
        
        factura.Validar();
        ObjectSpace.CommitChanges();
        
        View.Refresh();
    }

    private void EmitirAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (e.CurrentObject is not FacturaBase factura) return;

        factura.Emitir();
        ObjectSpace.CommitChanges();

        View.Refresh();
    }

    private void RevertirABorradorAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (e.CurrentObject is not FacturaBase factura) return;

        factura.RevertirABorrador();
        ObjectSpace.CommitChanges();
        
        View.Refresh();
    }

    private async void EnviarVerifactuAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (e.CurrentObject is not FacturaBase factura || _veriFactuService == null) return;

        var result = await factura.EnviarVerifactuAsync(ObjectSpace, _veriFactuService);

        var options = new MessageOptions
        {
            Duration = 5000,
            Message = result.Message,
            Type = result.Success ? InformationType.Success : InformationType.Error,
            Web = { Position = InformationPosition.Right }
        };
        Application.ShowViewStrategy.ShowMessage(options);

        // El servicio ya hace su propio CommitChanges() si llega a la fase de envío.
        // Solo hacemos un commit adicional y Refresh para asegurar que la UI esté sincronizada
        // con el estado actual de la factura (ej. Rechazada, Aceptada, etc.).
        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private void ContabilizarAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (e.CurrentObject is not FacturaBase factura) return;

        if (factura.EstadoVeriFactu != EstadoVeriFactu.EnviadaVeriFactu && 
            factura.EstadoVeriFactu != EstadoVeriFactu.AceptadaVeriFactu)
        {
            throw new UserFriendlyException("La factura debe haber sido enviada a VeriFactu antes de contabilizarse.");
        }

        factura.Contabilizar();
        ObjectSpace.CommitChanges();
        
        // Refrescar vistas para mostrar cambios en asiento y apuntes
        View.Refresh();
    }

    private async void ProcesarFlujoCompletoAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (e.CurrentObject is not FacturaBase factura || _veriFactuService == null || _facturaOrchestrator == null) return;

        var result = await _facturaOrchestrator.ProcesarHastaContabilizadaAsync(ObjectSpace, factura, _veriFactuService);

        var options = new MessageOptions
        {
            Duration = 5000,
            Message = result.Message,
            Type = result.Success ? InformationType.Success : InformationType.Error,
            Web = { Position = InformationPosition.Right }
        };
        Application.ShowViewStrategy.ShowMessage(options);

        ObjectSpace.CommitChanges();
        View.Refresh();
    }
}
