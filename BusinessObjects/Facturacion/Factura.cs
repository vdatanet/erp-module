using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Alquileres;

namespace erp.Module.BusinessObjects.Facturacion;

[DefaultClassOptions]
[NavigationItem("Facturacion")]
[ImageName("BO_Factura")]
[DefaultProperty(nameof(Numero))]
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

    private Pago? _pago;

    [Association("Factura-Pago")]
    [XafDisplayName("Pago")]
    public Pago? Pago
    {
        get => _pago;
        set => SetPropertyValue(nameof(Pago), ref _pago, value);
    }
}