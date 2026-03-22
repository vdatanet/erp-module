using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Impuestos;

namespace erp.Module.BusinessObjects.Configuraciones;

[DefaultClassOptions]
[NavigationItem("Configuraciones")]
[XafDisplayName("Información de la Empresa")]
[DefaultProperty(nameof(Nombre))]
[ImageName("Actions_Settings")]
[RuleObjectExists("InformacionEmpresaExists", DefaultContexts.Save, "True", InvertResult = true,
    CustomMessageTemplate = "La información de la empresa ya existe.")]
[RuleCriteria("NotDeleteInformacionEmpresa", DefaultContexts.Delete, "False",
    CustomMessageTemplate = "No se puede eliminar la información de la empresa.")]
public class InformacionEmpresa(Session session) : Contacto(session)
{
    private Cuenta? _cuentaAcreedoresPorDefecto;
    private Cuenta? _cuentaPadreAcreedores;
    private Cuenta? _cuentaPadreClientes;
    private Cuenta? _cuentaPadreProveedores;
    private CondicionPago? _condicionPagoPorDefecto;
    private Cuenta? _cuentaClientesPorDefecto;
    private Cuenta? _cuentaCobrosPorDefecto;
    private Cuenta? _cuentaComprasPorDefecto;
    private Cuenta? _cuentaPagosPorDefecto;
    private Cuenta? _cuentaProveedoresPorDefecto;
    private Cuenta? _cuentaVentasPorDefecto;
    private Diario? _diarioComprasPorDefecto;
    private Diario? _diarioVentasPorDefecto;
    private string? _nifAdministradorSistemaVeriFactu;
    private string? _nombreAdministradorSistemaVeriFactu;
    private string? _nombreArchivoConfigVeriFactu;
    private string? _nombreSistemaVeriFactu;
    private PosicionFiscal? _posicionFiscalPorDefecto;
    private string? _prefijoAlbaranesCompraPorDefecto;
    private string? _prefijoFacturasCompraPorDefecto;
    private string? _prefijoFacturasSimplificadasPorDefecto;
    private string? _prefijoFacturasVentaPorDefecto;
    private string? _prefijoPartesDiariosPorDefecto;
    private string? _prefijoPedidosCompraPorDefecto;
    private string? _prefijoPedidosPorDefecto;
    private string? _prefijoPresupuestosCompraPorDefecto;
    private string? _prefijoPresupuestosPorDefecto;
    private string? _prefijoUrlValidacionVeriFactu;
    private string? _prefijoUrlVeriFactu;
    private string? _serieCertificadoVeriFactu;
    private string? _textoDefectoVeriFactu;
    private string? _versionSistemaVeriFactu;

