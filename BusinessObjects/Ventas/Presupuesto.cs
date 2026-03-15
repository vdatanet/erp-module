using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;

using erp.Module.BusinessObjects.Contactos;

using erp.Module.BusinessObjects.Crm;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[NavigationItem("Ventas")]
[ImageName("BO_Order")]
public class Presupuesto(Session session): DocumentoVenta(session)
{
    [Association("Oportunidad-Presupuestos")]
    public override Oportunidad Oportunidad
    {
        get => base.Oportunidad;
        set => base.Oportunidad = value;
    }
}