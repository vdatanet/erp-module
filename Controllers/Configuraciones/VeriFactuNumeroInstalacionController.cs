using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Configuraciones;

namespace erp.Module.Controllers.Configuraciones;

public class VeriFactuNumeroInstalacionController : ObjectViewController<DetailView, InformacionEmpresa>
{
    public VeriFactuNumeroInstalacionController()
    {
        var reiniciarAccion = new PopupWindowShowAction(this, "VeriFactuReiniciarNumeroInstalacion", PredefinedCategory.Edit)
        {
            Caption = "Reiniciar Nº Instalación VeriFactu",
            ToolTip = "Reinicia el número de instalación para la cadena de facturación VeriFactu",
            ImageName = "Action_Reset"
        };
        
        reiniciarAccion.CustomizePopupWindowParams += ReiniciarAccion_CustomizePopupWindowParams;
        reiniciarAccion.Execute += ReiniciarAccion_Execute;
    }

    private void ReiniciarAccion_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        var os = Application.CreateObjectSpace(typeof(VeriFactuMotivoReinicioParam));
        var param = os.CreateObject<VeriFactuMotivoReinicioParam>();
        var dv = Application.CreateDetailView(os, param);
        e.View = dv;
    }

    private void ReiniciarAccion_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
    {
        var param = (VeriFactuMotivoReinicioParam)e.PopupWindowViewCurrentObject;
        if (string.IsNullOrWhiteSpace(param.Motivo))
            throw new UserFriendlyException("El motivo es obligatorio.");

        var empresa = ViewCurrentObject;
        empresa.ReiniciarNumeroInstalacion(param.Motivo);
        
        if (View.ObjectSpace.IsModified)
            View.ObjectSpace.CommitChanges();
            
        Application.ShowViewStrategy.ShowMessage("Número de instalación reiniciado correctamente.", InformationType.Success);
    }
}

[DomainComponent]
public class VeriFactuMotivoReinicioParam
{
    [Size(SizeAttribute.Unlimited)]
    public string? Motivo { get; set; }
}
