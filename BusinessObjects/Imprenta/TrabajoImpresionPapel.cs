using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Productos;
using erp.Module.BusinessObjects.Base.Ventas;
using System;
using System.Linq;

namespace erp.Module.BusinessObjects.Imprenta
{
    [DefaultClassOptions]
    public class TrabajoImpresionPapel(Session session) : LineaDocumentoVenta(session)
    {
        private TamanoPapel? _tamanoPapel;
        [ImmediatePostData]
        public TamanoPapel? TamanoPapel
        {
            get => _tamanoPapel;
            set
            {
                bool modified = SetPropertyValue(nameof(TamanoPapel), ref _tamanoPapel, value);
                if (!IsLoading && !IsSaving && value != null && modified)
                {
                    Ancho = value.Ancho;
                    Alto = value.Alto;
                }
            }
        }

        private decimal _ancho;
        [ImmediatePostData]
        public decimal Ancho
        {
            get => _ancho;
            set
            {
                bool modified = SetPropertyValue(nameof(Ancho), ref _ancho, value);
                if (!IsLoading && !IsSaving && modified)
                {
                    TotalizarLinea();
                }
            }
        }

        private decimal _alto;
        [ImmediatePostData]
        public decimal Alto
        {
            get => _alto;
            set
            {
                bool modified = SetPropertyValue(nameof(Alto), ref _alto, value);
                if (!IsLoading && !IsSaving && modified)
                {
                    TotalizarLinea();
                }
            }
        }

        private decimal _gramaje;
        [ImmediatePostData]
        public decimal Gramaje
        {
            get => _gramaje;
            set
            {
                bool modified = SetPropertyValue(nameof(Gramaje), ref _gramaje, value);
                if (!IsLoading && !IsSaving && modified)
                {
                    TotalizarLinea();
                }
            }
        }

        private void TotalizarLinea()
        {
            if (Producto != null && DocumentoVenta is TrabajoImpresion trabajoImpresion)
            {
                decimal ctd = trabajoImpresion.Quantitat;
                if (ctd != 0)
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
}
