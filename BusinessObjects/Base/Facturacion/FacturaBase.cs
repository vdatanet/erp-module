using System.ComponentModel;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using DevExpress.Persistent.Validation;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.Helpers.Contactos;
using erp.Module.Services.Facturacion;
using erp.Module.Services.Ventas;
using erp.Module.Services.Ventas.StateMachines;

namespace erp.Module.BusinessObjects.Base.Facturacion;

[Appearance("BlockEditingWhenSent", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "EstadoFactura != 'Borrador' AND EstadoFactura != 'Validada'", Context = "DetailView", Enabled = false)]
[Appearance("BlockDeletionWhenSent", AppearanceItemType = "Action", TargetItems = "Delete",
    Criteria = "EstadoFactura != 'Borrador' AND EstadoFactura != 'Validada'", Context = "Any", Enabled = false)]
[Appearance("BlockSendActionOnlyWhenEmitida", AppearanceItemType = "Action", TargetItems = "Factura_EnviarVerifactu",
    Criteria = "EstadoFactura != 'Emitida' OR EstadoVeriFactu = 'AceptadaVeriFactu' OR EstadoVeriFactu = 'EnviadaVeriFactu' OR VeriFactuNoNecesario", Context = "Any", Enabled = false)]
public abstract class FacturaBase(Session session) : DocumentoVenta(session)
{

