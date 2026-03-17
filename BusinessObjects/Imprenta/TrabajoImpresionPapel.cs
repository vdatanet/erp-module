using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Productos;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Helpers.Imprenta;
using System;
using System.Linq;

namespace erp.Module.BusinessObjects.Imprenta
{
    [DefaultClassOptions]
    [NavigationItem("Imprenta")]
    public class TrabajoImpresionPapel(Session session) : LineaDocumentoVenta(session)
    {
        private TrabajoImpresion? _trabajoImpresion;
        [Association("TrabajoImpresion-Papeles")]
        public TrabajoImpresion? TrabajoImpresion
        {
            get => _trabajoImpresion;
            set
            {
                if (SetPropertyValue(nameof(TrabajoImpresion), ref _trabajoImpresion, value) && !IsLoading && !IsSaving)
                {
                    if (value != null && DocumentoVenta != value)
                    {
                        DocumentoVenta = value;
                    }
                }
            }
        }

        protected override void OnProductoChanged()
        {
            base.OnProductoChanged();
            if (Producto != null)
            {
                TotalizarLinea();
            }
        }

        private bool SetAndRecalculate<T>(string propertyName, ref T field, T value)
        {
            bool modified = SetPropertyValue(propertyName, ref field, value);
            if (modified && !IsLoading && !IsSaving)
            {
                TotalizarLinea();
            }
            return modified;
        }

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
            set => SetAndRecalculate(nameof(Ancho), ref _ancho, value);
        }

        private decimal _alto;
        [ImmediatePostData]
        public decimal Alto
        {
            get => _alto;
            set => SetAndRecalculate(nameof(Alto), ref _alto, value);
        }

        private decimal _gramaje;
        [ImmediatePostData]
        public decimal Gramaje
        {
            get => _gramaje;
            set => SetAndRecalculate(nameof(Gramaje), ref _gramaje, value);
        }

        private void TotalizarLinea()
        {
            if (Producto != null && DocumentoVenta is TrabajoImpresion trabajoImpresion)
            {
                decimal ctd = trabajoImpresion.Cantidad;
                var tramo = ImprentaHelper.BuscarTramoDePrecio(Producto, ctd);
                if (tramo != null)
                {
                    PrecioUnitario = ImprentaHelper.CalcularPrecioUnitario(ctd, tramo);
                }
            }
        }
    }
}
