using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.Models.Contabilidad;
using erp.Module.Services.Contabilidad;
using System.Linq;

namespace erp.Module.Controllers.Contabilidad;

public class ExtractoCuentaController : ViewController
{
    public ExtractoCuentaController()
    {
        TargetObjectType = typeof(CuentaContable);

        var showExtractoAction = new PopupWindowShowAction(this, "ShowExtractoCuenta", PredefinedCategory.View)
        {
            Caption = "Extracto de Cuenta",
            ImageName = "BO_List",
            ToolTip = "Muestra el extracto de la cuenta seleccionada",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        
        showExtractoAction.CustomizePopupWindowParams += ShowExtractoAction_CustomizePopupWindowParams;
        showExtractoAction.Execute += ShowExtractoAction_Execute;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        if (View.ObjectTypeInfo.Type == typeof(ExtractoCuentaItem))
        {
            var refreshExtractoAction = new PopupWindowShowAction(this, "RefreshExtractoCuenta", PredefinedCategory.View)
            {
                Caption = "Cambiar Parámetros",
                ImageName = "Action_Refresh",
                ToolTip = "Cambia los parámetros del extracto"
            };
            refreshExtractoAction.CustomizePopupWindowParams += ShowExtractoAction_CustomizePopupWindowParams;
            refreshExtractoAction.Execute += ShowExtractoAction_Execute;
        }
    }

    private void ShowExtractoResult(ExtractoCuentaParameters parameters, ShowViewParameters svp)
    {
        var objectSpace = Application.CreateObjectSpace(typeof(ExtractoCuentaItem));
        var items = ContabilidadService.GetExtractoCuenta(objectSpace, parameters);
        
        var listViewId = "ExtractoCuentaItem_ListView";
        var collectionSource = Application.CreateCollectionSource(objectSpace, typeof(ExtractoCuentaItem), listViewId);
        
        foreach (var item in items)
        {
            collectionSource.Add(item);
        }

        var listView = Application.CreateListView(listViewId, collectionSource, true);
        listView.Caption = $"Extracto: {parameters.CuentaContable?.Codigo} - {parameters.CuentaContable?.Nombre}";
        
        svp.CreatedView = listView;
        svp.TargetWindow = TargetWindow.Default;
    }

    private void ShowExtractoAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        var objectSpace = Application.CreateObjectSpace(typeof(ExtractoCuentaParameters));
        var parameters = new ExtractoCuentaParameters();
        
        if (View.CurrentObject is CuentaContable cuenta)
        {
            parameters.CuentaContable = objectSpace.GetObject(cuenta);
        }
        else if (View.Id == "ExtractoCuentaItem_ListView")
        {
            // Si estamos en el listview de extracto, podríamos intentar recuperar la cuenta del título o similar,
            // pero por ahora dejamos que el usuario la seleccione si no viene de una cuenta.
        }

        var ejercicioActual = objectSpace.GetObjects<Ejercicio>(
            DevExpress.Data.Filtering.CriteriaOperator.Parse("Anio = ?", DateTime.Today.Year)).FirstOrDefault();
        if (ejercicioActual != null)
        {
            parameters.FechaInicio = ejercicioActual.FechaInicio;
            parameters.FechaFin = ejercicioActual.FechaFin;
        }

        var detailView = Application.CreateDetailView(objectSpace, parameters);
        detailView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
        e.View = detailView;
    }

    private void ShowExtractoAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
    {
        if (e.PopupWindowViewCurrentObject is ExtractoCuentaParameters parameters)
        {
            ShowExtractoResult(parameters, e.ShowViewParameters);
        }
    }
}
