using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Crm;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[NavigationItem("Ventas")]
[ImageName("BO_Order")]
public class Presupuesto(Session session) : DocumentoVenta(session)
{
    private Oportunidad? _oportunidad;

    [Association("Oportunidad-Presupuestos")]
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
}