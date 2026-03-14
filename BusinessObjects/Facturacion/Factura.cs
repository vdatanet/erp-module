using System.ComponentModel;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.Factories;
using erp.Module.Helpers.Contactos;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Facturacion;

[DefaultClassOptions]
[NavigationItem("Facturacion")]
[ImageName("BO_Factura")]
[DefaultProperty(nameof(NumeroFactura))]
[Appearance("BlockEditingWhenSent", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "EstadoVeriFactu = 'Enviado'", Context = "Any", Enabled = false)]
[Appearance("BlockDeletionWhenSent", AppearanceItemType = "Action", TargetItems = "Delete",
    Criteria = "EstadoVeriFactu = 'Enviado'", Context = "Any", Enabled = false)]
[Appearance("BlockSendActionWhenSent", AppearanceItemType = "Action", TargetItems = "ValidateFactura",
    Criteria = "EstadoVeriFactu = 'Enviado'", Context = "Any", Enabled = false)]
public class Factura(Session session) : DocumentoVenta(session)
{
    private string _prefijoFactura;
    private string _numeroFactura;
    private DateTime _fechaFactura;
    private Cliente _cliente;
    private ValoresEstadoVeriFactu _estadoVeriFactu;
    private string _estadoEntradaFactura;
    private string _codigoErrorEntradaFactura;
    private TipoFactura _tipoFactura;
    private TipoRectificativa _tipoRectificativa;
    private bool _esSubsanacion;
    private string _texto;
    private string _respuestaAgenciaTributaria;
    private string _xmlAgenciaTributaria;
    private string _csv;
    private string _urlValidacion;
    private MediaDataObject _qr;

    [RuleRequiredField]
    public string PrefijoFactura
    {
        get => _prefijoFactura;
        set => SetPropertyValue(nameof(PrefijoFactura), ref _prefijoFactura, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    public string NumeroFactura
    {
        get => _numeroFactura;
        set => SetPropertyValue(nameof(NumeroFactura), ref _numeroFactura, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    public DateTime FechaFactura
    {
        get => _fechaFactura;
        set => SetPropertyValue(nameof(FechaFactura), ref _fechaFactura, value);
    }

    [RuleRequiredField]
    [Association("Customer-Invoices")]
    public Cliente Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public ValoresEstadoVeriFactu EstadoVeriFactu
    {
        get => _estadoVeriFactu;
        set => SetPropertyValue(nameof(EstadoVeriFactu), ref _estadoVeriFactu, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public string EstadoEntradaFactura
    {
        get => _estadoEntradaFactura;
        set => SetPropertyValue(nameof(EstadoEntradaFactura), ref _estadoEntradaFactura, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public string CodigoErrorEntradaFactura
    {
        get => _codigoErrorEntradaFactura;
        set => SetPropertyValue(nameof(CodigoErrorEntradaFactura), ref _codigoErrorEntradaFactura, value);
    }

    [NonCloneable]
    public TipoFactura TipoFactura
    {
        get => _tipoFactura;
        set => SetPropertyValue(nameof(TipoFactura), ref _tipoFactura, value);
    }

    [NonCloneable]
    public TipoRectificativa TipoRectificativa
    {
        get => _tipoRectificativa;
        set => SetPropertyValue(nameof(TipoRectificativa), ref _tipoRectificativa, value);
    }

    [NonCloneable]
    public bool EsSubsanacion
    {
        get => _esSubsanacion;
        set => SetPropertyValue(nameof(EsSubsanacion), ref _esSubsanacion, value);
    }
    
    [Size(500)]
    public string Texto
    {
        get => _texto;
        set => SetPropertyValue(nameof(Texto), ref _texto, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public string RespuestaAgenciaTributaria
    {
        get => _respuestaAgenciaTributaria;
        set => SetPropertyValue(nameof(RespuestaAgenciaTributaria), ref _respuestaAgenciaTributaria, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public string XmlAgenciaTributaria
    {
        get => _xmlAgenciaTributaria;
        set => SetPropertyValue(nameof(XmlAgenciaTributaria), ref _xmlAgenciaTributaria, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public string Csv
    {
        get => _csv;
        set => SetPropertyValue(nameof(Csv), ref _csv, value);
    }

    [Size(255)]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public string UrlValidacion
    {
        get => _urlValidacion;
        set => SetPropertyValue(nameof(UrlValidacion), ref _urlValidacion, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public MediaDataObject Qr
    {
        get => _qr;
        set => SetPropertyValue(nameof(Qr), ref _qr, value);
    }

    public enum ValoresEstadoVeriFactu
    {
        Borrador,
        Enviado
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        EstadoVeriFactu = ValoresEstadoVeriFactu.Borrador;
        TipoFactura = TipoFactura.F1;
        TipoRectificativa = TipoRectificativa.I;
        EsSubsanacion = false;
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        PrefijoFactura ??= companyInfo?.PrefijoFacturasVentaPorDefecto;
        Texto ??= companyInfo?.TextoDefectoVeriFactu;
    }

    public void ObtenerNumeroFactura()
    {
        NumeroFactura =
            SequenceFactory.GetNextSequence(Session, $"{typeof(Factura).FullName}.{PrefijoFactura}", PrefijoFactura, 5);
    }
    
    public bool EsValida()
    {
        return EstadoVeriFactu != ValoresEstadoVeriFactu.Enviado
               && Cliente != null
               && !string.IsNullOrEmpty(Cliente.Nombre)
               && !string.IsNullOrEmpty(Cliente.Nif)
               && !string.IsNullOrEmpty(Texto)
               && Impuestos.Count > 0;
    }
}