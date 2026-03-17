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
    public class TrabajoImpresionServicio(Session session) : LineaDocumentoVenta(session)
    {
        private decimal _precio;
        [ImmediatePostData]
        public decimal Precio
        {
            get => _precio;
            set
            {
                bool modified = SetPropertyValue(nameof(Precio), ref _precio, value);
                if (!IsLoading && !IsSaving && modified)
                {
                    TotalizarLineaSinCambiarPrecio();
                }
            }
        }

        private decimal _numEntradasMaq;
        [ImmediatePostData]
        public decimal NumEntradasMaq
        {
            get => _numEntradasMaq;
            set
            {
                bool modified = SetPropertyValue(nameof(NumEntradasMaq), ref _numEntradasMaq, value);
                if (!IsLoading && !IsSaving && modified)
                {
                    TotalizarLinea();
                }
            }
        }

        private decimal _precioEntrada;
        [ImmediatePostData]
        public decimal PrecioEntrada
        {
            get => _precioEntrada;
            set
            {
                bool modified = SetPropertyValue(nameof(PrecioEntrada), ref _precioEntrada, value);
                if (!IsLoading && !IsSaving && modified)
                {
                    TotalizarLineaSinCambiarPrecio();
                }
            }
        }

        private void TotalizarLinea()
        {
            decimal ctd = Cantidad;
            if (Producto != null && ctd != 0)
            {
                var precio = Producto.PreciosPorCantidad
                    .FirstOrDefault(p => ctd >= p.InicioIntervalo && ctd <= p.FinIntervalo);

                if (precio != null)
                {
                    if (ctd * precio.PrecioUnitario > precio.ImporteMinimo)
                    {
                        Precio = precio.PrecioUnitario;
                        PrecioEntrada = precio.PrecioEntrada;
                    }
                    else
                    {
                        Precio = precio.ImporteMinimo / ctd;
                        PrecioEntrada = precio.PrecioEntrada;
                    }
                }
            }
            TotalizarLineaSinCambiarPrecio();
        }

        private void TotalizarLineaSinCambiarPrecio()
        {
            if (Cantidad != 0)
            {
                PrecioUnitario = ((NumEntradasMaq * PrecioEntrada) + (Cantidad * Precio * NumEntradasMaq)) / Cantidad;
            }
            else
            {
                PrecioUnitario = 0;
            }
        }
    }
}
