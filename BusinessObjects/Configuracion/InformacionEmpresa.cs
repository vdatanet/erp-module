using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Contabilidad;

using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Impuestos;

namespace erp.Module.BusinessObjects.Configuracion;

[DefaultClassOptions]
[NavigationItem("Configuracion")]
[ImageName("Actions_Settings")]
[RuleObjectExists("InformacionEmpresaExists", DefaultContexts.Save, "True", InvertResult = true,
    CustomMessageTemplate = "Company Info already exists.")]
[RuleCriteria("NotDeleteInformacionEmpresa", DefaultContexts.Delete, "False",
    CustomMessageTemplate = "Can't delete Company Info.")]
public class InformacionEmpresa(Session session) : Contacto(session)
{
    private Diario _diarioVentasPorDefecto;
    private Diario _diarioComprasPorDefecto;
    private Cuenta _cuentaVentasPorDefecto;
    private Cuenta _cuentaComprasPorDefecto;
    private Cuenta _cuentaClientesPorDefecto;
    private Cuenta _cuentaProveedoresPorDefecto;
    private string _prefijoFacturasVentaPorDefecto;
    private string _prefijoFacturasCompraPorDefecto;
    private string _prefijoPartesDiariosPorDefecto;
    private string _textoDefectoVeriFactu;
    private string _nombreArchivoConfigVeriFactu;
    private string _serieCertificadoVeriFactu;
    private string _prefijoUrlVeriFactu;
    private string _prefijoUrlValidacionVeriFactu;
    private string _nombreSistemaVeriFactu;
    private string _versionSistemaVeriFactu;
    private string _nombreAdministradorSistemaVeriFactu;
    private string _nifAdministradorSistemaVeriFactu;

    [DataSourceCriteria("EstaActivo = True")]
    public Diario DiarioVentasPorDefecto
    {
        get => _diarioVentasPorDefecto;
        set => SetPropertyValue(nameof(DiarioVentasPorDefecto), ref _diarioVentasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActivo = True")]
    public Diario DiarioComprasPorDefecto
    {
        get => _diarioComprasPorDefecto;
        set => SetPropertyValue(nameof(DiarioComprasPorDefecto), ref _diarioComprasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Cuenta CuentaVentasPorDefecto
    {
        get => _cuentaVentasPorDefecto;
        set => SetPropertyValue(nameof(CuentaVentasPorDefecto), ref _cuentaVentasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Cuenta CuentaComprasPorDefecto
    {
        get => _cuentaComprasPorDefecto;
        set => SetPropertyValue(nameof(CuentaComprasPorDefecto), ref _cuentaComprasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Cuenta CuentaClientesPorDefecto
    {
        get => _cuentaClientesPorDefecto;
        set => SetPropertyValue(nameof(CuentaClientesPorDefecto), ref _cuentaClientesPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Cuenta CuentaProveedoresPorDefecto
    {
        get => _cuentaProveedoresPorDefecto;
        set => SetPropertyValue(nameof(CuentaProveedoresPorDefecto), ref _cuentaProveedoresPorDefecto, value);
    }

    public string PrefijoFacturasVentaPorDefecto
    {
        get => _prefijoFacturasVentaPorDefecto;
        set => SetPropertyValue(nameof(PrefijoFacturasVentaPorDefecto), ref _prefijoFacturasVentaPorDefecto, value);
    }

    public string PrefijoFacturasCompraPorDefecto
    {
        get => _prefijoFacturasCompraPorDefecto;
        set => SetPropertyValue(nameof(PrefijoFacturasCompraPorDefecto), ref _prefijoFacturasCompraPorDefecto, value);
    }

    public string PrefijoPartesDiariosPorDefecto
    {
        get => _prefijoPartesDiariosPorDefecto;
        set => SetPropertyValue(nameof(PrefijoPartesDiariosPorDefecto), ref _prefijoPartesDiariosPorDefecto, value);
    }

    [Size(500)]
    public string TextoDefectoVeriFactu
    {
        get => _textoDefectoVeriFactu;
        set => SetPropertyValue(nameof(TextoDefectoVeriFactu), ref _textoDefectoVeriFactu, value);
    }

    public string NombreArchivoConfigVeriFactu
    {
        get => _nombreArchivoConfigVeriFactu;
        set => SetPropertyValue(nameof(NombreArchivoConfigVeriFactu), ref _nombreArchivoConfigVeriFactu, value);
    }

    public string SerieCertificadoVeriFactu
    {
        get => _serieCertificadoVeriFactu;
        set => SetPropertyValue(nameof(SerieCertificadoVeriFactu), ref _serieCertificadoVeriFactu, value);
    }
    
    public string PrefijoUrlVeriFactu
    {
        get => _prefijoUrlVeriFactu;
        set => SetPropertyValue(nameof(PrefijoUrlVeriFactu), ref _prefijoUrlVeriFactu, value);
    }
    
    public string PrefijoUrlValidacionVeriFactu
    {
        get => _prefijoUrlValidacionVeriFactu;
        set => SetPropertyValue(nameof(PrefijoUrlValidacionVeriFactu), ref _prefijoUrlValidacionVeriFactu, value);
    }

    [Size(30)]
    public string NombreSistemaVeriFactu
    {
        get => _nombreSistemaVeriFactu;
        set => SetPropertyValue(nameof(NombreSistemaVeriFactu), ref _nombreSistemaVeriFactu, value);
    }
    
    public string VersionSistemaVeriFactu
    {
        get => _versionSistemaVeriFactu;
        set => SetPropertyValue(nameof(VersionSistemaVeriFactu), ref _versionSistemaVeriFactu, value);
    }
    
    public string NombreAdministradorSistemaVeriFactu
    {
        get => _nombreAdministradorSistemaVeriFactu;
        set => SetPropertyValue(nameof(NombreAdministradorSistemaVeriFactu), ref _nombreAdministradorSistemaVeriFactu, value);
    }
    
    public string NifAdministradorSistemaVeriFactu
    {
        get => _nifAdministradorSistemaVeriFactu;
        set => SetPropertyValue(nameof(NifAdministradorSistemaVeriFactu), ref _nifAdministradorSistemaVeriFactu, value);
    }
    
    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("InformacionEmpresas-SalesTaxes")]
    [DataSourceCriteria("DisponibleEnVentas = True AND EstaActivo = True")]
    public XPCollection<TipoImpuesto> SalesTaxes => GetCollection<TipoImpuesto>(nameof(SalesTaxes));
    
    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("InformacionEmpresas-PurchaseTaxes")]
    [DataSourceCriteria("DisponibleEnCompras = True AND EstaActivo = True")]
    public XPCollection<TipoImpuesto> PurchaseTaxes => GetCollection<TipoImpuesto>(nameof(PurchaseTaxes));
}