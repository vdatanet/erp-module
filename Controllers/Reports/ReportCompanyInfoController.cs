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
        UpdateActionState();

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
        foreach (var controller in Frame.Controllers)
        {
            foreach (var action in controller.Actions)
            {
                if (action.Category == DevExpress.Persistent.Base.PredefinedCategory.Reports.ToString() && 
                    action.Id != printReportAction.Id)
                {
                    action.Active["CustomPrintActionActive"] = false;
                }
            }
        }
    }

    private void UpdateActionState()
    {
        printReportAction.Items.Clear();

        if (View != null && View.ObjectTypeInfo != null)
        {
            var os = Application.CreateObjectSpace(typeof(ReportDataV2));
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
