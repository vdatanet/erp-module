using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Productos
{
    [DefaultClassOptions]
    [NavigationItem("Productos")]
    public class PrecioPorCantidad : EntidadBase
    {
        public PrecioPorCantidad(Session session) : base(session) { }

        private Producto? _producto;
        [Association("Producto-PreciosPorCantidad")]
        public Producto? Producto
        {
            get => _producto;
            set => SetPropertyValue(nameof(Producto), ref _producto, value);
        }

        private decimal _inicioIntervalo;
        public decimal InicioIntervalo
        {
            get => _inicioIntervalo;
            set => SetPropertyValue(nameof(InicioIntervalo), ref _inicioIntervalo, value);
        }

        private decimal _finIntervalo;
        public decimal FinIntervalo
        {
            get => _finIntervalo;
            set => SetPropertyValue(nameof(FinIntervalo), ref _finIntervalo, value);
        }

        private decimal _precioUnitario;
        public decimal PrecioUnitario
        {
            get => _precioUnitario;
            set => SetPropertyValue(nameof(PrecioUnitario), ref _precioUnitario, value);
        }

        private decimal _importeMinimo;
        public decimal ImporteMinimo
        {
            get => _importeMinimo;
            set => SetPropertyValue(nameof(ImporteMinimo), ref _importeMinimo, value);
        }

        private decimal _precioEntrada;
        public decimal PrecioEntrada
        {
            get => _precioEntrada;
            set => SetPropertyValue(nameof(PrecioEntrada), ref _precioEntrada, value);
        }

        private string? _observaciones;
        [ModelDefault("PropertyEditorType", "DevExpress.ExpressApp.Win.Editors.MemoEditStringPropertyEditor")]
        [Size(4000)]
        public string? Observaciones
        {
            get => _observaciones;
            set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
        }
    }
}
