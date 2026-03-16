using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Comun;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.Helpers.Contactos;
using Tarea = erp.Module.BusinessObjects.Planificacion.Tarea;

namespace erp.Module.BusinessObjects.Productos;

[DefaultClassOptions]
[NavigationItem("Productos")]
[ImageName("BO_Product")]
[DefaultProperty(nameof(Codigo))]
public class Producto(Session session) : EntidadBase(session)
{
    private Categoria _categoria;
    private string _codigo;
    private string _codigoBarras;
    private decimal _costeEstandar;
    private Cuenta _cuentaCompras;
    private Cuenta _cuentaVentas;
    private bool _disponibleEnCompras;
    private bool _disponibleEnTpv;
    private bool _disponibleEnVentas;
    private bool _estaActivo;
    private MediaDataObject _foto;
    private string _nombre;
    private string _notas;
    private decimal _precioVenta;

    [RuleUniqueValue]
    [XafDisplayName("Código")]
    public string Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [RuleUniqueValue]
    [XafDisplayName("Código Barras")]
    public string CodigoBarras
    {
        get => _codigoBarras;
        set => SetPropertyValue(nameof(CodigoBarras), ref _codigoBarras, value);
    }

    [RuleUniqueValue]
    [RuleRequiredField]
    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Association("Categoria-Productos")]
    [DataSourceCriteria("EstaActivo = True")]
    [XafDisplayName("Categoría")]
    public Categoria Categoria
    {
        get => _categoria;
        set => SetPropertyValue(nameof(Categoria), ref _categoria, value);
    }

    [XafDisplayName("Coste Estándar")]
    public decimal CosteEstandar
    {
        get => _costeEstandar;
        set => SetPropertyValue(nameof(CosteEstandar), ref _costeEstandar, value);
    }

    [XafDisplayName("Precio Venta")]
    public decimal PrecioVenta
    {
        get => _precioVenta;
        set => SetPropertyValue(nameof(PrecioVenta), ref _precioVenta, value);
    }

    [XafDisplayName("Cuenta Ventas")]
    public Cuenta CuentaVentas
    {
        get => _cuentaVentas;
        set => SetPropertyValue(nameof(CuentaVentas), ref _cuentaVentas, value);
    }

    [XafDisplayName("Cuenta Compras")]
    public Cuenta CuentaCompras
    {
        get => _cuentaCompras;
        set => SetPropertyValue(nameof(CuentaCompras), ref _cuentaCompras, value);
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

    [XafDisplayName("Disponible en TPV")]
    public bool DisponibleEnTpv
    {
        get => _disponibleEnTpv;
        set => SetPropertyValue(nameof(DisponibleEnTpv), ref _disponibleEnTpv, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [XafDisplayName("Foto")]
    public MediaDataObject Foto
    {
        get => _foto;
        set => SetPropertyValue(nameof(Foto), ref _foto, value);
    }

    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("Productos-ImpuestosVentas")]
    [DataSourceCriteria("DisponibleEnVentas = True AND EstaActivo = True")]
    [XafDisplayName("Impuestos Ventas")]
    public XPCollection<TipoImpuesto> SalesTaxes => GetCollection<TipoImpuesto>();

    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("Productos-ImpuestosCompras")]
    [DataSourceCriteria("DisponibleEnCompras = True AND EstaActivo = True")]
    [XafDisplayName("Impuestos Compras")]
    public XPCollection<TipoImpuesto> PurchaseTaxes => GetCollection<TipoImpuesto>();

    [DevExpress.Xpo.Aggregated]
    [Association("Producto-Tareas")]
    [XafDisplayName("Tareas")]
    public XPCollection<Tarea> Tareas => GetCollection<Tarea>();

    [DevExpress.Xpo.Aggregated]
    [Association("Producto-Fotos")]
    [XafDisplayName("Imágenes")]
    public XPCollection<Imagen> Imagenes => GetCollection<Imagen>();

    [DevExpress.Xpo.Aggregated]
    [Association("Producto-Adjuntos")]
    [XafDisplayName("Adjuntos")]
    public XPCollection<Adjunto> Adjuntos => GetCollection<Adjunto>();

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
        DisponibleEnTpv = false;
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        if (companyInfo.CuentaVentasPorDefecto != null) CuentaVentas = companyInfo.CuentaVentasPorDefecto;
        if (companyInfo.CuentaComprasPorDefecto != null) CuentaCompras = companyInfo.CuentaComprasPorDefecto;
    }
}