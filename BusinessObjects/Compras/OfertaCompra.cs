using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Compras;

using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Compras;

[DefaultClassOptions]
[XafDisplayName("Oferta de Compra")]
[NavigationItem("Compras")]
[ImageName("BO_Order")]
public class OfertaCompra(Session session) : DocumentoCompra(session)
{
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        Serie ??= companyInfo.PrefijoOfertasCompraPorDefecto;
    }
}