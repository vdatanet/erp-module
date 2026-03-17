using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Productos;
using erp.Module.BusinessObjects.Base.Ventas;
using System;
using System.Linq;
using erp.Module.Helpers.Imprenta;

namespace erp.Module.BusinessObjects.Imprenta
{
    [DefaultClassOptions]
    [NavigationItem("Imprenta")]
    public class TrabajoImpresionHora(Session session) : LineaDocumentoVenta(session)
    {
        protected override void OnProductoChanged()
        {
            base.OnProductoChanged();
            if (Producto != null)
            {
                TotalizarLinea();
            }
        }

        protected override void OnCantidadChanged()
        {
            base.OnCantidadChanged();
            TotalizarLinea();
        }

        private void TotalizarLinea()
        {
            if (Producto != null)
            {
                var tramo = ImprentaHelper.BuscarTramoDePrecio(Producto, Cantidad);
                if (tramo != null)
                {
                    PrecioUnitario = ImprentaHelper.CalcularPrecioUnitario(Cantidad, tramo);
                }
            }
        }
    }
}
