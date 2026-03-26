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
        var empresaProvider = Application.ServiceProvider.GetRequiredService<IInformacionEmpresaProvider>();
        
        // Obtenemos el reporte (ejemplo por nombre)
        var os = Application.CreateObjectSpace(typeof(ReportDataV2));
        var reportData = os.FindObject<ReportDataV2>(new DevExpress.Data.Filtering.BinaryOperator("DisplayName", "Mi Reporte"));
        
        if (reportData != null)
        {
            // Obtenemos el DTO de la empresa
            var companyDto = empresaProvider.GetInformacionEmpresaDto(os);
            
            // En XAF, para pasar parámetros antes de mostrar el reporte, se recomienda usar el evento SetupBeforePrint del ReportsDataSourceHelper
            // o suscribirse a eventos del ReportServiceController.
            // Para la Web API, ya lo hemos automatizado en el ReportController.
            var controller = Frame.GetController<ReportServiceController>();
            if (controller != null)
            {
                // Ejemplo conceptual: en XAF se suele delegar la carga al sistema, 
                // pero podemos intervenir en la creación del reporte si es necesario.
                controller.ShowPreview(reportData.Oid.ToString());
            }
        }
    }
}
