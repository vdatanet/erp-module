using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Ventas;

namespace erp.Module.BusinessObjects.Imprenta;

[DefaultClassOptions]
[NavigationItem("Imprenta")]
[XafDisplayName("Trabajo de Impresión")]
[ImageName("BO_Order")]
public class TrabajoImpresion : Presupuesto
{
    public enum EstadosPresupuesto
    {
        Borrador,
        Presupuesto,
        Orden,
        Rechazado
    }

    public TrabajoImpresion(Session session) : base(session)
    {
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadosPresupuesto.Borrador;
    }

    #region Datos Generales

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

    #endregion

    #region Impresión Digital

    private bool _digitalColor;

    [XafDisplayName("Digital Color")]
    public bool DigitalColor
    {
        get => _digitalColor;
        set => SetPropertyValue(nameof(DigitalColor), ref _digitalColor, value);
    }

    private bool _digitalColor1c;

    [XafDisplayName("Digital Color 1c.")]
    public bool DigitalColor1c
    {
        get => _digitalColor1c;
        set => SetPropertyValue(nameof(DigitalColor1c), ref _digitalColor1c, value);
    }

    private bool _digitalColor2c;

    [XafDisplayName("Digital Color 2c.")]
    public bool DigitalColor2c
    {
        get => _digitalColor2c;
        set => SetPropertyValue(nameof(DigitalColor2c), ref _digitalColor2c, value);
    }

    private bool _digitalBN;

    [XafDisplayName("Digital B/N")]
    public bool DigitalBN
    {
        get => _digitalBN;
        set => SetPropertyValue(nameof(DigitalBN), ref _digitalBN, value);
    }

    private bool _digitalBN1c;

    [XafDisplayName("Digital B/N 1c.")]
    public bool DigitalBN1c
    {
        get => _digitalBN1c;
        set => SetPropertyValue(nameof(DigitalBN1c), ref _digitalBN1c, value);
    }

    private bool _digitalBN2c;

    [XafDisplayName("Digital B/N 2c.")]
    public bool DigitalBN2c
    {
        get => _digitalBN2c;
        set => SetPropertyValue(nameof(DigitalBN2c), ref _digitalBN2c, value);
    }

    private bool _digital1c;

    [XafDisplayName("Digital 1c.")]
    public bool Digital1c
    {
        get => _digital1c;
        set => SetPropertyValue(nameof(Digital1c), ref _digital1c, value);
    }

    private bool _digital2c;

    [XafDisplayName("Digital 2c.")]
    public bool Digital2c
    {
        get => _digital2c;
        set => SetPropertyValue(nameof(Digital2c), ref _digital2c, value);
    }

    #endregion

    #region Acabados

    private bool _grapado;

    [XafDisplayName("Grapado")]
    public bool Grapado
    {
        get => _grapado;
        set => SetPropertyValue(nameof(Grapado), ref _grapado, value);
    }

    private bool _taladrado;

    [XafDisplayName("Taladrado")]
    public bool Taladrado
    {
        get => _taladrado;
        set => SetPropertyValue(nameof(Taladrado), ref _taladrado, value);
    }

    private bool _encolado;

    [XafDisplayName("Encolado")]
    public bool Encolado
    {
        get => _encolado;
        set => SetPropertyValue(nameof(Encolado), ref _encolado, value);
    }

    private bool _troquelado;

    [XafDisplayName("Troquelado")]
    public bool Troquelado
    {
        get => _troquelado;
        set => SetPropertyValue(nameof(Troquelado), ref _troquelado, value);
    }

    private bool _doblado;

    [XafDisplayName("Doblado")]
    public bool Doblado
    {
        get => _doblado;
        set => SetPropertyValue(nameof(Doblado), ref _doblado, value);
    }

    private bool _encuadernado;

    [XafDisplayName("Encuadernado")]
    public bool Encuadernado
    {
        get => _encuadernado;
        set => SetPropertyValue(nameof(Encuadernado), ref _encuadernado, value);
    }

