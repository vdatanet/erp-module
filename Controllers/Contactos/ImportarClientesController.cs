using System.IO;
using System.Text;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.MultiTenancy;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Ventas;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.Controllers.Contactos;

public class ImportarClientesController : ViewController
{
    public ImportarClientesController()
    {
        TargetObjectType = typeof(Cliente);
        TargetViewType = ViewType.ListView;

        var importarClientesAction = new PopupWindowShowAction(this, "ImportarClientes", PredefinedCategory.RecordEdit)
        {
            Caption = "Importar Clientes",
            ToolTip = "Importar clientes desde un fichero CSV",
            ImageName = "Action_Import",
            TargetObjectType = typeof(Cliente),
            TargetViewType = ViewType.ListView
        };

        importarClientesAction.CustomizePopupWindowParams += ImportarClientesAction_CustomizePopupWindowParams;
        importarClientesAction.Execute += ImportarClientesAction_Execute;

        Actions.Add(importarClientesAction);
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        var tenantProvider = Application.ServiceProvider.GetService<ITenantProvider>();
        var tenantName = tenantProvider?.TenantName;
        Active["DemoOnly"] = tenantName == "demo";
    }

    private void ImportarClientesAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        var objectSpace = Application.CreateObjectSpace(typeof(ImportarClientesParameters));
        var parameters = new ImportarClientesParameters();
        e.View = Application.CreateDetailView(objectSpace, parameters);
        e.DialogController.SaveOnAccept = false;
    }

    private void ImportarClientesAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
    {
        var parameters = (ImportarClientesParameters)e.PopupWindowViewCurrentObject;
        if (parameters?.Archivo == null) return;

        using var stream = new MemoryStream();
        parameters.Archivo.SaveToStream(stream);
        stream.Position = 0;

        using var reader = new StreamReader(stream, Encoding.UTF8);
        var csvContent = reader.ReadToEnd();
        
        var importedCount = Cliente.ImportarDesdeCsv(((XPObjectSpace)ObjectSpace).Session, csvContent);

        if (importedCount > 0)
        {
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
            Application.ShowViewStrategy.ShowMessage($"Se han importado/actualizado {importedCount} clientes.", InformationType.Success);
        }
    }
}
