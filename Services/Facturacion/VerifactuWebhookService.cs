using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.MultiTenancy;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl.MultiTenancy;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Configuraciones;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace erp.Module.Services.Facturacion;

public class VerifactuWebhookService : IVerifactuWebhookService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VerifactuWebhookService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ITenantProvider _tenantProvider;

    public VerifactuWebhookService(
        IServiceProvider serviceProvider, 
        ILogger<VerifactuWebhookService> logger,
        IConfiguration configuration,
        ITenantProvider tenantProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        _tenantProvider = tenantProvider;
    }

    private string GetHostConnectionString() => _configuration.GetConnectionString("ConnectionString") 
        ?? throw new Exception("Host connection string not found.");

    private IObjectSpace CreateHostObjectSpace()
    {
        var connectionString = GetHostConnectionString();
        var xpoObjectSpaceProvider = new XPObjectSpaceProvider(connectionString, null);
        return xpoObjectSpaceProvider.CreateObjectSpace();
    }

    public async Task<bool> VerifySignatureAsync(string payload, string signature, string? tenantOid = null)
    {
        if (string.IsNullOrEmpty(signature)) return false;

        using var objectSpace = CreateHostObjectSpace();

        var criteria = tenantOid != null 
            ? CriteriaOperator.FromLambda<WebhookTenant>(c => c.Oid.ToString() == tenantOid && c.WebhookEnabled)
            : CriteriaOperator.FromLambda<WebhookTenant>(c => c.WebhookEnabled);

        var configs = objectSpace.GetObjects<WebhookTenant>(criteria);

        foreach (var config in configs)
        {
            if (string.IsNullOrEmpty(config.WebhookSecret)) continue;

            if (ComputeHmac(payload, config.WebhookSecret) == signature)
            {
                return true;
            }
        }

        return false;
    }

    private string ComputeHmac(string payload, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var bytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLower();
    }

    public async Task ProcessWebhookAsync(string payload, string eventId, string eventType, string signature)
    {
        using var objectSpace = CreateHostObjectSpace();

        // Idempotencia
        var existingEvent = objectSpace.FirstOrDefault<VerifactuWebhookEvent>(e => e.EventId == eventId);
        if (existingEvent != null)
        {
            _logger.LogInformation("Webhook event {EventId} already exists. Skipping.", eventId);
            return;
        }

        // Identificar Tenant
        string? resolvedTenantOid = null;
        var configs = objectSpace.GetObjects<WebhookTenant>(CriteriaOperator.FromLambda<WebhookTenant>(c => c.WebhookEnabled));
        foreach (var config in configs)
        {
            if (!string.IsNullOrEmpty(config.WebhookSecret) && ComputeHmac(payload, config.WebhookSecret) == signature)
            {
                resolvedTenantOid = config.Oid.ToString();
                break;
            }
        }

        var webhookEvent = objectSpace.CreateObject<VerifactuWebhookEvent>();
        webhookEvent.EventId = eventId;
        webhookEvent.EventType = eventType;
        webhookEvent.Payload = payload;
        webhookEvent.TenantOid = resolvedTenantOid;
        webhookEvent.Status = "Received";
        
        objectSpace.CommitChanges();

        _logger.LogInformation("Webhook event {EventId} saved for tenant {TenantOid}.", eventId, resolvedTenantOid);

        if (!string.IsNullOrEmpty(resolvedTenantOid))
        {
            _ = Task.Run(() => ProcessEventAsync(webhookEvent.Oid));
        }
    }

    public async Task ProcessPendingEventsAsync()
    {
        using var objectSpace = CreateHostObjectSpace();

        var pendingEvents = objectSpace.GetObjects<VerifactuWebhookEvent>(
            CriteriaOperator.Parse("Status = 'Received' OR Status = 'Failed'"));
        
        foreach (var ev in pendingEvents)
        {
            await ProcessEventAsync(ev.Oid);
        }
    }

    private async Task ProcessEventAsync(Guid eventOid)
    {
        var originalTenantId = _tenantProvider.TenantId;
        try 
        {
            using var hostOs = CreateHostObjectSpace();
            var ev = hostOs.GetObjectByKey<VerifactuWebhookEvent>(eventOid);
            if (ev == null || string.IsNullOrEmpty(ev.TenantOid)) return;

            if (!Guid.TryParse(ev.TenantOid, out var tenantGuid))
            {
                _logger.LogError("Invalid TenantOid {TenantOid} in event {EventOid}", ev.TenantOid, eventOid);
                return;
            }

            // Resolver ConnectionString del Tenant
            var tenant = hostOs.GetObjectByKey<Tenant>(tenantGuid);
            if (tenant == null)
            {
                _logger.LogError("Tenant {TenantOid} not found for event {EventOid}", ev.TenantOid, eventOid);
                return;
            }

            _logger.LogInformation("Processing event {EventOid} for tenant {TenantName}...", eventOid, tenant.Name);

            // Cambiar contexto al tenant
            _tenantProvider.TenantId = tenantGuid;

            // Aquí usaríamos el IObjectSpaceProvider estándar que ya está configurado con MultiTenancy
            // para obtener un ObjectSpace filtrado por el TenantId que acabamos de establecer.
            using var scope = _serviceProvider.CreateScope();
            var osProvider = scope.ServiceProvider.GetRequiredService<IObjectSpaceProvider>();
            using var tenantOs = osProvider.CreateObjectSpace();

            // Lógica de mapeo de estados y actualización de factura
            if (string.IsNullOrEmpty(ev.Payload))
            {
                _logger.LogWarning("Event {EventOid} has no payload", eventOid);
                return;
            }

            using var jsonDoc = JsonDocument.Parse(ev.Payload);
            var root = jsonDoc.RootElement;
            
            if (root.TryGetProperty("invoiceId", out var invoiceIdProp))
            {
                var invoiceOidStr = invoiceIdProp.GetString();
                if (Guid.TryParse(invoiceOidStr, out var invoiceOid))
                {
                    var factura = tenantOs.GetObjectByKey<FacturaBase>(invoiceOid);
                    if (factura != null)
                    {
                        if (root.TryGetProperty("status", out var statusProp))
                        {
                            var externalStatus = statusProp.GetString();
                            // Mapear externalStatus a EstadoVeriFactu
                            if (Enum.TryParse<EstadoVeriFactu>(externalStatus, true, out var nuevoEstado))
                            {
                                factura.EstadoVeriFactu = nuevoEstado;
                                tenantOs.CommitChanges();
                                _logger.LogInformation("Updated invoice {InvoiceOid} status to {Status}", invoiceOid, nuevoEstado);
                            }
                        }
                    }
                }
            }

            ev.Status = "Processed";
            ev.ProcessedAt = DateTime.UtcNow;
            hostOs.CommitChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event {EventOid}", eventOid);
            using var hostOs = CreateHostObjectSpace();
            var ev = hostOs.GetObjectByKey<VerifactuWebhookEvent>(eventOid);
            if (ev != null)
            {
                ev.Status = "Failed";
                ev.ErrorMessage = ex.Message;
                hostOs.CommitChanges();
            }
        }
        finally
        {
            _tenantProvider.TenantId = originalTenantId;
        }
    }
}
