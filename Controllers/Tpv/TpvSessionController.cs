using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Tpv;
using erp.Module.BusinessObjects;
using DevExpress.ExpressApp.Security;

namespace erp.Module.Controllers.Tpv;

public class TpvSessionController : ViewController
{
    public TpvSessionController()
    {
        TargetObjectType = typeof(BusinessObjects.Tpv.Tpv);
        
        var abrirSesion = new SimpleAction(this, "Tpv_AbrirSesion", PredefinedCategory.RecordEdit)
        {
            Caption = "Abrir Sesión",
            ImageName = "Action_Open",
            TargetObjectsCriteria = "SesionAbierta = false",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        abrirSesion.Execute += AbrirSesion_Execute;
        Actions.Add(abrirSesion);

        var cerrarSesion = new PopupWindowShowAction(this, "Tpv_CerrarSesion", PredefinedCategory.RecordEdit)
        {
            Caption = "Cerrar Sesión",
            ImageName = "Action_Close",
            TargetObjectsCriteria = "SesionAbierta = true",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        cerrarSesion.CustomizePopupWindowParams += CerrarSesion_CustomizePopupWindowParams;
        cerrarSesion.Execute += CerrarSesion_Execute;
        Actions.Add(cerrarSesion);

        var continuarSesion = new SimpleAction(this, "Tpv_ContinuarSesion", PredefinedCategory.RecordEdit)
        {
            Caption = "Continuar Sesión",
            ImageName = "Action_LinkUnlink_Link",
            TargetObjectsCriteria = "SesionAbierta = true",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        continuarSesion.Execute += ContinuarSesion_Execute;
        Actions.Add(continuarSesion);
        
        var retirarEfectivo = new PopupWindowShowAction(this, "Tpv_RetirarEfectivo", PredefinedCategory.RecordEdit)
        {
            Caption = "Retirar Efectivo",
            ImageName = "Action_MoneyWithdraw",
            TargetObjectsCriteria = "SesionAbierta = true",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        retirarEfectivo.CustomizePopupWindowParams += RetirarEfectivo_CustomizePopupWindowParams;
        retirarEfectivo.Execute += RetirarEfectivo_Execute;
        Actions.Add(retirarEfectivo);
        var reabrirSesion = new SimpleAction(this, "Tpv_ReabrirSesion", PredefinedCategory.RecordEdit)
        {
            Caption = "Reabrir Sesión",
            ImageName = "Action_ResetViewSettings",
            TargetObjectsCriteria = "SesionAbierta = false AND Sesiones.Count > 0",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject
        };
        reabrirSesion.Execute += ReabrirSesion_Execute;
        Actions.Add(reabrirSesion);
    }

    private void AbrirSesion_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var tpv = (BusinessObjects.Tpv.Tpv)e.CurrentObject;
        tpv.AbrirSesionAction();
        ObjectSpace.CommitChanges();
    }

    private void ReabrirSesion_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var tpv = (BusinessObjects.Tpv.Tpv)e.CurrentObject;
        tpv.ReabrirSesionAction();
        ObjectSpace.CommitChanges();
    }

    private void ContinuarSesion_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var tpv = (BusinessObjects.Tpv.Tpv)e.CurrentObject;
        if (tpv.SesionActualAbierta == null) return;
        
        var detailView = Application.CreateDetailView(ObjectSpace, tpv.SesionActualAbierta);
        e.ShowViewParameters.CreatedView = detailView;
        e.ShowViewParameters.TargetWindow = TargetWindow.Current;
    }

    private void CerrarSesion_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        var tpv = (BusinessObjects.Tpv.Tpv)View.CurrentObject;
        var sesion = tpv.SesionActualAbierta;
        if (sesion == null) return;

        sesion.CalcularImporteEsperado();
        
        var parameters = new CierreSesionParameters
        {
            ImporteEsperado = sesion.ImporteEsperado,
            ImporteContado = sesion.ImporteEsperado
        };
        
        var objectSpace = Application.CreateObjectSpace(typeof(CierreSesionParameters));
        e.View = Application.CreateDetailView(objectSpace, parameters);
    }

    private void CerrarSesion_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
    {
        var tpv = (BusinessObjects.Tpv.Tpv)e.CurrentObject;
        var sesion = tpv.SesionActualAbierta;
        if (sesion == null) return;

        var parameters = (CierreSesionParameters)e.PopupWindowViewCurrentObject;
        sesion.Observaciones = parameters.Observaciones;
        sesion.CerrarSesion(parameters.ImporteContado);
        
        ObjectSpace.CommitChanges();
    }

    private void RetirarEfectivo_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        var parameters = new MovimientoCajaParameters { Tipo = TipoMovimientoCajaTpv.Retirada };
        var objectSpace = Application.CreateObjectSpace(typeof(MovimientoCajaParameters));
        e.View = Application.CreateDetailView(objectSpace, parameters);
    }

    private void RetirarEfectivo_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
    {
        var tpv = (BusinessObjects.Tpv.Tpv)e.CurrentObject;
        var sesion = tpv.SesionActualAbierta;
        if (sesion == null) return;

        var parameters = (MovimientoCajaParameters)e.PopupWindowViewCurrentObject;
        sesion.RegistrarMovimiento(parameters.Tipo, parameters.Importe, parameters.Motivo);
        
        ObjectSpace.CommitChanges();
    }
}
