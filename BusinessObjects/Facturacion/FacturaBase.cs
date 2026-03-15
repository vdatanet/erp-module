using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Crm;
using erp.Module.Helpers.Contactos;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Facturacion;

[Appearance("BlockEditingWhenSent", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "EstadoVeriFactu = 'Enviado'", Context = "Any", Enabled = false)]
[Appearance("BlockDeletionWhenSent", AppearanceItemType = "Action", TargetItems = "Delete",
    Criteria = "EstadoVeriFactu = 'Enviado'", Context = "Any", Enabled = false)]
[Appearance("BlockSendActionWhenSent", AppearanceItemType = "Action", TargetItems = "ValidateFactura",
    Criteria = "EstadoVeriFactu = 'Enviado'", Context = "Any", Enabled = false)]
public abstract class FacturaBase(Session session) : DocumentoVenta(session)
{
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
    
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("Estado VeriFactu")]
    public ValoresEstadoVeriFactu EstadoVeriFactu
    {
        get => _estadoVeriFactu;
        set => SetPropertyValue(nameof(EstadoVeriFactu), ref _estadoVeriFactu, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("Estado Entrada Factura")]
    public string EstadoEntradaFactura
    {
        get => _estadoEntradaFactura;
        set => SetPropertyValue(nameof(EstadoEntradaFactura), ref _estadoEntradaFactura, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("Código Error Entrada Factura")]
    public string CodigoErrorEntradaFactura
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
    public string Texto
    {
        get => _texto;
        set => SetPropertyValue(nameof(Texto), ref _texto, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("Respuesta Agencia Tributaria")]
    public string RespuestaAgenciaTributaria
    {
        get => _respuestaAgenciaTributaria;
        set => SetPropertyValue(nameof(RespuestaAgenciaTributaria), ref _respuestaAgenciaTributaria, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("XML Agencia Tributaria")]
    public string XmlAgenciaTributaria
    {
        get => _xmlAgenciaTributaria;
        set => SetPropertyValue(nameof(XmlAgenciaTributaria), ref _xmlAgenciaTributaria, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("CSV")]
    public string Csv
    {
        get => _csv;
        set => SetPropertyValue(nameof(Csv), ref _csv, value);
    }

    [Size(255)]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("URL Validación")]
    public string UrlValidacion
    {
        get => _urlValidacion;
        set => SetPropertyValue(nameof(UrlValidacion), ref _urlValidacion, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    [XafDisplayName("QR")]
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
    
    public override bool GetAsignarNumeroAlGuardar() => false;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        EstadoVeriFactu = ValoresEstadoVeriFactu.Borrador;
        TipoFactura = (TipoFactura)1; // F1
        TipoRectificativa = (TipoRectificativa)1; // I
        EsSubsanacion = false;
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        Texto ??= companyInfo?.TextoDefectoVeriFactu;
    }

    
    public abstract bool EsValida();
}
