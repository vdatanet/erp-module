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
        _validarAction.Active["EstadoValido"] = selectedFacturas.Any(f => f.EstadoFactura == EstadoFactura.Borrador);
        _revertirABorradorAction.Active["EstadoValido"] = selectedFacturas.Any(f => f.EstadoFactura == EstadoFactura.Validada);
        _emitirAction.Active["EstadoValido"] = selectedFacturas.Any(f => f.EstadoFactura == EstadoFactura.Validada);
        _enviarVerifactuAction.Active["EstadoValido"] = selectedFacturas.Any(f => f.EstadoFactura == EstadoFactura.Emitida && 
                                                                           f.EstadoVeriFactu != EstadoVeriFactu.AceptadaVeriFactu && 
                                                                           f.EstadoVeriFactu != EstadoVeriFactu.EnviadaVeriFactu);
        
        _contabilizarAction.Active["EstadoValido"] = selectedFacturas.Any(f => (f.EstadoFactura == EstadoFactura.Enviada || 
                                                                              f.EstadoFactura == EstadoFactura.VeriFactuNoNecesario ||
                                                                              f.EstadoVeriFactu == EstadoVeriFactu.AceptadaVeriFactu || 
                                                                              f.EstadoVeriFactu == EstadoVeriFactu.EnviadaVeriFactu ||
                                                                              f.EstadoVeriFactu == EstadoVeriFactu.PendienteVeriFactu) &&
                                                                             f.EstadoFactura != EstadoFactura.Contabilizada);
    }

    private void ValidarAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var selectedFacturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        if (!selectedFacturas.Any()) return;

        foreach (var factura in selectedFacturas)
        {
            if (factura.EstadoFactura == EstadoFactura.Borrador)
            {
                factura.Validar();
            }
        }
        
        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private void EmitirAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var selectedFacturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        if (!selectedFacturas.Any()) return;

        foreach (var factura in selectedFacturas)
        {
            if (factura.EstadoFactura == EstadoFactura.Validada)
            {
                factura.Emitir();
            }
        }
        
        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private void RevertirABorradorAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var selectedFacturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        if (!selectedFacturas.Any()) return;

        foreach (var factura in selectedFacturas)
        {
            if (factura.EstadoFactura == EstadoFactura.Validada)
            {
                factura.RevertirABorrador();
            }
        }
        
        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private async void EnviarVerifactuAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (_veriFactuService == null) return;

        var selectedFacturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        if (!selectedFacturas.Any()) return;

        var successCount = 0;
        var totalCount = selectedFacturas.Count;
        var lastErrorMessage = string.Empty;

        foreach (var factura in selectedFacturas)
        {
            if (factura.EstadoFactura == EstadoFactura.Emitida && 
                factura.EstadoVeriFactu != EstadoVeriFactu.AceptadaVeriFactu && 
                factura.EstadoVeriFactu != EstadoVeriFactu.EnviadaVeriFactu)
            {
                var result = await factura.EnviarVerifactuAsync(ObjectSpace, _veriFactuService);
                if (result.Success)
                {
                    successCount++;
                }
                else
                {
                    lastErrorMessage = result.Message;
                }
            }
            else
            {
                totalCount--; // No era procesable por esta acción
            }
        }

        if (totalCount > 0)
        {
            var message = totalCount == 1 
                ? (successCount == 1 ? "Factura enviada a VeriFactu correctamente." : $"Error al enviar la factura: {lastErrorMessage}")
                : $"Proceso de envío finalizado. {successCount} de {totalCount} facturas enviadas correctamente.";

            var options = new MessageOptions
            {
                Duration = 5000,
                Message = message,
                Type = successCount == totalCount ? InformationType.Success : (successCount > 0 ? InformationType.Warning : InformationType.Error),
                Web = { Position = InformationPosition.Right }
            };
            Application.ShowViewStrategy.ShowMessage(options);
        }

        ObjectSpace.CommitChanges();
        View.Refresh();
    }

    private void ContabilizarAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var selectedFacturas = e.SelectedObjects.Cast<FacturaBase>().ToList();
        if (!selectedFacturas.Any()) return;

        var successCount = 0;
        var totalCount = selectedFacturas.Count;
        var errorMessages = new List<string>();

        foreach (var factura in selectedFacturas)
        {
            bool puedeContabilizar = (factura.EstadoFactura == EstadoFactura.Enviada || 
                                     factura.EstadoFactura == EstadoFactura.VeriFactuNoNecesario ||
                                     factura.EstadoVeriFactu == EstadoVeriFactu.AceptadaVeriFactu || 
                                     factura.EstadoVeriFactu == EstadoVeriFactu.EnviadaVeriFactu ||
                                     factura.EstadoVeriFactu == EstadoVeriFactu.PendienteVeriFactu) &&
                                    factura.EstadoFactura != EstadoFactura.Contabilizada;

            if (puedeContabilizar)
            {
                try
                {
                    factura.Contabilizar();
                    successCount++;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"{factura.Secuencia}: {ex.Message}");
                }
            }
            else
            {
                totalCount--;
            }
        }

        if (totalCount > 0)
        {
            var message = totalCount == 1 
                ? (successCount == 1 ? "Factura contabilizada correctamente." : $"Error al contabilizar: {errorMessages.FirstOrDefault()}")
                : $"Contabilización finalizada. {successCount} de {totalCount} facturas contabilizadas correctamente.";

            if (errorMessages.Any() && totalCount > 1)
            {
                message += " Revise los errores individuales.";
            }

            var options = new MessageOptions
            {
                Duration = 5000,
                Message = message,
                Type = successCount == totalCount ? InformationType.Success : (successCount > 0 ? InformationType.Warning : InformationType.Error),
                Web = { Position = InformationPosition.Right }
            };
            Application.ShowViewStrategy.ShowMessage(options);
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

        var options = new MessageOptions
        {
            Duration = 5000,
            Message = message,
            Type = successCount == totalCount ? InformationType.Success : (successCount > 0 ? InformationType.Warning : InformationType.Error),
            Web = { Position = InformationPosition.Right }
        };
        Application.ShowViewStrategy.ShowMessage(options);

        ObjectSpace.CommitChanges();
        View.Refresh();
    }
}
