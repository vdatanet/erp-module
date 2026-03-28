using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.Helpers.Contactos;
using erp.Module.Services.Facturacion;
using erp.Module.Services.Ventas;
using erp.Module.Services.Ventas.StateMachines;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Base.Facturacion;

[Appearance("BlockEditingWhenSent", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "EstadoFactura = 'EnviadaVerifactu' OR EstadoFactura = 2", Context = "Any", Enabled = false)]
[Appearance("BlockDeletionWhenSent", AppearanceItemType = "Action", TargetItems = "Delete",
    Criteria = "EstadoFactura = 'EnviadaVerifactu' OR EstadoFactura = 2", Context = "Any", Enabled = false)]
[Appearance("BlockSendActionWhenSent", AppearanceItemType = "Action", TargetItems = "Factura_EnviarVerifactu",
    Criteria = "EstadoFactura = 'EnviadaVerifactu' OR EstadoFactura = 2", Context = "Any", Enabled = false)]
public abstract class FacturaBase(Session session) : DocumentoVenta(session)
{

    private string? _codigoErrorEntradaFactura;
    private string? _csv;
    private bool _esSubsanacion;
    private string? _estadoEntradaFactura;
    private EstadoVeriFactu _estadoVeriFactu;
    private Domicilio? _domicilioDIR;
    private Asiento? _asientoContable;
    private MediaDataObject? _qr;
    private string? _respuestaAgenciaTributaria;
    private string? _texto;
    private TipoFactura _tipoFactura;
    private TipoRectificativa _tipoRectificativa;
    private string? _urlValidacion;
    private string? _xmlAgenciaTributaria;
    private EstadoFactura _estadoFactura;

    [XafDisplayName("Domicilio DIR (e-Factura)")]
    [DataSourceProperty("Cliente.DireccionesDIR")]
    [ImmediatePostData]
    public Domicilio? DomicilioDIR
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

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("Estado VeriFactu")]
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
    [XafDisplayName("Tipo Factura")]
    public TipoFactura TipoFactura
    {
        get => _tipoFactura;
        set => SetPropertyValue(nameof(TipoFactura), ref _tipoFactura, value);
    }

    [NonCloneable]
    [XafDisplayName("Tipo Rectificativa")]
    public TipoRectificativa TipoRectificativa
    {
        get => _tipoRectificativa;
        set => SetPropertyValue(nameof(TipoRectificativa), ref _tipoRectificativa, value);
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

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("Respuesta Agencia Tributaria")]
    public string? RespuestaAgenciaTributaria
    {
        get => _respuestaAgenciaTributaria;
        set => SetPropertyValue(nameof(RespuestaAgenciaTributaria), ref _respuestaAgenciaTributaria, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("XML Agencia Tributaria")]
    public string? XmlAgenciaTributaria
    {
        get => _xmlAgenciaTributaria;
        set => SetPropertyValue(nameof(XmlAgenciaTributaria), ref _xmlAgenciaTributaria, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("CSV")]
    public string? Csv
    {
        get => _csv;
        set => SetPropertyValue(nameof(Csv), ref _csv, value);
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
    public EstadoFactura EstadoFactura
    {
        get => _estadoFactura;
        set => SetPropertyValue(nameof(EstadoFactura), ref _estadoFactura, value);
    }

    [XafDisplayName("Borrador")]
    public bool Borrador => EstadoFactura == EstadoFactura.Borrador;

    [XafDisplayName("Confirmado")]
    public bool Confirmado => EstadoFactura >= EstadoFactura.Validada;

    [XafDisplayName("Emitido")]
    public bool Emitido => EstadoFactura >= EstadoFactura.Validada;

    [XafDisplayName("Impreso")]
    public bool Impreso => false;

    [XafDisplayName("Anulado")]
    public bool Anulado => false;

    [XafDisplayName("Bloqueado")]
    public bool Bloqueado => EstadoFactura == EstadoFactura.Contabilizada;

    [XafDisplayName("Sincronizado")]
    public bool Sincronizado => EstadoFactura == EstadoFactura.EnviadaVerifactu;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        EstadoFactura = EstadoFactura.Borrador;
        InitValues();
    }

    public IFacturaStateMachine StateMachine => GetStateMachine();

    protected abstract IFacturaStateMachine GetStateMachine();

    public bool Contabilizada => EstadoFactura == EstadoFactura.Contabilizada;
    public bool EnviadaVeriFactu => EstadoFactura == EstadoFactura.EnviadaVerifactu;
    public bool Cobrada => EstadoCobro == EstadoCobroFactura.Pagada;

    private void InitValues()
    {
        // EstadoVeriFactu = EstadoVeriFactu.Borrador;
        TipoFactura = (TipoFactura)1; // F1
        TipoRectificativa = (TipoRectificativa)1; // I
        EsSubsanacion = false;
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        Texto ??= companyInfo.TextoDefectoVeriFactu;
    }

    public abstract bool EsValida();

    public virtual ValidationResult ValidarParaEmision()
    {
        var result = ValidationResult.Success();
        if (EstadoFactura != EstadoFactura.Borrador && (int)EstadoFactura != 0)
            result.AddError("La factura debe estar en estado borrador para ser validada.");
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

    public VeriFactuService.SendResult EnviarVerifactu(IObjectSpace objectSpace, VeriFactuService veriFactuService)
    {
        var orchestrator = new FacturaOrchestrator();
        return orchestrator.EnviarAVerifactu(objectSpace, this, veriFactuService);
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