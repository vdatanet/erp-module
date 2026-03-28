using System.Linq;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
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
    private VeriFactuService? _veriFactuService;

    public FacturaLifecycleController()
    {
        TargetObjectType = typeof(FacturaBase);
        TargetViewType = ViewType.Any;

        _validarAction = new SimpleAction(this, "Factura_Validar", PredefinedCategory.Edit)
        {
            Caption = "Validar",
            ConfirmationMessage = "¿Desea validar esta factura?",
            ImageName = "Action_Validate",
            TargetObjectsCriteria = "EstadoFactura = 'Borrador'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _validarAction.Execute += ValidarAction_Execute;

        _revertirABorradorAction = new SimpleAction(this, "Factura_RevertirABorrador", PredefinedCategory.Edit)
        {
            Caption = "Revertir a Borrador",
            ConfirmationMessage = "¿Desea revertir esta factura a borrador?",
            ImageName = "Action_Undo",
            TargetObjectsCriteria = "EstadoFactura = 'Validada'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _revertirABorradorAction.Execute += RevertirABorradorAction_Execute;

        _emitirAction = new SimpleAction(this, "Factura_Emitir", PredefinedCategory.Edit)
        {
            Caption = "Emitir",
            ConfirmationMessage = "¿Desea emitir esta factura? (Se asignará número definitivo y fecha de emisión)",
            ImageName = "Action_LinkUnlink_Link",
            TargetObjectsCriteria = "EstadoFactura = 'Validada'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _emitirAction.Execute += EmitirAction_Execute;

        _enviarVerifactuAction = new SimpleAction(this, "Factura_EnviarVerifactu", PredefinedCategory.Edit)
        {
            Caption = "Enviar a VeriFactu",
            ConfirmationMessage = "¿Desea enviar esta factura a VeriFactu?",
            ImageName = "Action_Send",
            TargetObjectsCriteria = "EstadoFactura = 'Emitida'",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _enviarVerifactuAction.Execute += EnviarVerifactuAction_Execute;

        _contabilizarAction = new SimpleAction(this, "Factura_Contabilizar", PredefinedCategory.Edit)
        {
            Caption = "Contabilizar",
            ConfirmationMessage = "¿Desea generar el asiento contable para esta factura?",
            ImageName = "Action_LinkUnlink_Link",
            TargetObjectsCriteria = "EstadoFactura = 'Enviada' OR (EstadoVeriFactu = 'AceptadaVeriFactu' OR EstadoVeriFactu = 'EnviadaVeriFactu')",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        _contabilizarAction.Execute += ContabilizarAction_Execute;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        _veriFactuService = Application.ServiceProvider.GetRequiredService<VeriFactuService>();

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
    }

    protected override void OnDeactivated()
    {
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
}
