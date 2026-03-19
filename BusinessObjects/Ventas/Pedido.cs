using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Crm;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[NavigationItem("Ventas")]
[ImageName("BO_Order")]
public class Pedido(Session session) : DocumentoVenta(session)
{
    private Oportunidad? _oportunidad;

    [Association("Oportunidad-Pedidos")]
    [XafDisplayName("Oportunidad")]
    public Oportunidad? Oportunidad
    {
        get => _oportunidad;
        set
        {
            if (!SetPropertyValue(nameof(Oportunidad), ref _oportunidad, value) || IsLoading || IsSaving) return;
            if (value != null && value.Cliente != null) Cliente = value.Cliente;
        }
    }
}