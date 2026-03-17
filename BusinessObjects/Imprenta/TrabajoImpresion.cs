using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Ventas;
using System;

namespace erp.Module.BusinessObjects.Imprenta
{
    [DefaultClassOptions]
    [NavigationItem("Imprenta")]
    [XafDisplayName("Trabajo de Impresión")]
    [ImageName("BO_Order")]
    public class TrabajoImpresion : Presupuesto
    {
        public TrabajoImpresion(Session session) : base(session) { }

        private string? _descripcion;
        [ModelDefault("PropertyEditorType", "DevExpress.ExpressApp.Win.Editors.MemoEditStringPropertyEditor")]
        [Size(SizeAttribute.Unlimited)]
        [XafDisplayName("Trabajo")]
        public string? Descripcion
        {
            get => _descripcion;
            set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
        }

        private EstadosPresupuesto? _estado;
        public EstadosPresupuesto? Estado
        {
            get => _estado;
            set => SetPropertyValue(nameof(Estado), ref _estado, value);
        }

        private DateTime _fechaEntrega;
        [XafDisplayName("Entrega / Fecha compromiso")]
        public DateTime FechaEntrega
        {
            get => _fechaEntrega;
            set => SetPropertyValue(nameof(FechaEntrega), ref _fechaEntrega, value);
        }
        
        private decimal _acuenta;
        [XafDisplayName("A cuenta")]
        public decimal Acuenta
        {
            get => _acuenta;
            set => SetPropertyValue(nameof(Acuenta), ref _acuenta, value);
        }

        private decimal _cantidad;
        public decimal Cantidad
        {
            get => _cantidad;
            set => SetPropertyValue(nameof(Cantidad), ref _cantidad, value);
        }

        private string? _papel;
        public string? Papel
        {
            get => _papel;
            set => SetPropertyValue(nameof(Papel), ref _papel, value);
        }

        private string? _tintas;
        public string? Tintas
        {
            get => _tintas;
            set => SetPropertyValue(nameof(Tintas), ref _tintas, value);
        }

        private bool _bDigitalC;
        [XafDisplayName("Digital Color")]
        public bool bDigitalC
        {
            get => _bDigitalC;
            set => SetPropertyValue(nameof(bDigitalC), ref _bDigitalC, value);
        }

        private bool _bDigitalC1;
        [XafDisplayName("Digital Color 1c.")]
        public bool bDigitalC1
        {
            get => _bDigitalC1;
            set => SetPropertyValue(nameof(bDigitalC1), ref _bDigitalC1, value);
        }

        private bool _bDigitalC2;
        [XafDisplayName("Digital Color 2c.")]
        public bool bDigitalC2
        {
            get => _bDigitalC2;
            set => SetPropertyValue(nameof(bDigitalC2), ref _bDigitalC2, value);
        }

        private bool _bDigitalBN;
        [XafDisplayName("Digital B/N")]
        public bool bDigitalBN
        {
            get => _bDigitalBN;
            set => SetPropertyValue(nameof(bDigitalBN), ref _bDigitalBN, value);
        }

        private bool _bDigitalBN1;
        [XafDisplayName("Digital B/N 1c.")]
        public bool bDigitalBN1
        {
            get => _bDigitalBN1;
            set => SetPropertyValue(nameof(bDigitalBN1), ref _bDigitalBN1, value);
        }

        private bool _bDigitalBN2;
        [XafDisplayName("Digital B/N 2c.")]
        public bool bDigitalBN2
        {
            get => _bDigitalBN2;
            set => SetPropertyValue(nameof(bDigitalBN2), ref _bDigitalBN2, value);
        }

        private bool _bDigital1;
        [XafDisplayName("Digital 1c.")]
        public bool bDigital1
        {
            get => _bDigital1;
            set => SetPropertyValue(nameof(bDigital1), ref _bDigital1, value);
        }

        private bool _bDigital2;
        [XafDisplayName("Digital 2c.")]
        public bool bDigital2
        {
            get => _bDigital2;
            set => SetPropertyValue(nameof(bDigital2), ref _bDigital2, value);
        }

        private bool _bGrapado;
        [XafDisplayName("Grapado")]
        public bool bGrapado
        {
            get => _bGrapado;
            set => SetPropertyValue(nameof(bGrapado), ref _bGrapado, value);
        }
        
        private bool _bTaladrado;
        [XafDisplayName("Taladrado")]
        public bool bTaladrado
        {
            get => _bTaladrado;
            set => SetPropertyValue(nameof(bTaladrado), ref _bTaladrado, value);
        }

        private bool _bEncolado;
        [XafDisplayName("Encolado")]
        public bool bEncolado
        {
            get => _bEncolado;
            set => SetPropertyValue(nameof(bEncolado), ref _bEncolado, value);
        }

        private bool _bTroquelado;
        [XafDisplayName("Troquelado")]
        public bool bTroquelado
        {
            get => _bTroquelado;
            set => SetPropertyValue(nameof(bTroquelado), ref _bTroquelado, value);
        }

