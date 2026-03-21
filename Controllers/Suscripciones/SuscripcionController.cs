using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
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
        var hoy = DateTime.Today;
        var primerDiaMes = new DateTime(hoy.Year, hoy.Month, 1);
        var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

        var criteria = CriteriaOperator.Parse("Estado = ? AND ProximaFechaCobro >= ? AND ProximaFechaCobro <= ?", 
            EstadoSuscripcion.Activa, primerDiaMes, ultimoDiaMes);

        var suscripciones = ObjectSpace.GetObjects<Suscripcion>(criteria);
        int generadas = 0;

        foreach (var suscripcion in suscripciones)
        {
            try
            {
                suscripcion.GenerarFactura();
                generadas++;
            }
            catch (Exception ex)
            {
                // Log o manejo de error por suscripción individual si es necesario
                Tracing.Tracer.LogError(ex);
            }
        }

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
