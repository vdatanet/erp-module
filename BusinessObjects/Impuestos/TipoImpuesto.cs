using System.ComponentModel;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Configuracion;
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
    private string _codigo;
    private string _nombre;
    private string _notas;
    private int _secuencia;
    private Cuenta _cuenta;
    private decimal _tipo;
    private bool _estaActivo;
    private bool _disponibleEnVentas;
    private bool _disponibleEnCompras;
    private bool _esRetencion;
    private Impuesto? _impuesto;
    private ClaveRegimen? _regimenFiscal;
    private CalificacionOperacion? _tipoOperacion;
    private CausaExencion? _causaExencion;

    [RuleRequiredField]
    [RuleUniqueValue]
    public string Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [Size(255)]
    [RuleRequiredField]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }
    
    public int Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    public Cuenta Cuenta
    {
        get => _cuenta;
        set => SetPropertyValue(nameof(Cuenta), ref _cuenta, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal Tipo
    {
        get => _tipo;
        set => SetPropertyValue(nameof(Tipo), ref _tipo, value);
    }

    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    public bool DisponibleEnVentas
    {
        get => _disponibleEnVentas;
        set => SetPropertyValue(nameof(DisponibleEnVentas), ref _disponibleEnVentas, value);
    }

    public bool DisponibleEnCompras
    {
        get => _disponibleEnCompras;
        set => SetPropertyValue(nameof(DisponibleEnCompras), ref _disponibleEnCompras, value);
    }

    public bool EsRetencion
    {
        get => _esRetencion;
        set => SetPropertyValue(nameof(EsRetencion), ref _esRetencion, value);
    }
    public Impuesto? Impuesto
    {
        get => _impuesto;
        set => SetPropertyValue(nameof(Impuesto), ref _impuesto, value);
    }
    public ClaveRegimen? RegimenFiscal
    {
        get => _regimenFiscal;
        set => SetPropertyValue(nameof(RegimenFiscal), ref _regimenFiscal, value);
    }
    
    public CalificacionOperacion? TipoOperacion
    {
        get => _tipoOperacion;
        set => SetPropertyValue(nameof(TipoOperacion), ref _tipoOperacion, value);
    }
    
    public CausaExencion? CausaExencion
    {
        get => _causaExencion;
        set => SetPropertyValue(nameof(CausaExencion), ref _causaExencion, value);
    }

    [NonCloneable]
    [Association("LineaDocumentoVentas-TipoImpuestos")]
    public XPCollection<LineaDocumentoVenta> LineasDocumentoVenta => GetCollection<LineaDocumentoVenta>(nameof(LineasDocumentoVenta));

    [NonCloneable]
    [Association("Products-SalesTaxes")]
    public XPCollection<Producto> ProductoImpuestosVenta => GetCollection<Producto>(nameof(ProductoImpuestosVenta));

    [NonCloneable]
    [Association("Products-PurchaseTaxes")]
    public XPCollection<Producto> ProductoImpuestosCompra => GetCollection<Producto>(nameof(ProductoImpuestosCompra));

    [NonCloneable]
    [Association("InformacionEmpresas-SalesTaxes")]
    public XPCollection<InformacionEmpresa> InformacionEmpresaImpuestosVenta => GetCollection<InformacionEmpresa>(nameof(InformacionEmpresaImpuestosVenta));

    [NonCloneable]
    [Association("InformacionEmpresas-PurchaseTaxes")]
    public XPCollection<InformacionEmpresa> InformacionEmpresaImpuestosCompra => GetCollection<InformacionEmpresa>(nameof(InformacionEmpresaImpuestosCompra));
    
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