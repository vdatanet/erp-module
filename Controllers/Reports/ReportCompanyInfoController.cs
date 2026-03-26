using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.Persistent.BaseImpl;
using erp.Module.Services.Configuraciones;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.Controllers.Reports;

/// <summary>
/// Controlador de ejemplo para mostrar cómo pasar la información de la empresa a un reporte en XAF.
/// </summary>
public class ReportCompanyInfoController : ViewController
{
    private readonly SingleChoiceAction printReportAction;

    public ReportCompanyInfoController()
    {
        TargetViewType = ViewType.Any;

        printReportAction = new SingleChoiceAction(this, "PrintInplaceWithCompanyInfo", DevExpress.Persistent.Base.PredefinedCategory.Reports)
        {
            Caption = "Imprimir",
            ImageName = "Action_Printing_Print",
            ItemType = SingleChoiceActionItemType.ItemIsOperation,
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        printReportAction.Execute += PrintReportAction_Execute;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        
        // Verificamos si estamos en el Host (donde existe el tipo Tenant)
        // Este controlador solo debe estar disponible en los tenants.
        if (ObjectSpace.IsKnownType(typeof(DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant)))
        {
            Active["AvailableInTenantOnly"] = false;
            return;
        }

        UpdateActionState();

        // No desactivar acciones si estamos en una vista de diseño/gestión de reportes
        if (View.ObjectTypeInfo != null && View.ObjectTypeInfo.Type == typeof(ReportDataV2))
        {
            return;
        }

        // Desactivamos los controladores nativos de reportes de XAF que gestionan la impresión Inplace
        foreach (var controller in Frame.Controllers)
        {
            var typeName = controller.GetType().Name;
            if (typeName == "PrintSelectionController" || 
                typeName == "InplaceReportCacheController" ||
                typeName == "InplaceReportsController" ||
                typeName == "InplaceReportController") // Añadimos InplaceReportController (singular) por si acaso
            {
                controller.Active["CustomPrintActionActive"] = false;
            }
        }

        // Además, desactivamos directamente cualquier acción nativa en la categoría de Reportes
        // que no sea la nuestra. Esto es una medida de seguridad adicional.
        // Solo lo hacemos para acciones que parezcan de impresión o ejecución de reportes inplace.
        var actionsToDisable = new[] { "ShowInReportV2", "EditReportV2", "ExecuteReportV2" }; // Acciones comunes de impresión/visualización
        
        foreach (var controller in Frame.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                if (action.Category == DevExpress.Persistent.Base.PredefinedCategory.Reports.ToString() && 
                    action.Id != printReportAction.Id)
                {
                    // Si el Id de la acción contiene "Inplace" o es una de las conocidas de visualización, la desactivamos
                    if (action.Id.Contains("Inplace") || action.Id == "ShowInReportV2")
                    {
                        action.Active["CustomPrintActionActive"] = false;
                    }
                }
            }
        }
    }

    private void UpdateActionState()
    {
        printReportAction.Items.Clear();

        if (View != null && View.ObjectTypeInfo != null)
        {
            // Verificamos si el ObjectSpace actual puede manejar el tipo ReportDataV2
            // Esto evita errores en el Host (donde no existen reportes de negocio)
            if (!ObjectSpace.IsKnownType(typeof(ReportDataV2)))
            {
                printReportAction.Active["HasInplaceReports"] = false;
                return;
            }

            using var os = Application.CreateObjectSpace(typeof(ReportDataV2));
            var reports = os.GetObjects<ReportDataV2>(DevExpress.Data.Filtering.CriteriaOperator.Parse("IsInplaceReport = true AND DataTypeName = ?", View.ObjectTypeInfo.Type.FullName));

            foreach (var report in reports)
            {
                var item = new ChoiceActionItem(report.DisplayName, report)
                {
                    ImageName = "Action_Printing_Print"
                };
                printReportAction.Items.Add(item);
            }
        }

        printReportAction.Active["HasInplaceReports"] = printReportAction.Items.Count > 0;
    }

    private void PrintReportAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
    {
        if (e.SelectedChoiceActionItem?.Data is ReportDataV2 reportData)
        {
            var controller = Frame.GetController<ReportServiceController>();
            if (controller != null)
            {
                var reportStorage = ReportDataProvider.GetReportStorage(Application.ServiceProvider);
                string handle = reportStorage.GetReportContainerHandle(reportData);
                
                // Si queremos que el reporte se filtre por el objeto seleccionado (comportamiento Inplace estándar)
                // pasamos los criterios de selección.
                var criteria = DevExpress.Data.Filtering.CriteriaOperator.Parse("Oid = ?", ObjectSpace.GetKeyValue(View.CurrentObject));
                controller.ShowPreview(handle, criteria);
            }
        }
    }
}
