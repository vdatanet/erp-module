using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Services.Ventas.StateMachines;

using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[XafDisplayName("Albarán de Venta")]
[NavigationItem("Ventas")]
[ImageName("BO_Order")]
public class AlbaranVenta(Session session) : DocumentoVenta(session)
{
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        Serie ??= companyInfo.PrefijoAlbaranesVentaPorDefecto;
    }
    protected override IDocumentoVentaStateMachine CreateStateMachine() => new AlbaranVentaStateMachine(this);
}