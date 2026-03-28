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
            Caption = "Imprimir Empresa",
            ImageName = "Action_Printing_Print",
            ItemType = SingleChoiceActionItemType.ItemIsOperation,
            SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
        };
        printReportAction.Execute += PrintReportAction_Execute;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        
        // Verificamos si estamos en el Host (donde existe el tipo Tenant y el TenantId es nulo)
        // Este controlador solo debe estar disponible en los tenants.
        if (ObjectSpace.IsKnownType(typeof(DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant)))
        {
            var isHost = true;
            try
            {
                var tenantProvider = Application.ServiceProvider.GetService<DevExpress.ExpressApp.MultiTenancy.ITenantProvider>();
                if (tenantProvider != null && tenantProvider.TenantId != null)
                {
                    isHost = false;
                }
            }
            catch
            {
                // Si hay error al obtener el provider, por seguridad asumimos que podría ser el host 
                // si el tipo Tenant es conocido.
            }

            if (isHost)
            {
                Active["AvailableInTenantOnly"] = false;
                return;
            }
        }

        UpdateActionState();

        // No desactivar acciones si estamos en una vista de diseño/gestión de reportes
        if (View.ObjectTypeInfo != null && View.ObjectTypeInfo.Type == typeof(ReportDataV2))
        {
            return;
        }

        // Volvemos a desactivar los controladores nativos para que nuestra acción sea la única
        foreach (var controller in Frame.Controllers)
        {
            DisableNativeReportController(controller);
        }

        // XAF a veces actualiza el estado de las acciones después de OnActivated o cuando cambia el frame.
        // Forzamos la actualización de nuevo en SelectionChanged y otros eventos si fuera necesario.
        View.SelectionChanged += View_SelectionChanged;
        
        // También nos aseguramos de que nuestra acción esté activa
        printReportAction.Active["CustomPrintActionActive"] = true;
    }

    private void View_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateActionState();
        
        foreach (var controller in Frame.Controllers)
        {
            DisableNativeReportController(controller);
        }
    }

    private void DisableNativeReportController(Controller controller)
    {
        var typeName = controller.GetType().Name;
        // Solo desactivamos controladores que sabemos que gestionan reportes inplace nativos
        if (typeName == "PrintSelectionController" || 
            typeName == "InplaceReportCacheController" ||
            typeName == "InplaceReportsController" ||
            typeName == "InplaceReportController")
        {
            controller.Active["CustomPrintActionActive"] = false;
        }

        foreach (var action in controller.Actions)
        {
            // Desactivamos la acción nativa ShowInReportV2 si no es la nuestra
            // Asegurándonos de no desactivar nuestra propia acción accidentalmente
            if (action == printReportAction) continue;

            if (action.Id == "ShowInReportV2" || action.Id == "PrintSelection" || action.Id == "InplaceReportV2")
            {
                action.Active["CustomPrintActionActive"] = false;
            }
        }
    }

    protected override void OnDeactivated()
    {
        View.SelectionChanged -= View_SelectionChanged;
        base.OnDeactivated();
    }

    private void UpdateActionState()
    {
        printReportAction.Items.Clear();

        if (View?.ObjectTypeInfo == null)
        {
            printReportAction.Active["HasInplaceReports"] = false;
            return;
        }

        // Verificamos si el ObjectSpace actual puede manejar el tipo ReportDataV2
        // Esto evita errores en el Host (donde no existen reportes de negocio)
        if (!ObjectSpace.IsKnownType(typeof(ReportDataV2)))
        {
            printReportAction.Active["HasInplaceReports"] = false;
            return;
        }

        var typeFullName = View.ObjectTypeInfo.Type.FullName;
        var modelFullName = View.ObjectTypeInfo.FullName;
        var typeToSearch = typeFullName;
        
        // XAF a veces usa tipos proxy para XPO. Intentamos obtener el tipo base.
        if (View.ObjectTypeInfo.Type.Namespace == "DevExpress.Xpo" && View.ObjectTypeInfo.Type.Name.EndsWith("Proxy"))
        {
            // Si es un proxy, el FullName del ObjectTypeInfo suele ser el correcto del objeto de negocio
            typeToSearch = modelFullName;
        }

        var typesToSearch = new List<string>();
        if (!string.IsNullOrEmpty(typeToSearch)) typesToSearch.Add(typeToSearch);
        if (!string.IsNullOrEmpty(typeFullName)) typesToSearch.Add(typeFullName);
        if (!string.IsNullOrEmpty(modelFullName)) typesToSearch.Add(modelFullName);
        
        // Añadimos tipos base para ser más robustos, similar a cómo XAF busca reportes inplace
        var currentType = View.ObjectTypeInfo;
        while (currentType.Base != null && currentType.Base.IsPersistent)
        {
            if (currentType.Base.Type?.FullName != null) typesToSearch.Add(currentType.Base.Type.FullName);
            if (currentType.Base.FullName != null) typesToSearch.Add(currentType.Base.FullName);
            currentType = currentType.Base;
        }

        // También incluimos interfaces si el reporte estuviera definido para una interfaz de negocio
        foreach (var itf in View.ObjectTypeInfo.Type.GetInterfaces())
        {
            if (itf.FullName != null) typesToSearch.Add(itf.FullName);
        }
        
        // Filtrar nulos y duplicados
        var finalTypes = typesToSearch.Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();

        using var os = Application.CreateObjectSpace(typeof(ReportDataV2));
        var criteria = DevExpress.Data.Filtering.CriteriaOperator.Parse("IsInplaceReport = true") & 
                       new DevExpress.Data.Filtering.InOperator("DataTypeName", finalTypes);
        var reports = os.GetObjects<ReportDataV2>(criteria);

        foreach (var report in reports)
        {
            var item = new ChoiceActionItem(report.DisplayName, report.Oid)
            {
                ImageName = "Action_Printing_Print"
            };
            printReportAction.Items.Add(item);
        }

        printReportAction.Active["HasInplaceReports"] = printReportAction.Items.Count > 0;
    }

    private void PrintReportAction_Execute(object sender, SingleChoiceActionExecuteEventArgs e)
    {
        if (e.SelectedChoiceActionItem?.Data is not Guid reportDataOid) return;

        var reportData = ObjectSpace.GetObjectByKey<ReportDataV2>(reportDataOid);
        if (reportData == null) return;

        var controller = Frame.GetController<ReportServiceController>();
        if (controller == null) return;

        var reportStorage = ReportDataProvider.GetReportStorage(Application.ServiceProvider);
        string handle = reportStorage.GetReportContainerHandle(reportData);

        // Si queremos que el reporte se filtre por los objetos seleccionados (comportamiento Inplace estándar)
        // pasamos los criterios de selección.
        var selectedOids = new List<object>();
        foreach (var obj in e.SelectedObjects)
        {
            selectedOids.Add(ObjectSpace.GetKeyValue(obj));
        }

        // Usamos el nombre del miembro clave del tipo de objeto de la vista
        var keyName = View.ObjectTypeInfo.KeyMember.Name;
        var criteria = new DevExpress.Data.Filtering.InOperator(keyName, selectedOids);

        // Llamamos a ShowPreview con el handle y el criterio. 
        // La inyección de parámetros de empresa se realiza automáticamente en erpModule 
        // a través de ReportsDataSourceHelper.BeforeShowPreview.
        controller.ShowPreview(handle, criteria);
    }

}
