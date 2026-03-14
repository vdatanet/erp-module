using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Taxes;

namespace erp.Module.BusinessObjects.Settings;

[DefaultClassOptions]
[NavigationItem("Settings")]
[ImageName("Actions_Settings")]
[RuleObjectExists("CompanyInfoExists", DefaultContexts.Save, "True", InvertResult = true,
    CustomMessageTemplate = "Company Info already exists.")]
[RuleCriteria("NotDeleteCompanyInfo", DefaultContexts.Delete, "False",
    CustomMessageTemplate = "Can't delete Company Info.")]
public class CompanyInfo(Session session) : Contact(session)
{
    private Journal _diarioVentasPorDefecto;
    private Journal _diarioComprasPorDefecto;
    private Account _cuentaVentasPorDefecto;
    private Account _cuentaComprasPorDefecto;
    private Account _cuentaClientesPorDefecto;
    private Account _cuentaProveedoresPorDefecto;
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
    public Journal DiarioVentasPorDefecto
    {
        get => _diarioVentasPorDefecto;
        set => SetPropertyValue(nameof(DiarioVentasPorDefecto), ref _diarioVentasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActivo = True")]
    public Journal DiarioComprasPorDefecto
    {
        get => _diarioComprasPorDefecto;
        set => SetPropertyValue(nameof(DiarioComprasPorDefecto), ref _diarioComprasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Account CuentaVentasPorDefecto
    {
        get => _cuentaVentasPorDefecto;
        set => SetPropertyValue(nameof(CuentaVentasPorDefecto), ref _cuentaVentasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Account CuentaComprasPorDefecto
    {
        get => _cuentaComprasPorDefecto;
        set => SetPropertyValue(nameof(CuentaComprasPorDefecto), ref _cuentaComprasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Account CuentaClientesPorDefecto
    {
        get => _cuentaClientesPorDefecto;
        set => SetPropertyValue(nameof(CuentaClientesPorDefecto), ref _cuentaClientesPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Account CuentaProveedoresPorDefecto
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
    [Association("CompanyInfos-SalesTaxes")]
    [DataSourceCriteria("DisponibleEnVentas = True AND EstaActivo = True")]
    public XPCollection<TaxKind> SalesTaxes => GetCollection<TaxKind>(nameof(SalesTaxes));
    
    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("CompanyInfos-PurchaseTaxes")]
    [DataSourceCriteria("DisponibleEnCompras = True AND EstaActivo = True")]
    public XPCollection<TaxKind> PurchaseTaxes => GetCollection<TaxKind>(nameof(PurchaseTaxes));
}