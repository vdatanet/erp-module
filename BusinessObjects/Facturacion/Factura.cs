using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Alquileres;

namespace erp.Module.BusinessObjects.Facturacion;

[DefaultClassOptions]
[NavigationItem("Facturacion")]
[ImageName("BO_Factura")]
[DefaultProperty(nameof(Secuencia))]
public class Factura(Session session) : FacturaBase(session)
{
    public override bool EsValida()
    {
        return EstadoVeriFactu != ValoresEstadoVeriFactu.Enviado
               && Cliente != null
               && !string.IsNullOrEmpty(Cliente.Nombre)
               && !string.IsNullOrEmpty(Cliente.Nif)
               && !string.IsNullOrEmpty(Texto)
               && Impuestos.Count > 0;
    }

    [XafDisplayName("Pagos")]
    [Association("Factura-Pagos")]
    public XPCollection<Pago> Pagos => GetCollection<Pago>(nameof(Pagos));
}