        private bool _bDoblado;
        [XafDisplayName("Doblado")]
        public bool bDoblado
        {
            get => _bDoblado;
            set => SetPropertyValue(nameof(bDoblado), ref _bDoblado, value);
        }
        
        private bool _bEncuadernado;
        [XafDisplayName("Encuadernado")]
        public bool bEncuadernado
        {
            get => _bEncuadernado;
            set => SetPropertyValue(nameof(bEncuadernado), ref _bEncuadernado, value);
        }

        private bool _bPerforado;
        [XafDisplayName("Perforado")]
        public bool bPerforado
        {
            get => _bPerforado;
            set => SetPropertyValue(nameof(bPerforado), ref _bPerforado, value);
        }

        private int _mmPerforado;
        [XafDisplayName("Perforado (mm)")]
        public int mmPerforado
        {
            get => _mmPerforado;
            set => SetPropertyValue(nameof(mmPerforado), ref _mmPerforado, value);
        }

        private bool _bTroqueladora;
        [XafDisplayName("Troqueladora")]
        public bool bTroqueladora
        {
            get => _bTroqueladora;
            set => SetPropertyValue(nameof(bTroqueladora), ref _bTroqueladora, value);
        }

        private bool _bNumerado;
        [XafDisplayName("Numerado")]
        public bool bNumerado
        {
            get => _bNumerado;
            set => SetPropertyValue(nameof(bNumerado), ref _bNumerado, value);
        }

        private string? _numeratString;
        [XafDisplayName("Tipo Numerado")]
        public string? NumeratString
        {
            get => _numeratString;
            set => SetPropertyValue(nameof(NumeratString), ref _numeratString, value);
        }

        private bool _bEncoladoQ;
        [XafDisplayName("Encolado Q")]
        public bool bEncoladoQ
        {
            get => _bEncoladoQ;
            set => SetPropertyValue(nameof(bEncoladoQ), ref _bEncoladoQ, value);
        }

        private bool _bPlastificado;
        [XafDisplayName("Plastificado")]
        public bool bPlastificado
        {
            get => _bPlastificado;
            set => SetPropertyValue(nameof(bPlastificado), ref _bPlastificado, value);
        }

        private bool _bHendido;
        [XafDisplayName("Hendido")]
        public bool bHendido
        {
            get => _bHendido;
            set => SetPropertyValue(nameof(bHendido), ref _bHendido, value);
        }

        private bool _bLaminado;
        [XafDisplayName("Laminado")]
        public bool bLaminado
        {
            get => _bLaminado;
            set => SetPropertyValue(nameof(bLaminado), ref _bLaminado, value);
        }

        private bool _bM;
        [XafDisplayName("M.")]
        public bool bM
        {
            get => _bM;
            set => SetPropertyValue(nameof(bM), ref _bM, value);
        }

        private bool _bB;
        [XafDisplayName("B.")]
        public bool bB
        {
            get => _bB;
            set => SetPropertyValue(nameof(bB), ref _bB, value);
        }

        private bool _bV;
        [XafDisplayName("V.")]
        public bool bV
        {
            get => _bV;
            set => SetPropertyValue(nameof(bV), ref _bV, value);
        }

        private bool _b1c;
        [XafDisplayName("1 c.")]
        public bool b1c
        {
            get => _b1c;
            set => SetPropertyValue(nameof(b1c), ref _b1c, value);
        }

        private bool _b2c;
        [XafDisplayName("2 c.")]
        public bool b2c
        {
            get => _b2c;
            set => SetPropertyValue(nameof(b2c), ref _b2c, value);
        }

        [DevExpress.Xpo.Aggregated]
        public XPCollection<TrabajoImpresionPapel> Papeles => new XPCollection<TrabajoImpresionPapel>(Session, new BinaryOperator(nameof(LineaDocumentoVenta.DocumentoVenta), this));

        [DevExpress.Xpo.Aggregated]
        public XPCollection<TrabajoImpresionMaterial> Materiales => new XPCollection<TrabajoImpresionMaterial>(Session, new BinaryOperator(nameof(LineaDocumentoVenta.DocumentoVenta), this));

        [DevExpress.Xpo.Aggregated]
        public XPCollection<TrabajoImpresionServicio> Servicios => new XPCollection<TrabajoImpresionServicio>(Session, new BinaryOperator(nameof(LineaDocumentoVenta.DocumentoVenta), this));

        [DevExpress.Xpo.Aggregated]
        public XPCollection<TrabajoImpresionHora> Horas => new XPCollection<TrabajoImpresionHora>(Session, new BinaryOperator(nameof(LineaDocumentoVenta.DocumentoVenta), this));

        public enum EstadosPresupuesto { Borrador, Presupuesto, Orden, Rechazado };

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Estado = EstadosPresupuesto.Borrador;
        }
    }
}
