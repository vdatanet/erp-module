using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Productos;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Impuestos;

[DefaultClassOptions]
[NavigationItem("Impuestos")]
[ImageName("Top10Percent")]
[DefaultProperty(nameof(Codigo))]
public class TipoImpuesto(Session session) : EntidadBase(session)
{
    private CausaExencion? _causaExencion;
    private string? _codigo;
    private Cuenta? _cuenta;
    private bool _disponibleEnCompras;
    private bool _disponibleEnVentas;
    private bool _esRetencion;
    private bool _estaActivo;
    private Impuesto? _impuesto;
    private string? _nombre;
    private string? _notas;
    private ClaveRegimen? _regimenFiscal;
    private int _secuencia;
    private decimal _tipo;
    private CalificacionOperacion? _tipoOperacion;

    [RuleRequiredField]
    [RuleUniqueValue]
    [XafDisplayName("Código")]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [XafDisplayName("Secuencia")]
    public int Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    [XafDisplayName("Cuenta")]
    public Cuenta? Cuenta
    {
        get => _cuenta;
        set => SetPropertyValue(nameof(Cuenta), ref _cuenta, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Tipo %")]
    public decimal Tipo
    {
        get => _tipo;
        set => SetPropertyValue(nameof(Tipo), ref _tipo, value);
    }

    [XafDisplayName("Activo")]
    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    [XafDisplayName("Disponible en Ventas")]
    public bool DisponibleEnVentas
    {
        get => _disponibleEnVentas;
        set => SetPropertyValue(nameof(DisponibleEnVentas), ref _disponibleEnVentas, value);
    }

    [XafDisplayName("Disponible en Compras")]
    public bool DisponibleEnCompras
    {
        get => _disponibleEnCompras;
        set => SetPropertyValue(nameof(DisponibleEnCompras), ref _disponibleEnCompras, value);
    }

    [XafDisplayName("Es Retención")]
    public bool EsRetencion
    {
        get => _esRetencion;
        set => SetPropertyValue(nameof(EsRetencion), ref _esRetencion, value);
    }

    [XafDisplayName("Impuesto VeriFactu")]
    public Impuesto? Impuesto
    {
        get => _impuesto;
        set => SetPropertyValue(nameof(Impuesto), ref _impuesto, value);
    }

    [XafDisplayName("Posición Fiscal")]
    public ClaveRegimen? RegimenFiscal
    {
        get => _regimenFiscal;
        set => SetPropertyValue(nameof(RegimenFiscal), ref _regimenFiscal, value);
    }

    [XafDisplayName("Tipo Operación")]
    public CalificacionOperacion? TipoOperacion
    {
        get => _tipoOperacion;
        set => SetPropertyValue(nameof(TipoOperacion), ref _tipoOperacion, value);
    }

    [XafDisplayName("Causa Exención")]
    public CausaExencion? CausaExencion
    {
        get => _causaExencion;
        set => SetPropertyValue(nameof(CausaExencion), ref _causaExencion, value);
    }

    [NonCloneable]
    [Association("LineaDocumentoVenta-TipoImpuestos")]
    public XPCollection<LineaDocumentoVenta> LineasDocumentoVenta => GetCollection<LineaDocumentoVenta>();

    [NonCloneable]
    [Association("Productos-ImpuestosVentas")]
    public XPCollection<Producto> ProductoImpuestosVenta => GetCollection<Producto>();

    [NonCloneable]
    [Association("Productos-ImpuestosCompras")]
    public XPCollection<Producto> ProductoImpuestosCompra => GetCollection<Producto>();

    [NonCloneable]
    [Association("TipoSuscripcion-Impuestos")]
    public XPCollection<Suscripciones.TipoSuscripcion> TipoSuscripciones => GetCollection<Suscripciones.TipoSuscripcion>();

    [NonCloneable]
    [Association("InformacionEmpresa-ImpuestosVentas")]
    public XPCollection<InformacionEmpresa> InformacionEmpresaImpuestosVenta => GetCollection<InformacionEmpresa>();

    [NonCloneable]
    [Association("InformacionEmpresa-ImpuestosCompras")]
    public XPCollection<InformacionEmpresa> InformacionEmpresaImpuestosCompra => GetCollection<InformacionEmpresa>();

    [NonCloneable]
    [Association("MapeoImpuesto-ImpuestosDestino")]
    public XPCollection<MapeoImpuesto> MapeosImpuestoDestino => GetCollection<MapeoImpuesto>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        EstaActivo = true;
        DisponibleEnVentas = false;
        DisponibleEnCompras = false;
        EsRetencion = false;
        Tipo = 0;
        Cuenta = null;
    }
}