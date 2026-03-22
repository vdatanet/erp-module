using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using erp.Module.BusinessObjects.Contabilidad;
using System.Linq;

namespace erp.Module.Controllers.Contabilidad;

public class ContabilidadAccionesController : ViewController
{
    private readonly SimpleAction togglePublicadoAction;
    private readonly SimpleAction toggleEstadoEjercicioAction;

    public ContabilidadAccionesController()
    {
        togglePublicadoAction = new SimpleAction(this, "TogglePublicadoAction", "Edit")
        {
            Caption = "Cambiar Estado Publicado",
            SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
            TargetObjectType = typeof(Asiento)
        };
        togglePublicadoAction.Execute += TogglePublicadoAction_Execute;

        toggleEstadoEjercicioAction = new SimpleAction(this, "ToggleEstadoEjercicioAction", "Edit")
        {
            Caption = "Cambiar Estado Ejercicio",
            SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
            TargetObjectType = typeof(Ejercicio)
        };
        toggleEstadoEjercicioAction.Execute += ToggleEstadoEjercicioAction_Execute;
    }

    private void TogglePublicadoAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        foreach (Asiento asiento in e.SelectedObjects)
        {
            asiento.TogglePublicado();
        }
        ObjectSpace.CommitChanges();
    }

    private void ToggleEstadoEjercicioAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        foreach (Ejercicio ejercicio in e.SelectedObjects)
        {
            ejercicio.ToggleEstadoEjercicio();
        }
        ObjectSpace.CommitChanges();
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        View.CurrentObjectChanged += View_CurrentObjectChanged;
        if (View is ListView listView)
        {
            listView.SelectionChanged += ListView_SelectionChanged;
        }
        if (View is ObjectView objectView)
        {
            objectView.ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
            objectView.ObjectSpace.Committed += ObjectSpace_Committed;
        }
        ActualizarAcciones();
    }

    protected override void OnDeactivated()
    {
        View.CurrentObjectChanged -= View_CurrentObjectChanged;
        if (View is ListView listView)
        {
            listView.SelectionChanged -= ListView_SelectionChanged;
        }
        if (View is ObjectView objectView)
        {
            objectView.ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
            objectView.ObjectSpace.Committed -= ObjectSpace_Committed;
        }
        base.OnDeactivated();
    }

    private void ListView_SelectionChanged(object? sender, EventArgs e)
    {
        ActualizarAcciones();
    }

    private void ObjectSpace_Committed(object? sender, EventArgs e)
    {
        ActualizarAcciones();
    }

    private void View_CurrentObjectChanged(object? sender, EventArgs e)
    {
        ActualizarAcciones();
    }

    private void ObjectSpace_ObjectChanged(object? sender, ObjectChangedEventArgs e)
    {
        if (e.Object == View.CurrentObject && (e.PropertyName == "Estado" || string.IsNullOrEmpty(e.PropertyName)))
        {
            ActualizarAcciones();
        }
    }

    private void ActualizarAcciones()
    {
        // Gestión de la acción de Asiento
        if (togglePublicadoAction != null)
        {
            // Determinar el objeto de referencia para texto e icono
            Asiento? asientoReferencia = View.CurrentObject as Asiento;
            if (View is ListView { SelectedObjects.Count: > 0 } lvAsiento)
            {
                asientoReferencia = lvAsiento.SelectedObjects[0] as Asiento;
            }

            // Gestionar la visibilidad primero (SeleccionHeterogenea)
            if (View is ListView listView && listView.ObjectTypeInfo.Type == typeof(Asiento))
            {
                var selectedAsientos = listView.SelectedObjects.Cast<Asiento>().ToList();
                if (selectedAsientos.Count > 0)
                {
                    bool todosIguales = selectedAsientos.All(a => a.Estado == selectedAsientos[0].Estado);
                    togglePublicadoAction.Active["SeleccionHeterogenea"] = todosIguales;
                }
                else
                {
                    togglePublicadoAction.Active.RemoveItem("SeleccionHeterogenea");
                }
            }
            else
            {
                togglePublicadoAction.Active.RemoveItem("SeleccionHeterogenea");
            }

            // Después, actualizar el texto e icono según el objeto de referencia
            if (asientoReferencia != null)
            {
                string caption = asientoReferencia.Estado == EstadoAsiento.Borrador ? "Publicar Asiento" : "Pasar a Borrador";
                string image = asientoReferencia.Estado == EstadoAsiento.Borrador ? "Action_Security_Allow" : "Action_Undo";
                
                if (togglePublicadoAction.Caption != caption)
                {
                    togglePublicadoAction.Caption = caption;
                    togglePublicadoAction.ToolTip = caption;
                }
                if (togglePublicadoAction.ImageName != image)
                {
                    togglePublicadoAction.ImageName = image;
                }
            }
            
            // Forzar actualización de metadatos en Blazor
            togglePublicadoAction.BeginUpdate();
            togglePublicadoAction.EndUpdate();
        }

        // Gestión de la acción de Ejercicio
        if (toggleEstadoEjercicioAction != null)
        {
            Ejercicio? ejercicioReferencia = View.CurrentObject as Ejercicio;
            if (View is ListView { SelectedObjects.Count: > 0 } lvEjercicio)
            {
                ejercicioReferencia = lvEjercicio.SelectedObjects[0] as Ejercicio;
            }

            // Visibilidad primero
            if (View is ListView listView && listView.ObjectTypeInfo.Type == typeof(Ejercicio))
            {
                var selectedEjercicios = listView.SelectedObjects.Cast<Ejercicio>().ToList();
                if (selectedEjercicios.Count > 0)
                {
                    bool todosIguales = selectedEjercicios.All(e => e.Estado == selectedEjercicios[0].Estado);
                    toggleEstadoEjercicioAction.Active["SeleccionHeterogenea"] = todosIguales;
                }
                else
                {
                    toggleEstadoEjercicioAction.Active.RemoveItem("SeleccionHeterogenea");
                }
            }
            else
            {
                toggleEstadoEjercicioAction.Active.RemoveItem("SeleccionHeterogenea");
            }

            // Actualizar texto e icono
            if (ejercicioReferencia != null)
            {
                string caption = "";
                string image = "";
                switch (ejercicioReferencia.Estado)
                {
                    case EstadoEjercicio.Abierto:
                        caption = "Cerrar Ejercicio";
                        image = "Action_Close";
                        break;
                    case EstadoEjercicio.Cerrado:
                        caption = "Bloquear Ejercicio";
                        image = "Action_Lock";
                        break;
                    case EstadoEjercicio.Bloqueado:
                        caption = "Abrir Ejercicio";
                        image = "Action_Refresh";
                        break;
                }

                if (toggleEstadoEjercicioAction.Caption != caption)
                {
                    toggleEstadoEjercicioAction.Caption = caption;
                    toggleEstadoEjercicioAction.ToolTip = caption;
                }
                if (toggleEstadoEjercicioAction.ImageName != image)
                {
                    toggleEstadoEjercicioAction.ImageName = image;
                }
            }

            // Forzar actualización de metadatos
            toggleEstadoEjercicioAction.BeginUpdate();
            toggleEstadoEjercicioAction.EndUpdate();
        }
    }
}
