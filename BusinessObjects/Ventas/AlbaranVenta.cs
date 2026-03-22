using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;

using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[NavigationItem("Ventas")]
[ImageName("BO_Order")]
public class AlbaranVenta(Session session) : DocumentoVenta(session)
{
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        Serie ??= companyInfo?.PrefijoAlbaranesVentaPorDefecto;
    }
}