    private bool _perforado;

    [XafDisplayName("Perforado")]
    public bool Perforado
    {
        get => _perforado;
        set => SetPropertyValue(nameof(Perforado), ref _perforado, value);
    }

    private int _perforadoMm;

    [XafDisplayName("Perforado (mm)")]
    public int PerforadoMm
    {
        get => _perforadoMm;
        set => SetPropertyValue(nameof(PerforadoMm), ref _perforadoMm, value);
    }

    private bool _troqueladora;

    [XafDisplayName("Troqueladora")]
    public bool Troqueladora
    {
        get => _troqueladora;
        set => SetPropertyValue(nameof(Troqueladora), ref _troqueladora, value);
    }

    private bool _numerado;

    [XafDisplayName("Numerado")]
    public bool Numerado
    {
        get => _numerado;
        set => SetPropertyValue(nameof(Numerado), ref _numerado, value);
    }

    private string? _numeracion;

    [XafDisplayName("Tipo Numerado")]
    public string? Numeracion
    {
        get => _numeracion;
        set => SetPropertyValue(nameof(Numeracion), ref _numeracion, value);
    }

    private bool _encoladoQ;

    [XafDisplayName("Encolado Q")]
    public bool EncoladoQ
    {
        get => _encoladoQ;
        set => SetPropertyValue(nameof(EncoladoQ), ref _encoladoQ, value);
    }

    private bool _plastificado;

    [XafDisplayName("Plastificado")]
    public bool Plastificado
    {
        get => _plastificado;
        set => SetPropertyValue(nameof(Plastificado), ref _plastificado, value);
    }

    private bool _hendido;

    [XafDisplayName("Hendido")]
    public bool Hendido
    {
        get => _hendido;
        set => SetPropertyValue(nameof(Hendido), ref _hendido, value);
    }

    private bool _laminado;

    [XafDisplayName("Laminado")]
    public bool Laminado
    {
        get => _laminado;
        set => SetPropertyValue(nameof(Laminado), ref _laminado, value);
    }

    #endregion

    #region Otros Flags

    private bool _m;

    [XafDisplayName("M.")]
    public bool M
    {
        get => _m;
        set => SetPropertyValue(nameof(M), ref _m, value);
    }

    private bool _b;

    [XafDisplayName("B.")]
    public bool B
    {
        get => _b;
        set => SetPropertyValue(nameof(B), ref _b, value);
    }

    private bool _v;

    [XafDisplayName("V.")]
    public bool V
    {
        get => _v;
        set => SetPropertyValue(nameof(V), ref _v, value);
    }

    private bool _c1;

    [XafDisplayName("1 c.")]
    public bool C1
    {
        get => _c1;
        set => SetPropertyValue(nameof(C1), ref _c1, value);
    }

    private bool _c2;

    [XafDisplayName("2 c.")]
    public bool C2
    {
        get => _c2;
        set => SetPropertyValue(nameof(C2), ref _c2, value);
    }

    #endregion

    #region Colecciones

    [DevExpress.Xpo.Aggregated]
    [Association("TrabajoImpresion-Papeles")]
    public XPCollection<TrabajoImpresionPapel> Papeles => GetCollection<TrabajoImpresionPapel>();

    [DevExpress.Xpo.Aggregated]
    [Association("TrabajoImpresion-Materiales")]
    public XPCollection<TrabajoImpresionMaterial> Materiales => GetCollection<TrabajoImpresionMaterial>();

    [DevExpress.Xpo.Aggregated]
    [Association("TrabajoImpresion-Servicios")]
    public XPCollection<TrabajoImpresionServicio> Servicios => GetCollection<TrabajoImpresionServicio>();

    [DevExpress.Xpo.Aggregated]
    [Association("TrabajoImpresion-Horas")]
    public XPCollection<TrabajoImpresionHora> Horas => GetCollection<TrabajoImpresionHora>();

    #endregion
}