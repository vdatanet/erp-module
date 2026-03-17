using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Productos;
using erp.Module.BusinessObjects.Base.Ventas;
using System;
using System.Linq;

namespace erp.Module.BusinessObjects.Imprenta
{
    [DefaultClassOptions]
    [NavigationItem("Imprenta")]
    public class TrabajoImpresionHora(Session session) : LineaDocumentoVenta(session)
    {
        private void TotalizarLinea()
        {
            decimal ctd = Cantidad;
            if (Producto != null && ctd != 0)
            {
                var precio = Producto.PreciosPorCantidad
                    .FirstOrDefault(p => ctd >= p.InicioIntervalo && ctd < p.FinIntervalo);

                if (precio != null)
                {
                    if (ctd * precio.PrecioUnitario > precio.ImporteMinimo)
                    {
                        PrecioUnitario = precio.PrecioUnitario;
                    }
                    else
                    {
                        PrecioUnitario = precio.ImporteMinimo / ctd;
                    }
                }
            }
        }
    }
}
