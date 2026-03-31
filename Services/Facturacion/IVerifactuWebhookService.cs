using erp.Module.BusinessObjects.Configuraciones;

namespace erp.Module.Services.Facturacion;

public interface IVerifactuWebhookService
{
    Task<bool> VerifySignatureAsync(string payload, string signature, string? tenantOid = null);
    Task ProcessWebhookAsync(string payload, string eventId, string eventType, string signature);
    Task ProcessPendingEventsAsync();
}
