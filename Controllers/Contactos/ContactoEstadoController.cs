using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using erp.Module.BusinessObjects.Contactos;
using DevExpress.ExpressApp.SystemModule;

namespace erp.Module.Controllers.Contactos;

/*
public class ContactoEstadoController : ViewController
{
    private ActionBase? _toggleAction;

    public ContactoEstadoController()
    {
        TargetObjectType = typeof(Contacto);
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        
        var controller = Frame.GetController<ObjectMethodActionsViewController>();
        if (controller != null && controller.Actions != null)
        {
            foreach (var action in controller.Actions)
            {
                if (action.Id == "Contacto.ToggleEstado" || action.Id == "ToggleEstado")
                {
                    _toggleAction = action;
                    // En ListView lo desactivamos (ocultamos)
                    action.Active["VisibleInListView"] = View is DetailView;
                    break;
                }
            }
        }

        if (_toggleAction != null && View is DetailView detailView)
        {
            detailView.CurrentObjectChanged += View_CurrentObjectChanged;
            View.ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
            UpdateActionState();
        }
    }

    protected override void OnDeactivated()
    {
        if (View is DetailView detailView)
        {
            detailView.CurrentObjectChanged -= View_CurrentObjectChanged;
            View.ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
        }
        _toggleAction = null;
        base.OnDeactivated();
    }

    private void ObjectSpace_ObjectChanged(object? sender, ObjectChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Contacto.Activo))
        {
            UpdateActionState();
        }
    }

    private void View_CurrentObjectChanged(object? sender, EventArgs e)
    {
        UpdateActionState();
    }

    private void UpdateActionState()
    {
        if (_toggleAction == null || View.CurrentObject is not Contacto contacto) return;

        if (contacto.Activo)
        {
            _toggleAction.Caption = "Desactivar";
            _toggleAction.ImageName = "Action_Delete";
        }
        else
        {
            _toggleAction.Caption = "Reactivar";
            _toggleAction.ImageName = "Action_Refresh";
        }
    }
}
*/
