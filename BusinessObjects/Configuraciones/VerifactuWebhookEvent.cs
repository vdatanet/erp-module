using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Configuraciones;

[DefaultClassOptions]
[NavigationItem("Configuraciones")]
[XafDisplayName("Evento Webhook VeriFactu")]
[ImageName("Action_Log_History")]
public class VerifactuWebhookEvent(Session session) : EntidadBase(session)
{
    private string? _eventId;
    private string? _eventType;
    private string? _payload;
    private string? _tenantOid;
    private string? _status;
    private string? _errorMessage;
    private DateTime? _processedAt;

    [Size(100)]
    [Indexed(Unique = true)]
    [XafDisplayName("ID del Evento")]
    public string? EventId
    {
        get => _eventId;
        set => SetPropertyValue(nameof(EventId), ref _eventId, value);
    }

    [Size(100)]
    [XafDisplayName("Tipo de Evento")]
    public string? EventType
    {
        get => _eventType;
        set => SetPropertyValue(nameof(EventType), ref _eventType, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Carga Útil (Payload)")]
    public string? Payload
    {
        get => _payload;
        set => SetPropertyValue(nameof(Payload), ref _payload, value);
    }

    [Size(100)]
    [XafDisplayName("Tenant Oid")]
    public string? TenantOid
    {
        get => _tenantOid;
        set => SetPropertyValue(nameof(TenantOid), ref _tenantOid, value);
    }

    [Size(50)]
    [XafDisplayName("Estado")]
    public string? Status
    {
        get => _status;
        set => SetPropertyValue(nameof(Status), ref _status, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Mensaje de Error")]
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetPropertyValue(nameof(ErrorMessage), ref _errorMessage, value);
    }

    [XafDisplayName("Procesado el")]
    [ModelDefault("DisplayFormat", "G")]
    [ModelDefault("EditMask", "G")]
    public DateTime? ProcessedAt
    {
        get => _processedAt;
        set => SetPropertyValue(nameof(ProcessedAt), ref _processedAt, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        _status = "Received";
    }
}
