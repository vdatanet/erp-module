using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Suscripciones;

namespace erp.Module.Controllers.Suscripciones;

public class SuscripcionController : ViewController
{
    private readonly SimpleAction _generarFacturaAction;
    private readonly SimpleAction _procesarFacturacionMensualAction;

    public SuscripcionController()
    {
        TargetObjectType = typeof(Suscripcion);

        _generarFacturaAction = new SimpleAction(this, "Suscripcion_GenerarFactura", PredefinedCategory.Edit)
        {
            Caption = "Generar Factura",
            ImageName = "Action_Generate",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject,
            ConfirmationMessage = "¿Desea generar la factura para esta suscripción?"
        };
        _generarFacturaAction.Execute += GenerarFacturaAction_Execute;

        _procesarFacturacionMensualAction = new SimpleAction(this, "Suscripcion_ProcesarFacturacionMensual", PredefinedCategory.View)
        {
            Caption = "Procesar Facturación Mensual",
            ImageName = "Action_Workflow",
            TargetViewType = ViewType.ListView,
            ConfirmationMessage = "¿Desea procesar todas las suscripciones vencidas en el mes actual?"
        };
        _procesarFacturacionMensualAction.Execute += ProcesarFacturacionMensualAction_Execute;
    }

    private void GenerarFacturaAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var suscripcion = (Suscripcion)e.CurrentObject;
        try
        {
            var factura = suscripcion.GenerarFactura();
            ObjectSpace.CommitChanges();
            
            // Opcional: Mostrar la factura creada
            var detailView = Application.CreateDetailView(ObjectSpace, factura);
            e.ShowViewParameters.CreatedView = detailView;
            
            MessageOptions options = new MessageOptions
            {
                Duration = 3000,
                Message = $"Factura generada con éxito para {suscripcion.NombreDisplay}",
                Type = InformationType.Success
            };
            Application.ShowViewStrategy.ShowMessage(options);
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException(ex.Message);
        }
    }

    private void ProcesarFacturacionMensualAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        int generadas = Suscripcion.ProcesarFacturacionMensual(((XPObjectSpace)ObjectSpace).Session);

        if (generadas > 0)
        {
            ObjectSpace.CommitChanges();
            MessageOptions options = new MessageOptions
            {
                Duration = 5000,
                Message = $"Se han generado {generadas} facturas con éxito.",
                Type = InformationType.Success
            };
            Application.ShowViewStrategy.ShowMessage(options);
        }
        else
        {
            MessageOptions options = new MessageOptions
            {
                Duration = 5000,
                Message = "No se encontraron suscripciones vencidas en este mes para procesar.",
                Type = InformationType.Info
            };
            Application.ShowViewStrategy.ShowMessage(options);
        }
    }
}
