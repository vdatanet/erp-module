using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Configuraciones;

[DefaultClassOptions]
[NavigationItem("Configuraciones")]
[XafDisplayName("Configuración Webhook VeriFactu")]
[ImageName("Action_Cloud")]
public class VerifactuWebhookConfig(Session session) : EntidadBase(session)
{
    private string? _tenantOid;
    private string? _environment;
    private string? _secret;
    private bool _enabled;
    private DateTime _rotatedAt;
    private string? _webhookId;
    private string? _externalSubscriptionId;

    [Size(100)]
    [XafDisplayName("Tenant Oid / Referencia")]
    public string? TenantOid
    {
        get => _tenantOid;
        set => SetPropertyValue(nameof(TenantOid), ref _tenantOid, value);
    }

    [Size(50)]
    [XafDisplayName("Entorno")]
    public string? Environment
    {
        get => _environment;
        set => SetPropertyValue(nameof(Environment), ref _environment, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Secreto")]
    public string? Secret
    {
        get => _secret;
        set => SetPropertyValue(nameof(Secret), ref _secret, value);
    }

    [XafDisplayName("Habilitado")]
    public bool Enabled
    {
        get => _enabled;
        set => SetPropertyValue(nameof(Enabled), ref _enabled, value);
    }

    [XafDisplayName("Fecha de Creación")]
    [ModelDefault("DisplayFormat", "G")]
    [ModelDefault("EditMask", "G")]
    public DateTime CreatedAt => CreadoEl ?? DateTime.MinValue;

    [XafDisplayName("Fecha de Rotación")]
    [ModelDefault("DisplayFormat", "G")]
    [ModelDefault("EditMask", "G")]
    public DateTime RotatedAt
    {
        get => _rotatedAt;
        set => SetPropertyValue(nameof(RotatedAt), ref _rotatedAt, value);
    }

    [Size(100)]
    [XafDisplayName("Webhook ID")]
    public string? WebhookId
    {
        get => _webhookId;
        set => SetPropertyValue(nameof(WebhookId), ref _webhookId, value);
    }

    [Size(100)]
    [XafDisplayName("ID de Suscripción Externa")]
    public string? ExternalSubscriptionId
    {
        get => _externalSubscriptionId;
        set => SetPropertyValue(nameof(ExternalSubscriptionId), ref _externalSubscriptionId, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        _enabled = true;
    }
}
