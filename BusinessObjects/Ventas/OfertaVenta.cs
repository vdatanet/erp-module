using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Crm;

using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[XafDisplayName("Oferta de Venta")]
[NavigationItem("Ventas")]
[ImageName("BO_Order")]
public class OfertaVenta(Session session) : DocumentoVenta(session)
{
    private Oportunidad? _oportunidad;

    [Association("Oportunidad-OfertasVenta")]
    [XafDisplayName("Oportunidad")]
    public Oportunidad? Oportunidad
    {
        get => _oportunidad;
        set
        {
            if (!SetPropertyValue(nameof(Oportunidad), ref _oportunidad, value) || IsLoading || IsSaving) return;
            if (value != null)
            {
                if (value.Cliente != null) Cliente = value.Cliente;
                if (value.EquipoVenta != null) EquipoVenta = value.EquipoVenta;
                if (value.Vendedor != null) Vendedor = value.Vendedor;
            }
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        Serie ??= companyInfo.PrefijoOfertasVentaPorDefecto;
    }
}