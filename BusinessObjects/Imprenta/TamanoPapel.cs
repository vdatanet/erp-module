using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Imprenta
{
    [DefaultClassOptions]
    [NavigationItem("Imprenta")]
    public class TamanoPapel : EntidadBase
    {
        public TamanoPapel(Session session) : base(session) { }

        private decimal _ancho;
        [ImmediatePostData]
        public decimal Ancho
        {
            get => _ancho;
            set => SetPropertyValue(nameof(Ancho), ref _ancho, value);
        }

        private decimal _alto;
        [ImmediatePostData]
        public decimal Alto
        {
            get => _alto;
            set => SetPropertyValue(nameof(Alto), ref _alto, value);
        }

        private string? _descripcion;
        [Size(255)]
        public string? Descripcion
        {
            get => _descripcion;
            set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
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
