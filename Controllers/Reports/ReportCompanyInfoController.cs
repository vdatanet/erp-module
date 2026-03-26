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
    public ReportCompanyInfoController()
    {
        var printReportAction = new SimpleAction(this, "PrintWithCompanyInfo", DevExpress.Persistent.Base.PredefinedCategory.Reports)
        {
            Caption = "Imprimir con Info Empresa",
            ImageName = "Action_Printing_Print"
        };
        printReportAction.Execute += PrintReportAction_Execute;
    }

    private void PrintReportAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        // Obtenemos el reporte por su nombre (ejemplo: "Factura", como se ve en los logs)
        var os = Application.CreateObjectSpace(typeof(ReportDataV2));
        var reportData = os.FindObject<ReportDataV2>(new DevExpress.Data.Filtering.BinaryOperator("DisplayName", "Factura"));
        
        if (reportData != null)
        {
            var controller = Frame.GetController<ReportServiceController>();
            if (controller != null)
            {
                // Al llamar a ShowPreview, el sistema XAF cargará el reporte.
                // La inyección de datos de la empresa ahora es AUTOMÁTICA y GLOBAL
                // gracias a la suscripción en erpModule.cs.
                string handle = ReportDataProvider.ReportsStorage.GetReportContainerHandle(reportData);
                controller.ShowPreview(handle);
            }
        }
        else
        {
            throw new UserFriendlyException("No se encontró el reporte 'Factura' en la base de datos.");
        }
    }
}
