using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Alquileres;

namespace erp.Module.BusinessObjects.Facturacion;

[DefaultClassOptions]
[NavigationItem("Facturacion")]
[ImageName("BO_Invoice")]
[DefaultProperty(nameof(Secuencia))]
public class Factura(Session session) : FacturaBase(session)
{
    [XafDisplayName("Pagos")]
    [Association("Factura-Pagos")]
    public XPCollection<Pago> Pagos => GetCollection<Pago>();

    public override bool EsValida()
    {
        return EstadoVeriFactu != ValoresEstadoVeriFactu.Enviado
               && Cliente != null
               && !string.IsNullOrEmpty(Cliente.Nombre)
               && !string.IsNullOrEmpty(Cliente.Nif)
               && !string.IsNullOrEmpty(Texto)
               && Impuestos.Count > 0;
    }
}