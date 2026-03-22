using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Compras;

using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Compras;

[DefaultClassOptions]
[NavigationItem("Compras")]
[ImageName("BO_Order")]
public class PedidoCompra(Session session) : DocumentoCompra(session)
{
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        Serie ??= companyInfo?.PrefijoPedidosCompraPorDefecto;
    }
}