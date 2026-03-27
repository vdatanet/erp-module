using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using DevExpress.Persistent.BaseImpl;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Base.Comun;
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
public class InformacionEmpresa(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private string? _nombreComercial;
    private string? _nif;
    private string? _direccion;
    private string? _codigoPostal;
    private Pais? _pais;
    private Provincia? _provincia;
    private Poblacion? _poblacion;
    private string? _telefono;
    private string? _movil;
    private string? _correoElectronico;
    private string? _sitioWeb;
    private ZonaHoraria? _zonaHorariaPorDefecto;

    private CuentaContable? _cuentaAcreedoresPorDefecto;
    private CuentaContable? _cuentaPadreAcreedores;
    private CuentaContable? _cuentaPadreClientes;
    private CuentaContable? _cuentaPadreProveedores;
    private CondicionPago? _condicionPagoPorDefecto;
    private CuentaContable? _cuentaClientesPorDefecto;
    private CuentaContable? _cuentaCobrosPorDefecto;
    private CuentaContable? _cuentaComprasPorDefecto;
    private CuentaContable? _cuentaPagosPorDefecto;
    private CuentaContable? _cuentaProveedoresPorDefecto;
    private CuentaContable? _cuentaVentasPorDefecto;
    private MedioPago? _medioPagoPorDefecto;
    private Diario? _diarioComprasPorDefecto;
    private Diario? _diarioVentasPorDefecto;
    private Diario? _diarioTesoreriaPorDefecto;
    private Diario? _diarioOperacionesVariasPorDefecto;
    private Diario? _diarioAperturaPorDefecto;
    private Diario? _diarioCierrePorDefecto;
    private Diario? _diarioRegularizacionPorDefecto;
    private string? _nifAdministradorSistemaVeriFactu;
    private string? _nombreAdministradorSistemaVeriFactu;
    private string? _nombreArchivoConfigVeriFactu;
    private string? _nombreSistemaVeriFactu;
    private PosicionFiscal? _posicionFiscalPorDefecto;
    private string? _prefijoAsientosPorDefecto;
    private string? _prefijoAlbaranesCompraPorDefecto;
    private string? _prefijoAlbaranesVentaPorDefecto;
    private string? _prefijoFacturasCompraPorDefecto;
    private string? _prefijoFacturasSimplificadasPorDefecto;
    private string? _prefijoFacturasVentaPorDefecto;
    private string? _prefijoParteTrabajoPorDefecto;
    private string? _prefijoPedidosCompraPorDefecto;
    private string? _prefijoPedidosPorDefecto;
    private string? _prefijoOfertasCompraPorDefecto;
    private string? _prefijoOfertasVentaPorDefecto;
    private string? _prefijoUrlValidacionVeriFactu;
    private string? _prefijoUrlVeriFactu;
    private string? _prefijoClientes;
    private string? _prefijoProveedores;
    private string? _prefijoAcreedores;
    private string? _prefijoEmpleados;
    private string? _prefijoReservas;
    private string? _prefijoSesionTpvPorDefecto;
    private string? _prefijoVentaTpvPorDefecto;
    private string? _serieCertificadoVeriFactu;
    private string? _textoDefectoVeriFactu;
    private string? _versionSistemaVeriFactu;
    private MediaDataObject? _logo;
    private bool _activarVeriFactu;
    private bool _activarFacturae;
    private string? _serieCertificadoFacturae;
    private string? _nombreReporteTicket;
    private bool _impresionDirectaTicket;
    private string? _unidadOrganicaOficinaContable;
    private string? _unidadOrganicaOrganoGestor;
    private string? _unidadOrganicaUnidadTramitadora;
    private int _paddingNumero;
    private int _paddingCuentaContable;
    private TipoNumeracionDocumento _tipoNumeracionDocumento;

    [Size(255)]
    [XafDisplayName("Nombre")]
    [RuleRequiredField("RuleRequiredField_InformacionEmpresa_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre es obligatorio")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(255)]
    [XafDisplayName("Nombre Comercial")]
    public string? NombreComercial
    {
        get => _nombreComercial;
        set => SetPropertyValue(nameof(NombreComercial), ref _nombreComercial, value);
    }

    [Size(50)]
    [XafDisplayName("NIF")]
    public string? Nif
    {
        get => _nif;
        set => SetPropertyValue(nameof(Nif), ref _nif, value);
    }

    [Size(255)]
    [XafDisplayName("Dirección")]
    public string? Direccion
    {
        get => _direccion;
        set => SetPropertyValue(nameof(Direccion), ref _direccion, value);
    }

    [Size(10)]
    [XafDisplayName("Código Postal")]
    public string? CodigoPostal
    {
        get => _codigoPostal;
        set => SetPropertyValue(nameof(CodigoPostal), ref _codigoPostal, value);
    }

    [XafDisplayName("País")]
    public Pais? Pais
    {
        get => _pais;
        set => SetPropertyValue(nameof(Pais), ref _pais, value);
    }

    [DataSourceProperty("Pais.Provincias")]
    [XafDisplayName("Provincia")]
    public Provincia? Provincia
    {
        get => _provincia;
        set => SetPropertyValue(nameof(Provincia), ref _provincia, value);
    }

    [DataSourceProperty("Provincia.Poblaciones")]
    [XafDisplayName("Población")]
    public Poblacion? Poblacion
    {
        get => _poblacion;
        set => SetPropertyValue(nameof(Poblacion), ref _poblacion, value);
    }

    [XafDisplayName("Teléfono")]
    public string? Telefono
    {
        get => _telefono;
        set => SetPropertyValue(nameof(Telefono), ref _telefono, value);
    }

    [XafDisplayName("Móvil")]
    public string? Movil
    {
        get => _movil;
        set => SetPropertyValue(nameof(Movil), ref _movil, value);
    }

    [XafDisplayName("Correo Electrónico")]
    public string? CorreoElectronico
    {
        get => _correoElectronico;
        set => SetPropertyValue(nameof(CorreoElectronico), ref _correoElectronico, value);
    }

    [XafDisplayName("Sitio Web")]
    public string? SitioWeb
    {
        get => _sitioWeb;
        set => SetPropertyValue(nameof(SitioWeb), ref _sitioWeb, value);
    }

    [XafDisplayName("Zona Horaria por Defecto")]
    public ZonaHoraria? ZonaHorariaPorDefecto
    {
        get => _zonaHorariaPorDefecto;
        set => SetPropertyValue(nameof(ZonaHorariaPorDefecto), ref _zonaHorariaPorDefecto, value);
    }

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

    [DataSourceCriteria("EstaActivo = True")]
    [XafDisplayName("Diario Tesorería por Defecto")]
    public Diario? DiarioTesoreriaPorDefecto
    {
        get => _diarioTesoreriaPorDefecto;
        set => SetPropertyValue(nameof(DiarioTesoreriaPorDefecto), ref _diarioTesoreriaPorDefecto, value);
    }

    [DataSourceCriteria("EstaActivo = True")]
    [XafDisplayName("Diario Operaciones Varias por Defecto")]
    public Diario? DiarioOperacionesVariasPorDefecto
    {
        get => _diarioOperacionesVariasPorDefecto;
        set => SetPropertyValue(nameof(DiarioOperacionesVariasPorDefecto), ref _diarioOperacionesVariasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActivo = True")]
    [XafDisplayName("Diario Apertura por Defecto")]
    public Diario? DiarioAperturaPorDefecto
    {
        get => _diarioAperturaPorDefecto;
        set => SetPropertyValue(nameof(DiarioAperturaPorDefecto), ref _diarioAperturaPorDefecto, value);
    }

    [DataSourceCriteria("EstaActivo = True")]
    [XafDisplayName("Diario Cierre por Defecto")]
    public Diario? DiarioCierrePorDefecto
    {
        get => _diarioCierrePorDefecto;
        set => SetPropertyValue(nameof(DiarioCierrePorDefecto), ref _diarioCierrePorDefecto, value);
    }

    [DataSourceCriteria("EstaActivo = True")]
    [XafDisplayName("Diario Regularización por Defecto")]
    public Diario? DiarioRegularizacionPorDefecto
    {
        get => _diarioRegularizacionPorDefecto;
        set => SetPropertyValue(nameof(DiarioRegularizacionPorDefecto), ref _diarioRegularizacionPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta Contable Ventas por Defecto")]
    public CuentaContable? CuentaVentasPorDefecto
    {
        get => _cuentaVentasPorDefecto;
        set => SetPropertyValue(nameof(CuentaVentasPorDefecto), ref _cuentaVentasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta Contable Compras por Defecto")]
    public CuentaContable? CuentaComprasPorDefecto
    {
        get => _cuentaComprasPorDefecto;
        set => SetPropertyValue(nameof(CuentaComprasPorDefecto), ref _cuentaComprasPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta Contable Clientes por Defecto")]
    public CuentaContable? CuentaClientesPorDefecto
    {
        get => _cuentaClientesPorDefecto;
        set => SetPropertyValue(nameof(CuentaClientesPorDefecto), ref _cuentaClientesPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta Contable de Cobros por Defecto")]
    public CuentaContable? CuentaCobrosPorDefecto
    {
        get => _cuentaCobrosPorDefecto;
        set => SetPropertyValue(nameof(CuentaCobrosPorDefecto), ref _cuentaCobrosPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta Contable Proveedores por Defecto")]
    public CuentaContable? CuentaProveedoresPorDefecto
    {
        get => _cuentaProveedoresPorDefecto;
        set => SetPropertyValue(nameof(CuentaProveedoresPorDefecto), ref _cuentaProveedoresPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta Contable Acreedores por Defecto")]
    public CuentaContable? CuentaAcreedoresPorDefecto
    {
        get => _cuentaAcreedoresPorDefecto;
        set => SetPropertyValue(nameof(CuentaAcreedoresPorDefecto), ref _cuentaAcreedoresPorDefecto, value);
    }

    [DataSourceCriteria("EstaActiva = True")]
    [XafDisplayName("Cuenta Contable Padre Clientes")]
    public CuentaContable? CuentaPadreClientes
    {
        get => _cuentaPadreClientes;
        set => SetPropertyValue(nameof(CuentaPadreClientes), ref _cuentaPadreClientes, value);
    }

    [DataSourceCriteria("EstaActiva = True")]
    [XafDisplayName("Cuenta Contable Padre Proveedores")]
    public CuentaContable? CuentaPadreProveedores
    {
        get => _cuentaPadreProveedores;
        set => SetPropertyValue(nameof(CuentaPadreProveedores), ref _cuentaPadreProveedores, value);
    }

    [DataSourceCriteria("EstaActiva = True")]
    [XafDisplayName("Cuenta Contable Padre Acreedores")]
    public CuentaContable? CuentaPadreAcreedores
    {
        get => _cuentaPadreAcreedores;
        set => SetPropertyValue(nameof(CuentaPadreAcreedores), ref _cuentaPadreAcreedores, value);
    }

    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    [XafDisplayName("Cuenta Contable de Pagos por Defecto")]
    public CuentaContable? CuentaPagosPorDefecto
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

    [XafDisplayName("Medio de Pago por Defecto")]
    public MedioPago? MedioPagoPorDefecto
    {
        get => _medioPagoPorDefecto;
        set => SetPropertyValue(nameof(MedioPagoPorDefecto), ref _medioPagoPorDefecto, value);
    }

    [XafDisplayName("Posición Fiscal por Defecto")]
    public PosicionFiscal? PosicionFiscalPorDefecto
    {
        get => _posicionFiscalPorDefecto;
        set => SetPropertyValue(nameof(PosicionFiscalPorDefecto), ref _posicionFiscalPorDefecto, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoFacturasVentaPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Facturas Venta")]
    public string? PrefijoFacturasVentaPorDefecto
    {
        get => _prefijoFacturasVentaPorDefecto;
        set => SetPropertyValue(nameof(PrefijoFacturasVentaPorDefecto), ref _prefijoFacturasVentaPorDefecto, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoFacturasSimplificadasPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Facturas Simplificadas")]
    public string? PrefijoFacturasSimplificadasPorDefecto
    {
        get => _prefijoFacturasSimplificadasPorDefecto;
        set => SetPropertyValue(nameof(PrefijoFacturasSimplificadasPorDefecto),
            ref _prefijoFacturasSimplificadasPorDefecto, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoFacturasCompraPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Facturas Compra")]
    public string? PrefijoFacturasCompraPorDefecto
    {
        get => _prefijoFacturasCompraPorDefecto;
        set => SetPropertyValue(nameof(PrefijoFacturasCompraPorDefecto), ref _prefijoFacturasCompraPorDefecto, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoPedidosCompraPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Pedidos Compra")]
    public string? PrefijoPedidosCompraPorDefecto
    {
        get => _prefijoPedidosCompraPorDefecto;
        set => SetPropertyValue(nameof(PrefijoPedidosCompraPorDefecto), ref _prefijoPedidosCompraPorDefecto, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoOfertasCompraPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Ofertas Compra")]
    public string? PrefijoOfertasCompraPorDefecto
    {
        get => _prefijoOfertasCompraPorDefecto;
        set => SetPropertyValue(nameof(PrefijoOfertasCompraPorDefecto), ref _prefijoOfertasCompraPorDefecto,
            value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoAlbaranesCompraPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Albaranes Compra")]
    public string? PrefijoAlbaranesCompraPorDefecto
    {
        get => _prefijoAlbaranesCompraPorDefecto;
        set => SetPropertyValue(nameof(PrefijoAlbaranesCompraPorDefecto), ref _prefijoAlbaranesCompraPorDefecto, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoAlbaranesVentaPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Albaranes Venta")]
    public string? PrefijoAlbaranesVentaPorDefecto
    {
        get => _prefijoAlbaranesVentaPorDefecto;
        set => SetPropertyValue(nameof(PrefijoAlbaranesVentaPorDefecto), ref _prefijoAlbaranesVentaPorDefecto, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoPedidosPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Pedidos")]
    public string? PrefijoPedidosPorDefecto
    {
        get => _prefijoPedidosPorDefecto;
        set => SetPropertyValue(nameof(PrefijoPedidosPorDefecto), ref _prefijoPedidosPorDefecto, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoOfertasVentaPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Ofertas Venta")]
    public string? PrefijoOfertasVentaPorDefecto
    {
        get => _prefijoOfertasVentaPorDefecto;
        set => SetPropertyValue(nameof(PrefijoOfertasVentaPorDefecto), ref _prefijoOfertasVentaPorDefecto, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoParteTrabajoPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Parte de Trabajo")]
    public string? PrefijoParteTrabajoPorDefecto
    {
        get => _prefijoParteTrabajoPorDefecto;
        set => SetPropertyValue(nameof(PrefijoParteTrabajoPorDefecto), ref _prefijoParteTrabajoPorDefecto, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoAsientosPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Asientos")]
    public string? PrefijoAsientosPorDefecto
    {
        get => _prefijoAsientosPorDefecto;
        set => SetPropertyValue(nameof(PrefijoAsientosPorDefecto), ref _prefijoAsientosPorDefecto, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoClientes_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Clientes")]
    public string? PrefijoClientes
    {
        get => _prefijoClientes;
        set => SetPropertyValue(nameof(PrefijoClientes), ref _prefijoClientes, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoProveedores_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Proveedores")]
    public string? PrefijoProveedores
    {
        get => _prefijoProveedores;
        set => SetPropertyValue(nameof(PrefijoProveedores), ref _prefijoProveedores, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoAcreedores_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Acreedores")]
    public string? PrefijoAcreedores
    {
        get => _prefijoAcreedores;
        set => SetPropertyValue(nameof(PrefijoAcreedores), ref _prefijoAcreedores, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoEmpleados_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Empleados")]
    public string? PrefijoEmpleados
    {
        get => _prefijoEmpleados;
        set => SetPropertyValue(nameof(PrefijoEmpleados), ref _prefijoEmpleados, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoReservas_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Reservas")]
    public string? PrefijoReservas
    {
        get => _prefijoReservas;
        set => SetPropertyValue(nameof(PrefijoReservas), ref _prefijoReservas, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoSesionTpvPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Sesiones TPV")]
    public string? PrefijoSesionTpvPorDefecto
    {
        get => _prefijoSesionTpvPorDefecto;
        set => SetPropertyValue(nameof(PrefijoSesionTpvPorDefecto), ref _prefijoSesionTpvPorDefecto, value);
    }

    [RuleRequiredField("InformacionEmpresa_PrefijoVentaTpvPorDefecto_Required", DefaultContexts.Save)]
    [XafDisplayName("Prefijo Ventas TPV")]
    public string? PrefijoVentaTpvPorDefecto
    {
        get => _prefijoVentaTpvPorDefecto;
        set => SetPropertyValue(nameof(PrefijoVentaTpvPorDefecto), ref _prefijoVentaTpvPorDefecto, value);
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

    [XafDisplayName("Logo")]
    [ImageEditor]
    public MediaDataObject? Logo
    {
        get => _logo;
        set => SetPropertyValue(nameof(Logo), ref _logo, value);
    }

    [XafDisplayName("Activar VeriFactu")]
    public bool ActivarVeriFactu
    {
        get => _activarVeriFactu;
        set => SetPropertyValue(nameof(ActivarVeriFactu), ref _activarVeriFactu, value);
    }

    [XafDisplayName("Activar Facturae")]
    public bool ActivarFacturae
    {
        get => _activarFacturae;
        set => SetPropertyValue(nameof(ActivarFacturae), ref _activarFacturae, value);
    }

    [Size(255)]
    [XafDisplayName("Serie Certificado Facturae")]
    public string? SerieCertificadoFacturae
    {
        get => _serieCertificadoFacturae;
        set => SetPropertyValue(nameof(SerieCertificadoFacturae), ref _serieCertificadoFacturae, value);
    }

    [Size(50)]
    [XafDisplayName("Oficina Contable (FACe)")]
    public string? UnidadOrganicaOficinaContable
    {
        get => _unidadOrganicaOficinaContable;
        set => SetPropertyValue(nameof(UnidadOrganicaOficinaContable), ref _unidadOrganicaOficinaContable, value);
    }

    [Size(50)]
    [XafDisplayName("Órgano Gestor (FACe)")]
    public string? UnidadOrganicaOrganoGestor
    {
        get => _unidadOrganicaOrganoGestor;
        set => SetPropertyValue(nameof(UnidadOrganicaOrganoGestor), ref _unidadOrganicaOrganoGestor, value);
    }

    [Size(50)]
    [XafDisplayName("Unidad Tramitadora (FACe)")]
    public string? UnidadOrganicaUnidadTramitadora
    {
        get => _unidadOrganicaUnidadTramitadora;
        set => SetPropertyValue(nameof(UnidadOrganicaUnidadTramitadora), ref _unidadOrganicaUnidadTramitadora, value);
    }

    [XafDisplayName("Nombre Admin Sistema VeriFactu")]
    public string? NombreAdministradorSistemaVeriFactu
    {
        get => _nombreAdministradorSistemaVeriFactu;
        set => SetPropertyValue(nameof(NombreAdministradorSistemaVeriFactu), ref _nombreAdministradorSistemaVeriFactu,
            value);
    }

    [RuleRange("InformacionEmpresa_PaddingNumero_Range", DefaultContexts.Save, 1, 10)]
    [XafDisplayName("Padding Número")]
    public int PaddingNumero
    {
        get => _paddingNumero == 0 ? 5 : _paddingNumero;
        set => SetPropertyValue(nameof(PaddingNumero), ref _paddingNumero, value);
    }

    [RuleRange("InformacionEmpresa_PaddingCuentaContable_Range", DefaultContexts.Save, 1, 15)]
    [XafDisplayName("Padding Cuenta Contable")]
    public int PaddingCuentaContable
    {
        get => _paddingCuentaContable == 0 ? 10 : _paddingCuentaContable;
        set => SetPropertyValue(nameof(PaddingCuentaContable), ref _paddingCuentaContable, value);
    }

    [XafDisplayName("NIF Admin Sistema VeriFactu")]
    public string? NifAdministradorSistemaVeriFactu
    {
        get => _nifAdministradorSistemaVeriFactu;
        set => SetPropertyValue(nameof(NifAdministradorSistemaVeriFactu), ref _nifAdministradorSistemaVeriFactu, value);
    }

    [XafDisplayName("Nombre Reporte Ticket")]
    public string? NombreReporteTicket
    {
        get => _nombreReporteTicket;
        set => SetPropertyValue(nameof(NombreReporteTicket), ref _nombreReporteTicket, value);
    }

    [XafDisplayName("Impresión Directa Ticket")]
    public bool ImpresionDirectaTicket
    {
        get => _impresionDirectaTicket;
        set => SetPropertyValue(nameof(ImpresionDirectaTicket), ref _impresionDirectaTicket, value);
    }

    [XafDisplayName("Tipo Numeración Documento")]
    public TipoNumeracionDocumento TipoNumeracionDocumento
    {
        get => _tipoNumeracionDocumento;
        set => SetPropertyValue(nameof(TipoNumeracionDocumento), ref _tipoNumeracionDocumento, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        TipoNumeracionDocumento = TipoNumeracionDocumento.PrefijoEjercicioNumero;
        PaddingNumero = 5;
        PaddingCuentaContable = 10;
    }

    public DateTime GetLocalTime()
    {
        var tz = ZonaHorariaPorDefecto?.GetTimeZoneInfo();
        return tz != null ? TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz) : DateTime.Now;
    }
}

public enum TipoNumeracionDocumento
{
    [XafDisplayName("Prefijo/Número")] PrefijoNumero,
    [XafDisplayName("Prefijo/Ejercicio/Número")] PrefijoEjercicioNumero,
    [XafDisplayName("Prefijo/Ejercicio/Mes/Número")] PrefijoEjercicioMesNumero
}