using erp.Module.Helpers.Contactos;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[NavigationItem("Ventas")]
[ImageName("BO_Order")]
public class Presupuesto(Session session): DocumentoVenta(session)
{
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        Serie ??= companyInfo?.PrefijoFacturasVentaPorDefecto;
    }
}