    private string? _uuid;
    private TipoFacturaAmigable _tipoFacturaAmigable;
    private TipoRectificativaAmigable _tipoRectificativaAmigable;
    private Diario? _diarioVentas;
    private string? _codigoErrorEntradaFactura;
    private bool _esSubsanacion;
    private string? _estadoEntradaFactura;
    private EstadoVeriFactu _estadoVeriFactu;
    private DomicilioDIR3? _domicilioDIR;
    private Asiento? _asientoContable;
    private MediaDataObject? _qr;
    private Guid _correlationId;
    private string? _texto;
    private TipoFactura _tipoFactura;
    private TipoRectificativa _tipoRectificativa;
    private string? _urlValidacion;
    private EstadoFactura _estadoFactura;


    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("UUID")]
    public string? Uuid
    {
        get => _uuid;
        set => SetPropertyValue(nameof(Uuid), ref _uuid, value);
    }

    [XafDisplayName("Diario Ventas")]
    [DataSourceCriteria("EstaActivo = True")]
    public Diario? DiarioVentas
    {
        get => _diarioVentas;
        set => SetPropertyValue(nameof(DiarioVentas), ref _diarioVentas, value);
    }

    [XafDisplayName("Domicilio DIR3 (e-Factura)")]
    [DataSourceProperty("Cliente.DireccionesDIR3")]
    [ImmediatePostData]
    public DomicilioDIR3? DomicilioDIR
    {
        get => _domicilioDIR;
        set
        {
            var modified = SetPropertyValue(nameof(DomicilioDIR), ref _domicilioDIR, value);
            if (modified && !IsLoading && !IsSaving) AsignarDomicilio(value);
        }
    }

    [XafDisplayName("Asiento Contable")]
    [ModelDefault("AllowEdit", "False")]
    public Asiento? AsientoContable
    {
        get => _asientoContable;
        set => SetPropertyValue(nameof(AsientoContable), ref _asientoContable, value);
    }

    [XafDisplayName("Apuntes Contables")]
    public IEnumerable<Apunte> ApuntesContables => AsientoContable?.Apuntes ?? Enumerable.Empty<Apunte>();

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("Estado VeriFactu")]
    [ImmediatePostData]
    public EstadoVeriFactu EstadoVeriFactu
    {
        get => _estadoVeriFactu;
        set => SetPropertyValue(nameof(EstadoVeriFactu), ref _estadoVeriFactu, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("Estado Entrada Factura")]
    public string? EstadoEntradaFactura
    {
        get => _estadoEntradaFactura;
        set => SetPropertyValue(nameof(EstadoEntradaFactura), ref _estadoEntradaFactura, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("Código Error Entrada Factura")]
    public string? CodigoErrorEntradaFactura
    {
        get => _codigoErrorEntradaFactura;
        set => SetPropertyValue(nameof(CodigoErrorEntradaFactura), ref _codigoErrorEntradaFactura, value);
    }

    [NonCloneable]
    [Browsable(false)]
    public TipoFactura TipoFactura
    {
        get => _tipoFactura;
        set => SetPropertyValue(nameof(TipoFactura), ref _tipoFactura, value);
    }

    [XafDisplayName("Tipo Factura")]
    [VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
    [ImmediatePostData]
    public TipoFacturaAmigable TipoFacturaAmigable
    {
        get => _tipoFacturaAmigable;
        set
        {
            var modified = SetPropertyValue(nameof(TipoFacturaAmigable), ref _tipoFacturaAmigable, value);
            if (modified && !IsLoading && !IsSaving)
            {
                if (Enum.TryParse<TipoFactura>(value.ToString(), out var result))
                {
                    TipoFactura = result;
                }
            }
        }
    }

    [NonCloneable]
    [Browsable(false)]
    public TipoRectificativa TipoRectificativa
    {
        get => _tipoRectificativa;
        set => SetPropertyValue(nameof(TipoRectificativa), ref _tipoRectificativa, value);
    }

    [XafDisplayName("Tipo Rectificativa")]
    [VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(true)]
    [ImmediatePostData]
    public TipoRectificativaAmigable TipoRectificativaAmigable
    {
        get => _tipoRectificativaAmigable;
        set
        {
            var modified = SetPropertyValue(nameof(TipoRectificativaAmigable), ref _tipoRectificativaAmigable, value);
            if (modified && !IsLoading && !IsSaving)
            {
                if (Enum.TryParse<TipoRectificativa>(value.ToString(), out var result))
                {
                    TipoRectificativa = result;
                }
            }
        }
    }

    [NonCloneable]
    [XafDisplayName("Es Subsanación")]
    public bool EsSubsanacion
    {
        get => _esSubsanacion;
        set => SetPropertyValue(nameof(EsSubsanacion), ref _esSubsanacion, value);
    }

    [Size(500)]
    [XafDisplayName("Texto")]
    public string? Texto
    {
        get => _texto;
        set => SetPropertyValue(nameof(Texto), ref _texto, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("ID de Correlación")]
    public Guid CorrelationId
    {
        get => _correlationId;
        set => SetPropertyValue(nameof(CorrelationId), ref _correlationId, value);
    }

    [Size(255)]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("URL Validación")]
    public string? UrlValidacion
    {
        get => _urlValidacion;
        set => SetPropertyValue(nameof(UrlValidacion), ref _urlValidacion, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("QR")]
    public MediaDataObject? Qr
    {
        get => _qr;
        set => SetPropertyValue(nameof(Qr), ref _qr, value);
    }

    public override bool GetAsignarNumeroAlGuardar()
    {
        return false;
    }

    [XafDisplayName("Estado")]
    [ModelDefault("AllowEdit", "False")]
    [ImmediatePostData]
    public EstadoFactura EstadoFactura
    {
        get => _estadoFactura;
        set => SetPropertyValue(nameof(EstadoFactura), ref _estadoFactura, value);
    }

    [XafDisplayName("Borrador")]
    public bool Borrador => EstadoFactura == EstadoFactura.Borrador;

    [XafDisplayName("Confirmado")]
    public bool Confirmado => EstadoFactura >= EstadoFactura.Validada;

    [XafDisplayName("Emitido")] public bool Emitido => EstadoFactura >= EstadoFactura.Emitida;

    [XafDisplayName("VeriFactu No Necesario")]
    public bool VeriFactuNoNecesario => EstadoFactura == EstadoFactura.VeriFactuNoNecesario;

    [XafDisplayName("Impreso")]
    public bool Impreso => false;

    [XafDisplayName("Anulado")]
    public bool Anulado => false;

    [XafDisplayName("Bloqueado")]
    public bool Bloqueado => EstadoFactura == EstadoFactura.Contabilizada;

    [XafDisplayName("Sincronizado")]
    public bool Sincronizado => EstadoVeriFactu == EstadoVeriFactu.AceptadaVeriFactu;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        EstadoFactura = EstadoFactura.Borrador;
        TipoFacturaAmigable = TipoFacturaAmigable.F1;
        TipoRectificativaAmigable = TipoRectificativaAmigable.I;
        
        // Asegurar que los campos técnicos también se inicialicen
        if (Enum.TryParse<TipoFactura>(TipoFacturaAmigable.F1.ToString(), out var tf))
        {
            TipoFactura = tf;
        }
        if (Enum.TryParse<TipoRectificativa>(TipoRectificativaAmigable.I.ToString(), out var tr))
        {
            TipoRectificativa = tr;
        }
        
        InitValues();
    }

    [XafDisplayName("Estado del Documento")]
    public IFacturaStateMachine StateMachine => GetStateMachine();

    protected abstract IFacturaStateMachine GetStateMachine();

    public bool Contabilizada => EstadoFactura == EstadoFactura.Contabilizada;
    public bool EnviadaVeriFactu => EstadoFactura == EstadoFactura.Enviada || 
                                    EstadoFactura == EstadoFactura.VeriFactuNoNecesario || 
                                    EstadoVeriFactu == EstadoVeriFactu.AceptadaVeriFactu || 
                                    EstadoVeriFactu == EstadoVeriFactu.EnviadaVeriFactu ||
                                    EstadoVeriFactu == EstadoVeriFactu.NoNecesario;
    public bool Emitida => EstadoFactura >= EstadoFactura.Emitida;
    public bool Cobrada => EstadoCobro == EstadoCobroFactura.Pagada;

    private void InitValues()
    {
        // EstadoVeriFactu = EstadoVeriFactu.Borrador;
        EsSubsanacion = false;
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        Texto ??= companyInfo.TextoDefectoVeriFactu;
        
        if (!companyInfo.ActivarVeriFactu)
        {
            EstadoVeriFactu = EstadoVeriFactu.NoNecesario;
        }
    }

    public abstract bool EsValida();

    public bool PuedeValidar => EstadoFactura == EstadoFactura.Borrador;
    public bool PuedeEmitir => EstadoFactura == EstadoFactura.Validada;
    public bool PuedeRevertirABorrador => EstadoFactura == EstadoFactura.Validada;
    public bool PuedeEnviarVerifactu => EstadoFactura == EstadoFactura.Emitida && 
                                        EstadoVeriFactu != EstadoVeriFactu.AceptadaVeriFactu && 
                                        EstadoVeriFactu != EstadoVeriFactu.EnviadaVeriFactu;
    public bool PuedeContabilizar => (EstadoFactura == EstadoFactura.Enviada || 
                                      EstadoFactura == EstadoFactura.VeriFactuNoNecesario ||
                                      EstadoVeriFactu == EstadoVeriFactu.AceptadaVeriFactu || 
                                      EstadoVeriFactu == EstadoVeriFactu.EnviadaVeriFactu ||
                                      EstadoVeriFactu == EstadoVeriFactu.PendienteVeriFactu) &&
                                     EstadoFactura != EstadoFactura.Contabilizada;

    public virtual ValidationResult ValidarParaEmision()
    {
        var result = ValidationResult.Success();
        if (EstadoFactura == EstadoFactura.Contabilizada)
            result.AddError("La factura ya ha sido contabilizada y no puede modificarse ni re-enviarse.");
        if (string.IsNullOrEmpty(Texto))
            result.AddError("El texto de la factura es obligatorio para VeriFactu.");
        if (Impuestos.Count == 0)
            result.AddError("La factura debe tener al menos un impuesto.");
        return result;
    }
    
    public void Validar()
    {
        var orchestrator = new FacturaOrchestrator();
        orchestrator.Validar(this);
    }

    public void Emitir()
    {
        var orchestrator = new FacturaOrchestrator();
        orchestrator.Emitir(this);
    }

    public async Task<VeriFactuService.SendResult> EnviarVerifactuAsync(IObjectSpace objectSpace, VeriFactuService veriFactuService)
    {
        var orchestrator = new FacturaOrchestrator();
        return await orchestrator.EnviarAVerifactuAsync(objectSpace, this, veriFactuService);
    }

    public void Contabilizar()
    {
        var orchestrator = new FacturaOrchestrator();
        orchestrator.Contabilizar(this);
    }

    public void RevertirABorrador()
    {
        var orchestrator = new FacturaOrchestrator();
        orchestrator.RevertirABorrador(this);
    }
}