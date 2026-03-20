using System.IO;
using System.Text;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Ventas;

namespace erp.Module.Controllers.Ventas;

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
        
        var lines = csvContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1) return; // Encabezado o vacío

        var objectSpace = View.ObjectSpace;
        var importedCount = 0;

        // Asumimos formato: Nombre;NIF;Email;Telefono;Direccion
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            var values = line.Split(';');
            if (values.Length < 2) continue;

            var nombre = values[0].Trim();
            var nif = values[1].Trim();
            var email = values.Length > 2 ? values[2].Trim() : string.Empty;
            var telefono = values.Length > 3 ? values[3].Trim() : string.Empty;
            var direccion = values.Length > 4 ? values[4].Trim() : string.Empty;

            if (string.IsNullOrEmpty(nombre)) continue;

            // Buscar si ya existe por NIF (si tiene) o Nombre
            Cliente cliente = null;
            if (!string.IsNullOrEmpty(nif))
            {
                cliente = objectSpace.FindObject<Cliente>(CriteriaOperator.Parse("Nif = ?", nif));
            }

            if (cliente == null)
            {
                cliente = objectSpace.CreateObject<Cliente>();
                cliente.Nombre = nombre;
                cliente.Nif = nif;
            }

            if (!string.IsNullOrEmpty(email)) cliente.CorreoElectronico = email;
            if (!string.IsNullOrEmpty(telefono)) cliente.Telefono = telefono;
            if (!string.IsNullOrEmpty(direccion)) cliente.Direccion = direccion;

            importedCount++;
        }

        if (importedCount > 0)
        {
            objectSpace.CommitChanges();
            View.ObjectSpace.Refresh();
            Application.ShowViewStrategy.ShowMessage($"Se han importado/actualizado {importedCount} clientes.", InformationType.Success);
        }
    }
}
