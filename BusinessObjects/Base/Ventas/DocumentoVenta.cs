using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Documentos;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Inventario;
using erp.Module.BusinessObjects.Logistica;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.BusinessObjects.Tpv;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.Factories;
using erp.Module.Helpers.Contactos;
using erp.Module.Models.Ventas;
using erp.Module.Services.Ventas;
using erp.Module.Services.Ventas.StateMachines;

namespace erp.Module.BusinessObjects.Base.Ventas;

public abstract class DocumentoVenta(Session session) : EntidadBase(session)
{
    private bool _aCuenta;
    private Almacen? _almacen;
    private decimal _anticipo;
    private decimal _baseExenta;
    private decimal _baseImponible;
    private decimal _baseNoSujeta;
    private decimal _baseSujeta;
    private decimal _cambio;
    private CategoriaVenta? _categoriaVenta;
    private Cliente? _cliente;
    private bool _cobrado;
    private string? _codigoExterno;
    private string? _codigoPostalCliente;
    private Contacto? _comercial;
    private CondicionPago? _condicionPago;
    private MedioPago? _medioPago;
    private CuentaContable? _cuentaBanco;
    private CuentaContable? _cuentaCaja;

    // --- DATOS ECONÓMICOS ---
    private decimal _descuento;
    private bool _devuelto;
    private string? _direccionCliente;

    // --- ENTREGA / LOGÍSTICA ---
    private string? _direccionEntrega;
    private string? _documentoIdentificacionCliente;
    private DocumentoVenta? _documentoOrigen;
    private DocumentoVenta? _documentoRectificado;
    private DocumentoVenta? _documentoRelacionado;

    private IDocumentoVentaService? _documentoVentaService;
    private Ejercicio? _ejercicio;
    private Ejercicio? _ejercicioContable;
    private Ejercicio? _ejercicioFiscal;
    private string? _emailCliente;
    private bool _entregado;
    private EquipoVenta? _equipoVenta;
    private bool _esFactura;

    // --- FISCAL / FACTURACIÓN ---
    private bool _esFacturaSimplificada;
    private bool _esOferta;
    private bool _esPedido;
    private bool _esPresupuesto;
    private EstadoDocumentoVenta _estado;
    private EstadoCobroFactura _estadoCobro;
    private bool _esTicket;
    private DateTime _fecha;
    private DateTime? _fechaAnulacion;
    private DateTime? _fechaCobro;

    // --- ESTADO Y CONTROL ---
    private DateTime? _fechaConfirmacion;
    private DateTime _fechaCreacion;
    private DateTime? _fechaEmision;
    private DateTime? _fechaEntregaPrevista;
    private DateTime? _fechaEntregaReal;
    private DateTime? _fechaImpresion;
    private DateTime _fechaModificacion;

    // --- PAGO ---
    private CondicionPago? _formaPago;
    private decimal _gastosEnvio;

    // --- IDENTIFICACIÓN ---
    private TimeSpan _hora;
    private string? _huellaFiscal;
    private decimal _importeImpuestos;
    private decimal _importeIva;
    private decimal _importeIvaRepercutido;
    private decimal _importeIvaSoportado;
    private decimal _importePagado;
    private decimal _importePendiente;
    private decimal _importeRetencion;
    private decimal _importeTotal;
    private bool _impuestoIncluido;
    private MetodoCobroVenta _metodoCobro;
    private MetodoEntrega? _metodoEntrega;
    private string? _motivoAnulacion;

    // --- CLIENTE / DESTINATARIO ---
    private string? _nombreCliente;
    private string? _notas;
    private string? _notasInternas;
    private int _numero;
    private string? _numeroFiscal;
    private string? _numeroSeguimiento;

    // --- TRAZABILIDAD ---
    private string? _observaciones;
    private OrigenDocumentoVenta _origen;
    private decimal _otrosGastos;
    private Pais? _paisCliente;
    private string? _poblacionCliente;

    // --- IMPUESTOS Y CÁLCULOS ---
    private decimal _porcentajeDescuento;
    private decimal _porcentajeRecargo;
    private string? _provinciaCliente;
    private string? _qrVeriFactu;
    private decimal _recargoEquivalencia;
    private string? _recibidoPor;
    private decimal _redondeo;
    private string? _referenciaExterna;
    private string? _referenciaPago;
    private string? _secuencia;
    private string? _serie;
    private string? _serieFiscal;
    private SesionTpv? _sesionTpv;

    private IDocumentoVentaStateMachine? _stateMachine;
    private string? _telefonoCliente;
    private TipoDocumentoVenta _tipoDocumento;
    private decimal _totalBruto;

    private TotalesDocumento? _totalesCache;
    private decimal _totalNeto;

    // --- RELACIÓN OPERATIVA ---
    private Tpv.Tpv? _tpv;
    private Transportista? _transportista;
    private ApplicationUser? _usuario;
    private ApplicationUser? _usuarioCreacion;
    private ApplicationUser? _usuarioModificacion;
    private Contacto? _vendedor;