    [DataSourceCriteria("EstaActivo = True")]
    [XafDisplayName("Diario Ventas por Defecto")]
    public Diario? DiarioVentasPorDefecto
    {
        get => _diarioVentasPorDefecto;
        set => SetPropertyValue(nameof(DiarioVentasPorDefecto), ref _diarioVentasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActivo = True")]
    [XafDisplayName("Diario Compras por Defecto")]
    public Diario? DiarioComprasPorDefecto
    {
        get => _diarioComprasPorDefecto;
        set => SetPropertyValue(nameof(DiarioComprasPorDefecto), ref _diarioComprasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta Ventas por Defecto")]
    public Cuenta? CuentaVentasPorDefecto
    {
        get => _cuentaVentasPorDefecto;
        set => SetPropertyValue(nameof(CuentaVentasPorDefecto), ref _cuentaVentasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta Compras por Defecto")]
    public Cuenta? CuentaComprasPorDefecto
    {
        get => _cuentaComprasPorDefecto;
        set => SetPropertyValue(nameof(CuentaComprasPorDefecto), ref _cuentaComprasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta Clientes por Defecto")]
    public Cuenta? CuentaClientesPorDefecto
    {
        get => _cuentaClientesPorDefecto;
        set => SetPropertyValue(nameof(CuentaClientesPorDefecto), ref _cuentaClientesPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta de Cobros por Defecto")]
    public Cuenta? CuentaCobrosPorDefecto
    {
        get => _cuentaCobrosPorDefecto;
        set => SetPropertyValue(nameof(CuentaCobrosPorDefecto), ref _cuentaCobrosPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta Proveedores por Defecto")]
    public Cuenta? CuentaProveedoresPorDefecto
    {
        get => _cuentaProveedoresPorDefecto;
        set => SetPropertyValue(nameof(CuentaProveedoresPorDefecto), ref _cuentaProveedoresPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta Acreedores por Defecto")]
    public Cuenta? CuentaAcreedoresPorDefecto
    {
        get => _cuentaAcreedoresPorDefecto;
        set => SetPropertyValue(nameof(CuentaAcreedoresPorDefecto), ref _cuentaAcreedoresPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True")]
    [XafDisplayName("Cuenta Padre Clientes")]
    public Cuenta? CuentaPadreClientes
    {
        get => _cuentaPadreClientes;
        set => SetPropertyValue(nameof(CuentaPadreClientes), ref _cuentaPadreClientes, value);
    }

    [DataSourceCriteria("EstaActiva = True")]
    [XafDisplayName("Cuenta Padre Proveedores")]
    public Cuenta? CuentaPadreProveedores
    {
        get => _cuentaPadreProveedores;
        set => SetPropertyValue(nameof(CuentaPadreProveedores), ref _cuentaPadreProveedores, value);
    }

    [DataSourceCriteria("EstaActiva = True")]
    [XafDisplayName("Cuenta Padre Acreedores")]
    public Cuenta? CuentaPadreAcreedores
    {
        get => _cuentaPadreAcreedores;
        set => SetPropertyValue(nameof(CuentaPadreAcreedores), ref _cuentaPadreAcreedores, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta de Pagos por Defecto")]
    public Cuenta? CuentaPagosPorDefecto
    {
        get => _cuentaPagosPorDefecto;
        set => SetPropertyValue(nameof(CuentaPagosPorDefecto), ref _cuentaPagosPorDefecto, value);
    }

    [XafDisplayName("Condiciones de Pago por Defecto")]
    public CondicionPago? CondicionPagoPorDefecto
    {
        get => _condicionPagoPorDefecto;
        set => SetPropertyValue(nameof(CondicionPagoPorDefecto), ref _condicionPagoPorDefecto, value);
    }

    [XafDisplayName("Posición Fiscal por Defecto")]
    public PosicionFiscal? PosicionFiscalPorDefecto
    {
        get => _posicionFiscalPorDefecto;
        set => SetPropertyValue(nameof(PosicionFiscalPorDefecto), ref _posicionFiscalPorDefecto, value);
    }

    [XafDisplayName("Prefijo Facturas Venta")]
    public string? PrefijoFacturasVentaPorDefecto
    {
        get => _prefijoFacturasVentaPorDefecto;
        set => SetPropertyValue(nameof(PrefijoFacturasVentaPorDefecto), ref _prefijoFacturasVentaPorDefecto, value);
    }

    [XafDisplayName("Prefijo Facturas Simplificadas")]
    public string? PrefijoFacturasSimplificadasPorDefecto
    {
        get => _prefijoFacturasSimplificadasPorDefecto;
        set => SetPropertyValue(nameof(PrefijoFacturasSimplificadasPorDefecto),
            ref _prefijoFacturasSimplificadasPorDefecto, value);
    }

    [XafDisplayName("Prefijo Facturas Compra")]
    public string? PrefijoFacturasCompraPorDefecto
    {
        get => _prefijoFacturasCompraPorDefecto;
        set => SetPropertyValue(nameof(PrefijoFacturasCompraPorDefecto), ref _prefijoFacturasCompraPorDefecto, value);
    }

    [XafDisplayName("Prefijo Pedidos Compra")]
    public string? PrefijoPedidosCompraPorDefecto
    {
        get => _prefijoPedidosCompraPorDefecto;
        set => SetPropertyValue(nameof(PrefijoPedidosCompraPorDefecto), ref _prefijoPedidosCompraPorDefecto, value);
    }

    [XafDisplayName("Prefijo Presupuestos Compra")]
    public string? PrefijoPresupuestosCompraPorDefecto
    {
        get => _prefijoPresupuestosCompraPorDefecto;
        set => SetPropertyValue(nameof(PrefijoPresupuestosCompraPorDefecto), ref _prefijoPresupuestosCompraPorDefecto,
            value);
    }

    [XafDisplayName("Prefijo Albaranes Compra")]
    public string? PrefijoAlbaranesCompraPorDefecto
    {
        get => _prefijoAlbaranesCompraPorDefecto;
        set => SetPropertyValue(nameof(PrefijoAlbaranesCompraPorDefecto), ref _prefijoAlbaranesCompraPorDefecto, value);
    }

    [XafDisplayName("Prefijo Pedidos")]
    public string? PrefijoPedidosPorDefecto
    {
        get => _prefijoPedidosPorDefecto;
        set => SetPropertyValue(nameof(PrefijoPedidosPorDefecto), ref _prefijoPedidosPorDefecto, value);
    }

    [XafDisplayName("Prefijo Presupuestos")]
    public string? PrefijoPresupuestosPorDefecto
    {
        get => _prefijoPresupuestosPorDefecto;
        set => SetPropertyValue(nameof(PrefijoPresupuestosPorDefecto), ref _prefijoPresupuestosPorDefecto, value);
    }

    [XafDisplayName("Prefijo Partes Diarios")]
    public string? PrefijoPartesDiariosPorDefecto
    {
        get => _prefijoPartesDiariosPorDefecto;
        set => SetPropertyValue(nameof(PrefijoPartesDiariosPorDefecto), ref _prefijoPartesDiariosPorDefecto, value);
    }

    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("InformacionEmpresa-ImpuestosVentas")]
    [DataSourceCriteria("DisponibleEnVentas = True AND EstaActivo = True")]
    [XafDisplayName("Impuestos Ventas")]
    public XPCollection<TipoImpuesto> ImpuestosVentas => GetCollection<TipoImpuesto>();

    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("InformacionEmpresa-ImpuestosCompras")]
    [DataSourceCriteria("DisponibleEnCompras = True AND EstaActivo = True")]
    [XafDisplayName("Impuestos Compras")]
    public XPCollection<TipoImpuesto> ImpuestosCompras => GetCollection<TipoImpuesto>();

    [Size(500)]
    [XafDisplayName("Texto Defecto VeriFactu")]
    public string? TextoDefectoVeriFactu
    {
        get => _textoDefectoVeriFactu;
        set => SetPropertyValue(nameof(TextoDefectoVeriFactu), ref _textoDefectoVeriFactu, value);
    }

    [XafDisplayName("Archivo Config VeriFactu")]
    public string? NombreArchivoConfigVeriFactu
    {
        get => _nombreArchivoConfigVeriFactu;
        set => SetPropertyValue(nameof(NombreArchivoConfigVeriFactu), ref _nombreArchivoConfigVeriFactu, value);
    }

    [XafDisplayName("Serie Certificado VeriFactu")]
    public string? SerieCertificadoVeriFactu
    {
        get => _serieCertificadoVeriFactu;
        set => SetPropertyValue(nameof(SerieCertificadoVeriFactu), ref _serieCertificadoVeriFactu, value);
    }

    [XafDisplayName("URL VeriFactu")]
    public string? PrefijoUrlVeriFactu
    {
        get => _prefijoUrlVeriFactu;
        set => SetPropertyValue(nameof(PrefijoUrlVeriFactu), ref _prefijoUrlVeriFactu, value);
    }

    [XafDisplayName("URL Validación VeriFactu")]
    public string? PrefijoUrlValidacionVeriFactu
    {
        get => _prefijoUrlValidacionVeriFactu;
        set => SetPropertyValue(nameof(PrefijoUrlValidacionVeriFactu), ref _prefijoUrlValidacionVeriFactu, value);
    }

    [Size(30)]
    [XafDisplayName("Nombre Sistema VeriFactu")]
    public string? NombreSistemaVeriFactu
    {
        get => _nombreSistemaVeriFactu;
        set => SetPropertyValue(nameof(NombreSistemaVeriFactu), ref _nombreSistemaVeriFactu, value);
    }

    [XafDisplayName("Versión Sistema VeriFactu")]
    public string? VersionSistemaVeriFactu
    {
        get => _versionSistemaVeriFactu;
        set => SetPropertyValue(nameof(VersionSistemaVeriFactu), ref _versionSistemaVeriFactu, value);
    }

    [XafDisplayName("Nombre Admin Sistema VeriFactu")]
    public string? NombreAdministradorSistemaVeriFactu
    {
        get => _nombreAdministradorSistemaVeriFactu;
        set => SetPropertyValue(nameof(NombreAdministradorSistemaVeriFactu), ref _nombreAdministradorSistemaVeriFactu,
            value);
    }

    [XafDisplayName("NIF Admin Sistema VeriFactu")]
    public string? NifAdministradorSistemaVeriFactu
    {
        get => _nifAdministradorSistemaVeriFactu;
        set => SetPropertyValue(nameof(NifAdministradorSistemaVeriFactu), ref _nifAdministradorSistemaVeriFactu, value);
    }
}