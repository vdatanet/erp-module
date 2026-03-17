using System.Linq;
using erp.Module.BusinessObjects.Productos;

namespace erp.Module.Helpers.Imprenta
{
    public static class ImprentaHelper
    {
        public static PrecioPorCantidad? BuscarTramoDePrecio(Producto producto, decimal cantidad)
        {
            if (producto == null || cantidad == 0) return null;
            
            return producto.PreciosPorCantidad
                .FirstOrDefault(p => cantidad >= p.InicioIntervalo && cantidad < p.FinIntervalo);
        }

        public static decimal CalcularPrecioUnitario(decimal cantidad, PrecioPorCantidad tramo)
        {
            if (tramo == null || cantidad == 0) return 0;

            if (cantidad * tramo.PrecioUnitario > tramo.ImporteMinimo)
            {
                return tramo.PrecioUnitario;
            }
            
            return tramo.ImporteMinimo / cantidad;
        }
    }
}