    [Browsable(false)] public IDocumentoVentaStateMachine StateMachine => _stateMachine ??= CreateStateMachine();

    [RuleRequiredField("erp.Module.BusinessObjects.Ventas.FacturaVenta.Cliente_Required", DefaultContexts.Save,
        TargetCriteria =
            "IsInstanceOfType(this, 'erp.Module.BusinessObjects.Ventas.FacturaVenta') or IsInstanceOfType(this, 'erp.Module.BusinessObjects.Ventas.OfertaVenta') or IsInstanceOfType(this, 'erp.Module.BusinessObjects.Ventas.AlbaranVenta')",
        CustomMessageTemplate = "El Cliente del documento de venta es obligatorio")]
    [Association("Cliente-DocumentosVenta")]
    [XafDisplayName("Cliente")]
    [DataSourceCriteria("Activo = true and IsIPuedeParticiparEnVentas")]
    [ImmediatePostData]
    public Cliente? Cliente
    {
        get => _cliente;
        set
        {
            var modified = SetPropertyValue(nameof(Cliente), ref _cliente, value);
            if (modified && !IsLoading && !IsSaving) AsignarCliente(value);
        }
    }

    [Size(100)]
    [XafDisplayName("Nombre Cliente")]
    [ModelDefault("AllowEdit", "False")]
    public string? NombreCliente
    {
        get => _nombreCliente;
        set => SetPropertyValue(nameof(NombreCliente), ref _nombreCliente, value);
    }

    [Size(20)]
    [XafDisplayName("NIF Cliente")]
    [ModelDefault("AllowEdit", "False")]
    public string? DocumentoIdentificacionCliente
    {
        get => _documentoIdentificacionCliente;
        set => SetPropertyValue(nameof(DocumentoIdentificacionCliente), ref _documentoIdentificacionCliente, value);
    }

    [Size(255)]
    [XafDisplayName("Email Cliente")]
    [ModelDefault("AllowEdit", "False")]
    public string? EmailCliente
    {
        get => _emailCliente;
        set => SetPropertyValue(nameof(EmailCliente), ref _emailCliente, value);
    }

    [Size(20)]
    [XafDisplayName("Teléfono Cliente")]
    [ModelDefault("AllowEdit", "False")]
    public string? TelefonoCliente
    {
        get => _telefonoCliente;
        set => SetPropertyValue(nameof(TelefonoCliente), ref _telefonoCliente, value);
    }

    [Size(255)]
    [XafDisplayName("Dirección Cliente")]
    [ModelDefault("AllowEdit", "False")]
    public string? DireccionCliente
    {
        get => _direccionCliente;
        set => SetPropertyValue(nameof(DireccionCliente), ref _direccionCliente, value);
    }

    [Size(100)]
    [XafDisplayName("Población Cliente")]
    [ModelDefault("AllowEdit", "False")]
    public string? PoblacionCliente
    {
        get => _poblacionCliente;
        set => SetPropertyValue(nameof(PoblacionCliente), ref _poblacionCliente, value);
    }

    [Size(100)]
    [XafDisplayName("Provincia Cliente")]
    [ModelDefault("AllowEdit", "False")]
    public string? ProvinciaCliente
    {
        get => _provinciaCliente;
        set => SetPropertyValue(nameof(ProvinciaCliente), ref _provinciaCliente, value);
    }

    [Size(10)]
    [XafDisplayName("CP Cliente")]
    [ModelDefault("AllowEdit", "False")]
    public string? CodigoPostalCliente
    {
        get => _codigoPostalCliente;
        set => SetPropertyValue(nameof(CodigoPostalCliente), ref _codigoPostalCliente, value);
    }

    [XafDisplayName("País Cliente")]
    [ModelDefault("AllowEdit", "False")]
    public Pais? PaisCliente
    {
        get => _paisCliente;
        set => SetPropertyValue(nameof(PaisCliente), ref _paisCliente, value);
    }

