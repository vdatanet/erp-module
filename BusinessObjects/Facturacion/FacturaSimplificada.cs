using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Facturacion;

namespace erp.Module.BusinessObjects.Facturacion;

[DefaultClassOptions]
[NavigationItem("Facturacion")]
[ImageName("BO_Factura")] // Podría cambiarse a algo más específico si existe
[DefaultProperty(nameof(Secuencia))]
public class FacturaSimplificada(Session session) : FacturaBase(session)
{
    public override bool EsValida()
    {
        return EstadoVeriFactu != ValoresEstadoVeriFactu.Enviado
               && !string.IsNullOrEmpty(Texto)
               && Impuestos.Count > 0;
    }
}