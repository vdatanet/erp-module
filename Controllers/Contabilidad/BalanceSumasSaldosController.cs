using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.Models.Contabilidad;
using erp.Module.Services.Contabilidad;
using System.Linq;

namespace erp.Module.Controllers.Contabilidad;

public class BalanceSumasSaldosController : WindowController
{
    public BalanceSumasSaldosController()
    {
        TargetWindowType = WindowType.Main;

        var showBalanceAction = new PopupWindowShowAction(this, "ShowBalanceSumasSaldos", PredefinedCategory.View)
        {
            Caption = "Balance Sumas y Saldos",
            ImageName = "BO_List",
            ToolTip = "Muestra el balance de sumas y saldos"
        };
        
        showBalanceAction.CustomizePopupWindowParams += ShowBalanceAction_CustomizePopupWindowParams;
        showBalanceAction.Execute += ShowBalanceAction_Execute;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        Application.CustomProcessShortcut += Application_CustomProcessShortcut;
    }

    protected override void OnDeactivated()
    {
        Application.CustomProcessShortcut -= Application_CustomProcessShortcut;
        base.OnDeactivated();
    }

    private void Application_CustomProcessShortcut(object? sender, CustomProcessShortcutEventArgs e)
    {
        if (e.Shortcut.ViewId == "BalanceSumasSaldosItem_ListView" && !e.Handled)
        {
            // Nota: Intentamos abrir la vista de parámetros antes de mostrar el balance
            // Pero por simplicidad ahora, dejamos que XAF abra la ListView. 
            // Podríamos forzar la carga de datos aquí si quisiéramos, pero mejor lo dejamos para la acción por ahora
            // o para un evento de ObjectsGetting si escalamos esto.
        }
    }

    private void ShowBalanceResult(BalanceSumasSaldosParameters parameters, ShowViewParameters svp)
    {
        var objectSpace = Application.CreateObjectSpace(typeof(BalanceSumasSaldosItem));
        var items = ContabilidadService.GetBalanceSumasSaldos(objectSpace, parameters);
        
        var listViewId = "BalanceSumasSaldosItem_ListView";
        var collectionSource = Application.CreateCollectionSource(objectSpace, typeof(BalanceSumasSaldosItem), listViewId);
        
        foreach (var item in items)
        {
            collectionSource.Add(item);
        }

        var listView = Application.CreateListView(listViewId, collectionSource, true);
        
        svp.CreatedView = listView;
        svp.TargetWindow = TargetWindow.Default;
    }

    private void ShowBalanceAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        var objectSpace = Application.CreateObjectSpace(typeof(BalanceSumasSaldosParameters));
        var parameters = new BalanceSumasSaldosParameters();
        
        var ejercicioActual = objectSpace.GetObjects<erp.Module.BusinessObjects.Contabilidad.Ejercicio>(
            DevExpress.Data.Filtering.CriteriaOperator.Parse("Anio = ?", DateTime.Today.Year)).FirstOrDefault();
        if (ejercicioActual != null)
        {
            parameters.Ejercicio = ejercicioActual;
            parameters.FechaInicio = ejercicioActual.FechaInicio;
            parameters.FechaFin = ejercicioActual.FechaFin;
        }

        var detailView = Application.CreateDetailView(objectSpace, parameters);
        detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
        e.View = detailView;
    }

    private void ShowBalanceAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
    {
        if (e.PopupWindowViewCurrentObject is BalanceSumasSaldosParameters parameters)
        {
            ShowBalanceResult(parameters, e.ShowViewParameters);
        }
    }
}