    [XafDisplayName("Serie")]
    [RuleRequiredField("RuleRequiredField_DocumentoVenta_Serie", DefaultContexts.Save,
        CustomMessageTemplate = "La Serie del documento de venta es obligatoria")]
    [Appearance("BlockSerieWhenNumeroIsSet", Enabled = false,
        Criteria = "!IsNewObject(this) and !IsNullOrEmpty(Secuencia)", Context = "Any")]
    public string? Serie
    {
        get => _serie;
        set => SetPropertyValue(nameof(Serie), ref _serie, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Número")]
    public int Numero
    {
        get => _numero;
        set => SetPropertyValue(nameof(Numero), ref _numero, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Secuencia")]
    public string? Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    [XafDisplayName("Fecha")]
    [ImmediatePostData]
    public DateTime Fecha
    {
        get => _fecha;
        set
        {
            var modified = SetPropertyValue(nameof(Fecha), ref _fecha, value);
            if (modified && !IsLoading && !IsSaving) OnChanged(nameof(IsFechaNoBloqueada));
        }
    }

    [XafDisplayName("Hora")]
    public TimeSpan Hora
    {
        get => _hora;
        set => SetPropertyValue(nameof(Hora), ref _hora, value);
    }

    [XafDisplayName("Ejercicio")]
    [RuleRequiredField("RuleRequiredField_DocumentoVenta_Ejercicio", DefaultContexts.Save)]
    [ImmediatePostData]
    public Ejercicio? Ejercicio
    {
        get => _ejercicio;
        set
        {
            var modified = SetPropertyValue(nameof(Ejercicio), ref _ejercicio, value);
            if (modified && !IsLoading && !IsSaving) OnChanged(nameof(IsFechaNoBloqueada));
        }
    }

    [XafDisplayName("Ejercicio Fiscal")]
    public Ejercicio? EjercicioFiscal
    {
        get => _ejercicioFiscal;
        set => SetPropertyValue(nameof(EjercicioFiscal), ref _ejercicioFiscal, value);
    }

    [XafDisplayName("Ejercicio Contable")]
    public Ejercicio? EjercicioContable
    {
        get => _ejercicioContable;
        set => SetPropertyValue(nameof(EjercicioContable), ref _ejercicioContable, value);
    }

    [XafDisplayName("Origen")]
    public OrigenDocumentoVenta Origen
    {
        get => _origen;
        set => SetPropertyValue(nameof(Origen), ref _origen, value);
    }

    [XafDisplayName("Tipo Documento")]
    public TipoDocumentoVenta TipoDocumento
    {
        get => _tipoDocumento;
        set => SetPropertyValue(nameof(TipoDocumento), ref _tipoDocumento, value);
    }

    [XafDisplayName("Estado")]
    public EstadoDocumentoVenta Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [XafDisplayName("Estado Cobro")]
    [ModelDefault("AllowEdit", "False")]
    public EstadoCobroFactura EstadoCobro
    {
        get => _estadoCobro;
        set => SetPropertyValue(nameof(EstadoCobro), ref _estadoCobro, value);
    }

    [XafDisplayName("TPV")]
    [Association("Tpv-DocumentosVenta")]
    public Tpv.Tpv? Tpv
    {
        get => _tpv;
        set => SetPropertyValue(nameof(Tpv), ref _tpv, value);
    }

    [XafDisplayName("Sesión TPV")]
    [Association("SesionTpv-DocumentosVenta")]
    public SesionTpv? SesionTpv
    {
        get => _sesionTpv;
        set => SetPropertyValue(nameof(SesionTpv), ref _sesionTpv, value);
    }

    [XafDisplayName("Usuario")]
    public ApplicationUser? Usuario
    {
        get => _usuario;
        set => SetPropertyValue(nameof(Usuario), ref _usuario, value);
    }

    [XafDisplayName("Comercial")]
    [DataSourceCriteria("Activo = true")]
    public Contacto? Comercial
    {
        get => _comercial;
        set => SetPropertyValue(nameof(Comercial), ref _comercial, value);
    }

    [XafDisplayName("Almacén")]
    [DataSourceCriteria("EstaActivo = True")]
    public Almacen? Almacen
    {
        get => _almacen;
        set => SetPropertyValue(nameof(Almacen), ref _almacen, value);
    }

    [Browsable(false)]
    [RuleFromBoolProperty("DocumentoVenta_FechaNoBloqueada", DefaultContexts.Save,
        "La fecha del documento se encuentra en un periodo bloqueado del ejercicio o el ejercicio está bloqueado.",
        UsedProperties = nameof(Fecha))]
    public bool IsFechaNoBloqueada
    {
        get
        {
            if (Ejercicio == null) return true;
            if (Ejercicio.Estado == EstadoEjercicio.Bloqueado) return false;

            return !Ejercicio.PeriodosBloqueados.Any(p => Fecha >= p.FechaInicio && Fecha <= p.FechaFin);
        }
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Base Imponible")]
    public decimal BaseImponible
    {
        get => _baseImponible;
        set => SetPropertyValue(nameof(BaseImponible), ref _baseImponible, value);
    }

    [XafDisplayName("Descuento")]
    public decimal Descuento
    {
        get => _descuento;
        set => SetPropertyValue(nameof(Descuento), ref _descuento, value);
    }

    [XafDisplayName("Recargo Equivalencia")]
    public decimal RecargoEquivalencia
    {
        get => _recargoEquivalencia;
        set => SetPropertyValue(nameof(RecargoEquivalencia), ref _recargoEquivalencia, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Impuestos")]
    public decimal ImporteImpuestos
    {
        get => _importeImpuestos;
        set => SetPropertyValue(nameof(ImporteImpuestos), ref _importeImpuestos, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Total")]
    public decimal ImporteTotal
    {
        get => _importeTotal;
        set => SetPropertyValue(nameof(ImporteTotal), ref _importeTotal, value);
    }

    [XafDisplayName("Importe Pagado")]
    public decimal ImportePagado
    {
        get => _importePagado;
        set => SetPropertyValue(nameof(ImportePagado), ref _importePagado, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Importe Pendiente")]
    public decimal ImportePendiente
    {
        get => _importePendiente;
        set => SetPropertyValue(nameof(ImportePendiente), ref _importePendiente, value);
    }

    [XafDisplayName("Cambio")]
    public decimal Cambio
    {
        get => _cambio;
        set => SetPropertyValue(nameof(Cambio), ref _cambio, value);
    }

    [XafDisplayName("Redondeo")]
    public decimal Redondeo
    {
        get => _redondeo;
        set => SetPropertyValue(nameof(Redondeo), ref _redondeo, value);
    }

    [XafDisplayName("Gastos Envío")]
    public decimal GastosEnvio
    {
        get => _gastosEnvio;
        set => SetPropertyValue(nameof(GastosEnvio), ref _gastosEnvio, value);
    }

    [XafDisplayName("Otros Gastos")]
    public decimal OtrosGastos
    {
        get => _otrosGastos;
        set => SetPropertyValue(nameof(OtrosGastos), ref _otrosGastos, value);
    }

    [XafDisplayName("Anticipo")]
    public decimal Anticipo
    {
        get => _anticipo;
        set => SetPropertyValue(nameof(Anticipo), ref _anticipo, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Total Bruto")]
    public decimal TotalBruto
    {
        get => _totalBruto;
        set => SetPropertyValue(nameof(TotalBruto), ref _totalBruto, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Total Neto")]
    public decimal TotalNeto
    {
        get => _totalNeto;
        set => SetPropertyValue(nameof(TotalNeto), ref _totalNeto, value);
    }

    [XafDisplayName("% Descuento")]
    public decimal PorcentajeDescuento
    {
        get => _porcentajeDescuento;
        set => SetPropertyValue(nameof(PorcentajeDescuento), ref _porcentajeDescuento, value);
    }

    [XafDisplayName("% Recargo")]
    public decimal PorcentajeRecargo
    {
        get => _porcentajeRecargo;
        set => SetPropertyValue(nameof(PorcentajeRecargo), ref _porcentajeRecargo, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Base Exenta")]
    public decimal BaseExenta
    {
        get => _baseExenta;
        set => SetPropertyValue(nameof(BaseExenta), ref _baseExenta, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Base No Sujeta")]
    public decimal BaseNoSujeta
    {
        get => _baseNoSujeta;
        set => SetPropertyValue(nameof(BaseNoSujeta), ref _baseNoSujeta, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Base Sujeta")]
    public decimal BaseSujeta
    {
        get => _baseSujeta;
        set => SetPropertyValue(nameof(BaseSujeta), ref _baseSujeta, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Importe IVA")]
    public decimal ImporteIva
    {
        get => _importeIva;
        set => SetPropertyValue(nameof(ImporteIva), ref _importeIva, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("IVA Repercutido")]
    public decimal ImporteIvaRepercutido
    {
        get => _importeIvaRepercutido;
        set => SetPropertyValue(nameof(ImporteIvaRepercutido), ref _importeIvaRepercutido, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("IVA Soportado")]
    public decimal ImporteIvaSoportado
    {
        get => _importeIvaSoportado;
        set => SetPropertyValue(nameof(ImporteIvaSoportado), ref _importeIvaSoportado, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Importe Retención")]
    public decimal ImporteRetencion
    {
        get => _importeRetencion;
        set => SetPropertyValue(nameof(ImporteRetencion), ref _importeRetencion, value);
    }

    [XafDisplayName("Impuesto Incluido")]
    public bool ImpuestoIncluido
    {
        get => _impuestoIncluido;
        set => SetPropertyValue(nameof(ImpuestoIncluido), ref _impuestoIncluido, value);
    }

    [Browsable(false)] public TotalesDocumento Totales => _totalesCache ??= DocumentoVentaService.CalcularTotales(this);

    [XafDisplayName("Forma de Pago")]
    public CondicionPago? FormaPago
    {
        get => _formaPago;
        set => SetPropertyValue(nameof(FormaPago), ref _formaPago, value);
    }

    [XafDisplayName("Método Cobro")]
    public MetodoCobroVenta MetodoCobro
    {
        get => _metodoCobro;
        set => SetPropertyValue(nameof(MetodoCobro), ref _metodoCobro, value);
    }

    [Size(100)]
    [XafDisplayName("Referencia Pago")]
    public string? ReferenciaPago
    {
        get => _referenciaPago;
        set => SetPropertyValue(nameof(ReferenciaPago), ref _referenciaPago, value);
    }

    [XafDisplayName("Fecha Cobro")]
    public DateTime? FechaCobro
    {
        get => _fechaCobro;
        set => SetPropertyValue(nameof(FechaCobro), ref _fechaCobro, value);
    }

    [XafDisplayName("Cobrado")]
    public bool Cobrado
    {
        get => _cobrado;
        set => SetPropertyValue(nameof(Cobrado), ref _cobrado, value);
    }

    [XafDisplayName("A Cuenta")]
    public bool ACuenta
    {
        get => _aCuenta;
        set => SetPropertyValue(nameof(ACuenta), ref _aCuenta, value);
    }

    [XafDisplayName("Devuelto")]
    public bool Devuelto
    {
        get => _devuelto;
        set => SetPropertyValue(nameof(Devuelto), ref _devuelto, value);
    }

    [XafDisplayName("Cuenta Caja")]
    public CuentaContable? CuentaCaja
    {
        get => _cuentaCaja;
        set => SetPropertyValue(nameof(CuentaCaja), ref _cuentaCaja, value);
    }

    [XafDisplayName("Cuenta Banco")]
    public CuentaContable? CuentaBanco
    {
        get => _cuentaBanco;
        set => SetPropertyValue(nameof(CuentaBanco), ref _cuentaBanco, value);
    }

    [NonCloneable]
    [XafDisplayName("Borrador")]
    public bool Borrador => Estado == EstadoDocumentoVenta.Borrador;

    [NonCloneable]
    [XafDisplayName("Confirmado")]
    public bool Confirmado => Estado == EstadoDocumentoVenta.Confirmado;

    [NonCloneable]
    [XafDisplayName("Emitido")]
    public bool Emitido => Estado == EstadoDocumentoVenta.Emitido;

    [NonCloneable]
    [XafDisplayName("Impreso")]
    public bool Impreso => Estado == EstadoDocumentoVenta.Impreso;

    [NonCloneable]
    [XafDisplayName("Anulado")]
    public bool Anulado => Estado == EstadoDocumentoVenta.Anulado;

    [NonCloneable]
    [XafDisplayName("Bloqueado")]
    public bool Bloqueado => Estado == EstadoDocumentoVenta.Bloqueado;

    [NonCloneable]
    [XafDisplayName("Sincronizado")]
    public bool Sincronizado => Estado == EstadoDocumentoVenta.Sincronizado;

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Fecha Confirmación")]
    public DateTime? FechaConfirmacion
    {
        get => _fechaConfirmacion;
        set => SetPropertyValue(nameof(FechaConfirmacion), ref _fechaConfirmacion, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Fecha Emisión")]
    public DateTime? FechaEmision
    {
        get => _fechaEmision;
        set => SetPropertyValue(nameof(FechaEmision), ref _fechaEmision, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Fecha Impresión")]
    public DateTime? FechaImpresion
    {
        get => _fechaImpresion;
        set => SetPropertyValue(nameof(FechaImpresion), ref _fechaImpresion, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Fecha Anulación")]
    public DateTime? FechaAnulacion
    {
        get => _fechaAnulacion;
        set => SetPropertyValue(nameof(FechaAnulacion), ref _fechaAnulacion, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Motivo Anulación")]
    public string? MotivoAnulacion
    {
        get => _motivoAnulacion;
        set => SetPropertyValue(nameof(MotivoAnulacion), ref _motivoAnulacion, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas Internas")]
    public string? NotasInternas
    {
        get => _notasInternas;
        set => SetPropertyValue(nameof(NotasInternas), ref _notasInternas, value);
    }

    [Size(100)]
    [XafDisplayName("Referencia Externa")]
    public string? ReferenciaExterna
    {
        get => _referenciaExterna;
        set => SetPropertyValue(nameof(ReferenciaExterna), ref _referenciaExterna, value);
    }

    [Size(100)]
    [XafDisplayName("Código Externo")]
    public string? CodigoExterno
    {
        get => _codigoExterno;
        set => SetPropertyValue(nameof(CodigoExterno), ref _codigoExterno, value);
    }

    [XafDisplayName("Documento Origen")]
    public DocumentoVenta? DocumentoOrigen
    {
        get => _documentoOrigen;
        set => SetPropertyValue(nameof(DocumentoOrigen), ref _documentoOrigen, value);
    }

    [XafDisplayName("Documento Relacionado")]
    public DocumentoVenta? DocumentoRelacionado
    {
        get => _documentoRelacionado;
        set => SetPropertyValue(nameof(DocumentoRelacionado), ref _documentoRelacionado, value);
    }

    [XafDisplayName("Documento Rectificado")]
    public DocumentoVenta? DocumentoRectificado
    {
        get => _documentoRectificado;
        set => SetPropertyValue(nameof(DocumentoRectificado), ref _documentoRectificado, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Usuario Creación")]
    public ApplicationUser? UsuarioCreacion
    {
        get => _usuarioCreacion;
        set => SetPropertyValue(nameof(UsuarioCreacion), ref _usuarioCreacion, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Usuario Modificación")]
    public ApplicationUser? UsuarioModificacion
    {
        get => _usuarioModificacion;
        set => SetPropertyValue(nameof(UsuarioModificacion), ref _usuarioModificacion, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Fecha Creación")]
    public DateTime FechaCreacion
    {
        get => _fechaCreacion;
        set => SetPropertyValue(nameof(FechaCreacion), ref _fechaCreacion, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Fecha Modificación")]
    public DateTime FechaModificacion
    {
        get => _fechaModificacion;
        set => SetPropertyValue(nameof(FechaModificacion), ref _fechaModificacion, value);
    }

    [Size(255)]
    [XafDisplayName("Dirección Entrega")]
    public string? DireccionEntrega
    {
        get => _direccionEntrega;
        set => SetPropertyValue(nameof(DireccionEntrega), ref _direccionEntrega, value);
    }

    [XafDisplayName("Fecha Entrega Prevista")]
    public DateTime? FechaEntregaPrevista
    {
        get => _fechaEntregaPrevista;
        set => SetPropertyValue(nameof(FechaEntregaPrevista), ref _fechaEntregaPrevista, value);
    }

    [XafDisplayName("Fecha Entrega Real")]
    public DateTime? FechaEntregaReal
    {
        get => _fechaEntregaReal;
        set => SetPropertyValue(nameof(FechaEntregaReal), ref _fechaEntregaReal, value);
    }

    [XafDisplayName("Método de Entrega")]
    [ImmediatePostData]
    public MetodoEntrega? MetodoEntrega
    {
        get => _metodoEntrega;
        set
        {
            var modified = SetPropertyValue(nameof(MetodoEntrega), ref _metodoEntrega, value);
            if (modified && !IsLoading && !IsSaving) AsignarMetodoEntrega(value);
        }
    }

    [XafDisplayName("Transportista")]
    public Transportista? Transportista
    {
        get => _transportista;
        set => SetPropertyValue(nameof(Transportista), ref _transportista, value);
    }

    [Size(100)]
    [XafDisplayName("Número Seguimiento")]
    public string? NumeroSeguimiento
    {
        get => _numeroSeguimiento;
        set => SetPropertyValue(nameof(NumeroSeguimiento), ref _numeroSeguimiento, value);
    }

    [XafDisplayName("Entregado")]
    public bool Entregado
    {
        get => _entregado;
        set => SetPropertyValue(nameof(Entregado), ref _entregado, value);
    }

    [Size(100)]
    [XafDisplayName("Recibido Por")]
    public string? RecibidoPor
    {
        get => _recibidoPor;
        set => SetPropertyValue(nameof(RecibidoPor), ref _recibidoPor, value);
    }

    [XafDisplayName("Es Factura Simplificada")]
    public bool EsFacturaSimplificada
    {
        get => _esFacturaSimplificada;
        set => SetPropertyValue(nameof(EsFacturaSimplificada), ref _esFacturaSimplificada, value);
    }

    [XafDisplayName("Es Factura")]
    public bool EsFactura
    {
        get => _esFactura;
        set => SetPropertyValue(nameof(EsFactura), ref _esFactura, value);
    }

    [XafDisplayName("Es Ticket")]
    public bool EsTicket
    {
        get => _esTicket;
        set => SetPropertyValue(nameof(EsTicket), ref _esTicket, value);
    }

    [XafDisplayName("Es Oferta")]
    public bool EsOferta
    {
        get => _esOferta;
        set => SetPropertyValue(nameof(EsOferta), ref _esOferta, value);
    }

    [XafDisplayName("Es Presupuesto")]
    public bool EsPresupuesto
    {
        get => _esPresupuesto;
        set => SetPropertyValue(nameof(EsPresupuesto), ref _esPresupuesto, value);
    }

    [XafDisplayName("Es Pedido")]
    public bool EsPedido
    {
        get => _esPedido;
        set => SetPropertyValue(nameof(EsPedido), ref _esPedido, value);
    }

    [Size(10)]
    [XafDisplayName("Serie Fiscal")]
    public string? SerieFiscal
    {
        get => _serieFiscal;
        set => SetPropertyValue(nameof(SerieFiscal), ref _serieFiscal, value);
    }

    [Size(20)]
    [XafDisplayName("Número Fiscal")]
    public string? NumeroFiscal
    {
        get => _numeroFiscal;
        set => SetPropertyValue(nameof(NumeroFiscal), ref _numeroFiscal, value);
    }

    [Size(255)]
    [XafDisplayName("Huella Fiscal")]
    public string? HuellaFiscal
    {
        get => _huellaFiscal;
        set => SetPropertyValue(nameof(HuellaFiscal), ref _huellaFiscal, value);
    }

    [Size(500)]
    [XafDisplayName("QR VeriFactu")]
    public string? QRVeriFactu
    {
        get => _qrVeriFactu;
        set => SetPropertyValue(nameof(QRVeriFactu), ref _qrVeriFactu, value);
    }

    [XafDisplayName("Equipo de Venta")]
    [Association("EquipoVenta-DocumentosVenta")]
    public EquipoVenta? EquipoVenta
    {
        get => _equipoVenta;
        set => SetPropertyValue(nameof(EquipoVenta), ref _equipoVenta, value);
    }

    [XafDisplayName("Vendedor")]
    [ToolTip("El vendedor puede ser un empleado o un agente externo (ambos son Contactos)")]
    [DataSourceCriteria("EsVendedor = true AND Activo = true")]
    [ImmediatePostData]
    public Contacto? Vendedor
    {
        get => _vendedor;
        set
        {
            var modified = SetPropertyValue(nameof(Vendedor), ref _vendedor, value);
            if (modified && !IsLoading && !IsSaving) AsignarVendedor(value);
        }
    }

    [XafDisplayName("Categoría de Venta")]
    public CategoriaVenta? CategoriaVenta
    {
        get => _categoriaVenta;
        set => SetPropertyValue(nameof(CategoriaVenta), ref _categoriaVenta, value);
    }

    [XafDisplayName("Condición de Pago")]
    public CondicionPago? CondicionPago
    {
        get => _condicionPago;
        set => SetPropertyValue(nameof(CondicionPago), ref _condicionPago, value);
    }

    [XafDisplayName("Medio de Pago")]
    public MedioPago? MedioPago
    {
        get => _medioPago;
        set => SetPropertyValue(nameof(MedioPago), ref _medioPago, value);
    }

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Grupos")]
    [XafDisplayName("Grupos")]
    public XPCollection<DocumentoVentaGrupo> Grupos => GetCollection<DocumentoVentaGrupo>();

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Lineas")]
    [XafDisplayName("Líneas")]
    public XPCollection<DocumentoVentaLinea> Lineas => GetCollection<DocumentoVentaLinea>();

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Impuestos")]
    [XafDisplayName("Impuestos")]
    public XPCollection<DocumentoVentaImpuesto> Impuestos => GetCollection<DocumentoVentaImpuesto>();

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Tareas")]
    [XafDisplayName("Tareas")]
    public XPCollection<Tarea> Tareas => GetCollection<Tarea>();

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Fotos")]
    [XafDisplayName("Imágenes")]
    public XPCollection<Imagen> Imagenes => GetCollection<Imagen>();

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Adjuntos")]
    [XafDisplayName("Adjuntos")]
    public XPCollection<Adjunto> Adjuntos => GetCollection<Adjunto>();

    [XafDisplayName("Movimientos de Caja")]
    public XPCollection<MovimientoCajaTpv> MovimientosCaja => GetCollection<MovimientoCajaTpv>();

    [XafDisplayName("Eventos")] public XPCollection<SesionTpvEvento> Eventos => GetCollection<SesionTpvEvento>();

    [XafDisplayName("Documentos Relacionados")]
    public XPCollection<DocumentoVenta> DocumentosRelacionados => GetCollection<DocumentoVenta>();

    protected IDocumentoVentaService DocumentoVentaService => _documentoVentaService ??= new DocumentoVentaService();

    protected abstract IDocumentoVentaStateMachine CreateStateMachine();

    /// <summary>
    ///     Regla de negocio: Al asignar un cliente se copian sus datos de contacto y facturación al documento.
    /// </summary>
    public virtual void AsignarCliente(Cliente? value)
    {
        if (value == null)
        {
            CondicionPago = null;
            MedioPago = null;
            NombreCliente = null;
            DocumentoIdentificacionCliente = null;
            EmailCliente = null;
            TelefonoCliente = null;
            DireccionCliente = null;
            PoblacionCliente = null;
            ProvinciaCliente = null;
            CodigoPostalCliente = null;
            PaisCliente = null;
            return;
        }

        CondicionPago = value.CondicionPago;
        MedioPago = value.MedioPago;

        NombreCliente = value.Nombre;
        DocumentoIdentificacionCliente = value.Nif;
        EmailCliente = value.CorreoElectronico;
        TelefonoCliente = value.Telefono;
        DireccionCliente = value.Direccion;
        PoblacionCliente = value.Poblacion?.Nombre;
        ProvinciaCliente = value.Provincia?.Nombre;
        CodigoPostalCliente = value.CodigoPostal;
        PaisCliente = value.Pais;
    }

    public void InvalidadCacheTotales()
    {
        _totalesCache = null;
    }

    /// <summary>
    ///     Regla de negocio: Al asignar un método de entrega se establece el transportista por defecto
    ///     y se inicializan los gastos de envío si el documento es nuevo.
    /// </summary>
    public virtual void AsignarMetodoEntrega(MetodoEntrega? value)
    {
        if (value == null) return;

        Transportista = value.TransportistaPorDefecto;

        // Solo asignamos si no tiene valor o es 0 y es un objeto nuevo 
        // (para no sobreescribir intencionadamente 0 en edición)
        if (GastosEnvio == 0 && Session.IsNewObject(this)) GastosEnvio = value.CosteFijo;
    }

    /// <summary>
    ///     Regla de negocio: Al asignar un vendedor se establece su equipo de venta
    ///     y se sincronizan las comisiones de las líneas existentes.
    /// </summary>
    public virtual void AsignarVendedor(Contacto? value)
    {
        if (value == null) return;

        if (value.EquipoVenta != null)
            EquipoVenta = value.EquipoVenta;

        SincronizarComisionesLineas();
    }

    public void SincronizarComisionesLineas()
    {
        if (Vendedor == null) return;
        foreach (var linea in Lineas)
        {
            linea.PorcentajeComision = Vendedor.PorcentajeComision;
            linea.ImporteComisionFijo = Vendedor.ImporteComisionFijo;
        }
    }

    protected override XPCollection<T> CreateCollection<T>(XPMemberInfo property)
    {
        var collection = base.CreateCollection<T>(property);
        if (property.Name == nameof(Lineas)) collection.CollectionChanged += Lineas_CollectionChanged;
        return collection;
    }

    private void Lineas_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
    {
        if (IsLoading || IsSaving || IsDeleted) return;
        InvalidadCacheTotales();
        RecalcularTotales();
    }

    public void RecalcularTotales()
    {
        DocumentoVentaService.RecalcularTotales(this);
    }

    [Obsolete("Use RecalcularTotales() through DocumentoVentaService")]
    public void BorrarResumenImpuestos()
    {
        for (var i = Impuestos.Count - 1; i >= 0; i--)
            Impuestos[i].Delete();
    }

    [Obsolete("Use RecalcularTotales() through DocumentoVentaService")]
    public void ReconstruirResumenImpuestos()
    {
        RecalcularTotales();
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitInformacionTemporal();
        InitEstadoYControl();
        InitAuditoria();
    }

    private void InitInformacionTemporal()
    {
        var localTime = InformacionEmpresaHelper.GetLocalTime(Session);
        Fecha = localTime.Date;
        Hora = localTime.TimeOfDay;

        Ejercicio ??= Session.Query<Ejercicio>().FirstOrDefault(e =>
            e.Estado == EstadoEjercicio.Abierto && e.FechaInicio <= Fecha && e.FechaFin >= Fecha);
        EjercicioFiscal ??= Ejercicio;
        EjercicioContable ??= Ejercicio;
    }

    private void InitEstadoYControl()
    {
        Origen = OrigenDocumentoVenta.Manual;
        Estado = EstadoDocumentoVenta.Borrador;
    }

    private void InitAuditoria()
    {
        FechaCreacion = InformacionEmpresaHelper.GetLocalTime(Session);
        try
        {
            UsuarioCreacion = Session.GetObjectByKey<ApplicationUser>(SecuritySystem.CurrentUserId);
        }
        catch (InvalidOperationException)
        {
            // Contexto de seguridad no disponible (ej. desde API fuera de sesión Blazor)
        }
    }

    public virtual bool GetAsignarNumeroAlGuardar()
    {
        return true;
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        ActualizarAuditoriaModificacion();
        ProcesarNumeracion();
    }

    private void ActualizarAuditoriaModificacion()
    {
        FechaModificacion = InformacionEmpresaHelper.GetLocalTime(Session);
        try
        {
            UsuarioModificacion = Session.GetObjectByKey<ApplicationUser>(SecuritySystem.CurrentUserId);
        }
        catch (InvalidOperationException)
        {
            // Contexto de seguridad no disponible
        }
    }

    private void ProcesarNumeracion()
    {
        if (GetAsignarNumeroAlGuardar() && string.IsNullOrEmpty(Secuencia) && !string.IsNullOrEmpty(Serie))
            AsignarNumero();
    }

    public virtual void AsignarNumero()
    {
        if (!string.IsNullOrEmpty(Serie) && Ejercicio != null)
        {
            var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
            if (companyInfo == null) return;

            var padding = companyInfo.PaddingNumero;

            var sequenceName = companyInfo.TipoNumeracionDocumento switch
            {
                TipoNumeracionDocumento.PrefijoNumero => $"{GetType().FullName}.{Serie}",
                TipoNumeracionDocumento.PrefijoEjercicioNumero => $"{GetType().FullName}.{Ejercicio.Anio}.{Serie}",
                TipoNumeracionDocumento.PrefijoEjercicioMesNumero =>
                    $"{GetType().FullName}.{Ejercicio.Anio}.{Fecha.Month:D2}.{Serie}",
                _ => $"{GetType().FullName}.{Ejercicio.Anio}.{Serie}"
            };

            var prefix = Serie;

            Numero = SequenceFactory.GetNextSequence(Session, sequenceName, out var formattedSequence,
                prefix, padding, companyInfo, Fecha);
            Secuencia = formattedSequence;
        }
    }
}