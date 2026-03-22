using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.BusinessObjects.Alquileres;
using System.Linq;

namespace erp.Module.Controllers.Comisiones;

public class LiquidacionComisionesController : ObjectViewController<DetailView, LiquidacionComision>
{
    public LiquidacionComisionesController()
    {
        var generarComisionesAction = new SimpleAction(this, "GenerarComisiones", PredefinedCategory.RecordEdit)
        {
            Caption = "Generar Comisiones",
            ConfirmationMessage = "¿Desea generar las comisiones para este mes/año y vendedor? Se borrarán las actuales de esta liquidación.",
            ImageName = "Action_Generate"
        };
        generarComisionesAction.Execute += GenerarComisionesAction_Execute;
    }

    private void GenerarComisionesAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var liquidacion = ViewCurrentObject;
        if (liquidacion == null) return;

        liquidacion.GenerarComisiones();

        if (ObjectSpace.IsModified)
        {
            ObjectSpace.CommitChanges();
        }
        
        View.Refresh();
    }
}
