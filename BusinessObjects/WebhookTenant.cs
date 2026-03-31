using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.MultiTenancy;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Configuraciones")]
public class WebhookTenant(Session session) : Tenant(session)
{
    private string? _webhookEnvironment;
    private string? _webhookSecret;
    private bool _webhookEnabled;
    private DateTime _webhookRotatedAt;
    private string? _webhookId;
    private string? _externalSubscriptionId;

    [Size(50)]
    public string? WebhookEnvironment
    {
        get => _webhookEnvironment;
        set => SetPropertyValue(nameof(WebhookEnvironment), ref _webhookEnvironment, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string? WebhookSecret
    {
        get => _webhookSecret;
        set => SetPropertyValue(nameof(WebhookSecret), ref _webhookSecret, value);
    }

    public bool WebhookEnabled
    {
        get => _webhookEnabled;
        set => SetPropertyValue(nameof(WebhookEnabled), ref _webhookEnabled, value);
    }

    [ModelDefault("DisplayFormat", "G")]
    [ModelDefault("EditMask", "G")]
    public DateTime WebhookRotatedAt
    {
        get => _webhookRotatedAt;
        set => SetPropertyValue(nameof(WebhookRotatedAt), ref _webhookRotatedAt, value);
    }

    [Size(100)]
    public string? WebhookId
    {
        get => _webhookId;
        set => SetPropertyValue(nameof(WebhookId), ref _webhookId, value);
    }

    [Size(100)]
    public string? ExternalSubscriptionId
    {
        get => _externalSubscriptionId;
        set => SetPropertyValue(nameof(ExternalSubscriptionId), ref _externalSubscriptionId, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        WebhookEnabled = true;
    }
}
