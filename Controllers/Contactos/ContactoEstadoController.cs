using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using erp.Module.BusinessObjects.Contactos;

namespace erp.Module.Controllers.Contactos;

public class ContactoEstadoController : ViewController
{
    public ContactoEstadoController()
    {
        TargetObjectType = typeof(Contacto);
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        // Cuando estamos en una ListView, desactivamos las acciones de Desactivar y Reactivar
        // que vienen definidas por atributo en el Business Object.
        if (View is ListView)
        {
            foreach (var action in Actions)
            {
                if (action.Id == "Desactivar" || action.Id == "Reactivar")
                {
                    action.Active["VisibleInListView"] = false;
                }
            }
        }
        else if (View is DetailView)
        {
            foreach (var action in Actions)
            {
                if (action.Id == "Desactivar" || action.Id == "Reactivar")
                {
                    action.Active["VisibleInListView"] = true;
                }
            }
        }
    }
}
