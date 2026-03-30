using System.Text;
using System.Collections.Concurrent;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using VeriFactu.Business;
using VeriFactu.Business.FlowControl;
using VeriFactu.Business.Operations;
using VeriFactu.Xml.Factu.Respuesta;

namespace erp.Module.Services.Facturacion;

/// <summary>
/// Gestor de la cola de procesamiento de VeriFactu para entornos multi-tenant.
/// Se encarga de suscribirse a los eventos globales de la librería y actualizar las facturas en sus respectivos tenants.
/// </summary>
public class VeriFactuQueueManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<VeriFactuQueueManager> _logger;

    public VeriFactuQueueManager(IServiceProvider serviceProvider, ILogger<VeriFactuQueueManager> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Inicializa la suscripción a los eventos de la cola de VeriFactu.
    /// Este método debe llamarse una sola vez al arrancar la aplicación.
    /// </summary>
    public void Initialize()
    {
        _logger.LogInformation("Inicializando VeriFactuQueueManager.");

        // Según documentación oficial:
        // Añadimos manejador del evento SentFinished
        InvoiceQueue.SentFinished = OnSentFinished;

        // Añadimos manejador del evento SentError
        InvoiceQueue.SentError = OnSentError;
        
        _logger.LogInformation("Suscripción a InvoiceQueue.SentFinished y SentError completada.");
    }

    /// <summary>
    /// Callback para el éxito del envío asíncrono.
    /// </summary>
    private void OnSentFinished(List<InvoiceAction> invoiceActionList, RespuestaRegFactuSistemaFacturacion aeatResponse)
    {
        _logger.LogInformation("VeriFactu: Envío de lote finalizado con éxito. Respuesta AEAT: {Response}", aeatResponse);
        
        // Registrar detalles del lote
        if (invoiceActionList.Count > 0)
        {
            var firstAction = invoiceActionList[0];
            var (batchTenantId, _) = GetTenantAndCorrelationId(firstAction);
            _logger.LogInformation("VeriFactu: Procesando respuesta de lote con {Count} facturas. TenantId detectado en primera acción: {TenantId}", 
                invoiceActionList.Count, batchTenantId);
        }

        // Mostrar en log de la respuesta completa de la agencia tributaria
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            try
            {
                var serializedResponse = SerializeRespuesta(aeatResponse);
                _logger.LogDebug("VeriFactu: Respuesta completa AEAT:\n{FullResponse}", serializedResponse);
                
                // También intentar mostrar el XML crudo (ResponseEnvelope) si está disponible en la primera acción
                if (invoiceActionList.Count > 0)
                {
                    var action = invoiceActionList[0];
                    var responseEnvelopeProp = action.GetType().GetProperty("ResponseEnvelope");
                    var envelope = responseEnvelopeProp?.GetValue(action);
                    if (envelope != null)
                    {
                        _logger.LogDebug("VeriFactu: XML de respuesta AEAT (ResponseEnvelope):\n{XmlResponse}", envelope);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("VeriFactu: No se pudo obtener detalles de la respuesta AEAT para el log: {Msg}", ex.Message);
            }
        }

        // Procesar todas las facturas del lote de forma asíncrona pero rastreable
        _ = ProcessInvoiceActionListAsync(invoiceActionList, aeatResponse, null);
    }

    /// <summary>
    /// Callback para el fallo del envío asíncrono.
    /// </summary>
    private void OnSentError(List<InvoiceAction> invoiceActionList, Exception ex)
    {
        _logger.LogError(ex, "VeriFactu: Error al enviar lote a la AEAT.");

        if (invoiceActionList.Count > 0)
        {
            var firstAction = invoiceActionList[0];
            var (batchTenantId, _) = GetTenantAndCorrelationId(firstAction);
            _logger.LogWarning("VeriFactu: Procesando error de lote con {Count} facturas. TenantId detectado en primera acción: {TenantId}", 
                invoiceActionList.Count, batchTenantId);
        }

        // Procesar todas las facturas del lote de forma asíncrona pero rastreable
        _ = ProcessInvoiceActionListAsync(invoiceActionList, null, ex);
    }

    private async Task ProcessInvoiceActionListAsync(List<InvoiceAction> invoiceActionList, RespuestaRegFactuSistemaFacturacion? aeatResponse, Exception? error)
    {
        var tasks = invoiceActionList.Select(action => ProcessInvoiceActionAsync(action, aeatResponse, error));
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico procesando lote de facturas VeriFactu.");
        }
    }

    private static readonly ConcurrentDictionary<Guid, SemaphoreAndCounter> InvoiceSemaphores = new();

    private class SemaphoreAndCounter
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        public int ReferenceCount;
    }

    private async Task ProcessInvoiceActionAsync(InvoiceAction action, RespuestaRegFactuSistemaFacturacion? aeatResponse, Exception? error)
    {
        _logger.LogInformation("Procesando acción de factura {InvoiceID}. Resultado: {Status}. Error: {Error}", 
            action.Invoice?.InvoiceID, action.Status, error?.Message ?? "Ninguno");

        if (action.Invoice == null) return;

        var (tenantId, correlationId) = GetTenantAndCorrelationId(action);
        
        Guid lockId;
        if (correlationId.HasValue && correlationId.Value != Guid.Empty)
        {
            lockId = correlationId.Value;
        }
        else if (Guid.TryParse(action.Invoice.InvoiceID, out var parsedId))
        {
            lockId = parsedId;
        }
        else
        {
            lockId = GenerateGuidFromInvoiceId(action.Invoice.InvoiceID);
        }

        // Obtener o crear el objeto de control con contador de referencia
        var sac = InvoiceSemaphores.AddOrUpdate(lockId, 
            _ => new SemaphoreAndCounter { ReferenceCount = 1 },
            (_, existing) => {
                Interlocked.Increment(ref existing.ReferenceCount);
                return existing;
            });

        await sac.Semaphore.WaitAsync();

        try 
        {
            await UpdateInvoiceAsync(action, aeatResponse, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error asíncrono en UpdateInvoiceAsync para la factura {Id}", action.Invoice?.InvoiceID);
        }
        finally
        {
            sac.Semaphore.Release();
            
            // Decrementar el contador de referencia y limpiar si llega a cero
            if (Interlocked.Decrement(ref sac.ReferenceCount) == 0)
            {
                // Solo eliminamos si el contador sigue siendo 0 (doble comprobación)
                // Usamos TryRemove con el valor esperado para ser ultra-seguros
                InvoiceSemaphores.TryRemove(new KeyValuePair<Guid, SemaphoreAndCounter>(lockId, sac));
            }
        }
    }

    private Guid GenerateGuidFromInvoiceId(string invoiceId)
    {
        if (string.IsNullOrEmpty(invoiceId)) return Guid.NewGuid();
        
        using (var md5 = System.Security.Cryptography.MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(invoiceId));
            return new Guid(hash);
        }
    }

    private async Task UpdateInvoiceAsync(InvoiceAction action, RespuestaRegFactuSistemaFacturacion? aeatResponse, Exception? error)
    {
        string sellerNif = action.Invoice.SellerID;
        string invoiceId = action.Invoice.InvoiceID;

        // Intentar obtener información de tenant y correlación desde la entrada de la cola
        var (tenantIdFromAction, correlationIdFromAction) = GetTenantAndCorrelationId(action);

        using (var scope = _serviceProvider.CreateScope())
        {
            var sp = scope.ServiceProvider;
            var objectSpaceFactory = sp.GetRequiredService<IObjectSpaceFactory>();

            // 1. Intentar resolución directa por CorrelationId y TenantId (LÓGICA PREFERENTE)
            if (tenantIdFromAction.HasValue && correlationIdFromAction.HasValue)
            {
                if (await TryUpdateByCorrelationAndTenantAsync(sp, tenantIdFromAction.Value, correlationIdFromAction.Value, invoiceId, sellerNif, action, aeatResponse, error))
                {
                    return;
                }
            }
            else
            {
                _logger.LogWarning("VeriFactu: La acción para la factura {InvoiceId} no contiene información de TenantId o CorrelationId. Procediendo con fallbacks.", invoiceId);
            }

            // 2. Fallback 1: Buscar en el registro de auditoría global por InvoiceId y NifEmisor
            if (await TryUpdateByAuditRegistryAsync(sp, invoiceId, sellerNif, action, aeatResponse, error))
            {
                return;
            }

        // 3. Fallback 2: Buscar en todos los tenants (SOLO SI SE PERMITE EXPLÍCITAMENTE)
        var configuration = sp.GetService<IConfiguration>();
        bool disableGlobalFallback = configuration?.GetValue<bool>("VeriFactu:DisableGlobalFallback") ?? true;

        if (!disableGlobalFallback)
        {
            _logger.LogWarning("VeriFactu: Iniciando fallback global para la factura {InvoiceId} (NIF: {SellerNif}).", invoiceId, sellerNif);
            await PerformGlobalTenantFallbackAsync(sp, invoiceId, sellerNif, action, aeatResponse, error);
        }
        else
        {
            _logger.LogError("VeriFactu: No se pudo resolver la factura {InvoiceId} (NIF: {SellerNif}) por ninguna vía técnica y el fallback global está deshabilitado.", invoiceId, sellerNif);
        }
        }
    }

    private (Guid? tenantId, Guid? correlationId) GetTenantAndCorrelationId(InvoiceAction action)
    {
        try
        {
            var entryProp = action.GetType().GetProperty("Entry");
            var entry = entryProp?.GetValue(action);

            if (entry is TenantAwareInvoiceEntry tenantEntry)
            {
                return (tenantEntry.TenantId, tenantEntry.CorrelationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("No se pudo extraer TenantAwareInvoiceEntry de la acción: {Msg}", ex.Message);
        }
        return (null, null);
    }

    private async Task<bool> TryUpdateByCorrelationAndTenantAsync(IServiceProvider sp, Guid tenantId, Guid correlationId, string invoiceId, string sellerNif, InvoiceAction action, RespuestaRegFactuSistemaFacturacion? aeatResponse, Exception? error)
    {
        _logger.LogTrace("Intentando resolución primaria por CorrelationId {CorrelationId} y TenantId {TenantId} para factura {InvoiceId}", correlationId, tenantId, invoiceId);
        try
        {
            using (var hostOS = CreateHostObjectSpace(sp))
            {
                var criteria = CriteriaOperator.Parse("CorrelationId = ? AND TenantId = ?", correlationId, tenantId);
                var audits = hostOS.GetObjects<VeriFactuAudit>(criteria);
                
                if (audits.Count > 1)
                {
                    _logger.LogError("[AMBIGÜEDAD CORRELACIÓN] Se encontraron múltiples registros de auditoría ({Count}) para CorrelationId {CorrelationId} y TenantId {TenantId}.", audits.Count, correlationId, tenantId);
                    return false;
                }

                var audit = audits.FirstOrDefault();
                if (audit != null)
                {
                    // Validación de NIF emisor para evitar cruces accidentales
                    if (!IsNifMatch(audit.NifEmisor, sellerNif))
                    {
                        _logger.LogError("[ERROR CORRELACIÓN] Discrepancia de NIF: Auditoría={AuditNif}, Respuesta={RespNif} para CorrelationId={CorrId}", 
                            audit.NifEmisor, sellerNif, correlationId);
                        return false;
                    }

                    UpdateAuditStatus(audit, action, error);
                    hostOS.CommitChanges();

                    var tenant = hostOS.GetObjectByKey<DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant>(audit.TenantId);
                    if (tenant?.Name == null)
                    {
                        _logger.LogError("Tenant {TenantId} no encontrado o sin nombre válido.", audit.TenantId);
                        return false;
                    }

                    var objectSpaceProvider = sp.GetRequiredService<IObjectSpaceFactory>();
                    using (var tenantOS = objectSpaceProvider.CreateObjectSpace(typeof(InformacionEmpresa), tenant.Name))
                    {
                        FacturaBase? invoice = null;
                        if (audit.InvoiceOid != Guid.Empty)
                        {
                            invoice = tenantOS.GetObjectByKey<FacturaBase>(audit.InvoiceOid);
                        }

                        invoice ??= FindInvoiceInTenant(tenantOS, invoiceId);

                        if (invoice != null)
                        {
                            var companyInfo = tenantOS.FindObject<InformacionEmpresa>(null);
                            if (!IsNifMatch(companyInfo?.Nif, sellerNif))
                            {
                                _logger.LogError("[ERROR CONSISTENCIA] Factura {InvId} en Tenant {Tenant} tiene NIF {CompNif}, pero emisor es {SellNif}", 
                                    invoiceId, tenant.Name, companyInfo?.Nif, sellerNif);
                                return false;
                            }

                            await UpdateInvoiceInternalAsync(sp, tenantOS, invoice, action, aeatResponse, error);
                            _logger.LogInformation("Factura {InvoiceId} actualizada con éxito en tenant {Tenant} vía CorrelationId.", invoiceId, tenant.Name);
                            return true;
                        }
                        
                        _logger.LogError("Factura {InvoiceId} no localizada en tenant {Tenant} (Oid: {Oid}).", invoiceId, tenant.Name, audit.InvoiceOid);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fallo en TryUpdateByCorrelationAndTenantAsync para CorrelationId {CorrelationId}", correlationId);
        }
        return false;
    }

    private async Task<bool> TryUpdateByAuditRegistryAsync(IServiceProvider sp, string invoiceId, string sellerNif, InvoiceAction action, RespuestaRegFactuSistemaFacturacion? aeatResponse, Exception? error)
    {
        try
        {
            using (var hostOS = CreateHostObjectSpace(sp))
            {
                // Buscar por InvoiceId y NifEmisor para mayor seguridad que solo InvoiceId
                var audits = hostOS.GetObjects<VeriFactuAudit>(CriteriaOperator.Parse("InvoiceId = ? AND NifEmisor = ?", invoiceId, sellerNif));
                if (audits.Count > 1)
                {
                    _logger.LogError("Ambigüedad detectada en auditoría: Se encontraron {Count} registros para InvoiceId {InvoiceId} y NIF {SellerNif}. No se puede proceder con la resolución automática.", audits.Count, invoiceId, sellerNif);
                    return false;
                }

                var audit = audits.FirstOrDefault();
                if (audit != null)
                {
                    UpdateAuditStatus(audit, action, error);
                    hostOS.CommitChanges();

                    var tenant = hostOS.GetObjectByKey<DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant>(audit.TenantId);
                    if (tenant?.Name != null)
                    {
                        var objectSpaceFactory = sp.GetRequiredService<IObjectSpaceFactory>();
                        using (var tenantOS = objectSpaceFactory.CreateObjectSpace(typeof(InformacionEmpresa), tenant.Name))
                        {
                            // Intentar buscar por Oid si está disponible
                            FacturaBase? invoice = null;
                            if (audit.InvoiceOid != Guid.Empty)
                            {
                                invoice = tenantOS.GetObjectByKey<FacturaBase>(audit.InvoiceOid);
                            }
                            
                            invoice ??= FindInvoiceInTenant(tenantOS, invoiceId);

                            if (invoice != null)
                            {
                                // Confirmar consistencia de NIF
                                var companyInfo = tenantOS.FindObject<InformacionEmpresa>(null);
                                if (!IsNifMatch(companyInfo?.Nif, sellerNif))
                                {
                                    _logger.LogError("Inconsistencia en fallback de auditoría: Factura {InvoiceId} en tenant {Tenant} no coincide con NIF emisor {SellerNif}", 
                                        invoiceId, tenant.Name, sellerNif);
                                    return false;
                                }

                                await UpdateInvoiceInternalAsync(sp, tenantOS, invoice, action, aeatResponse, error);
                                return true;
                            }
                            else
                            {
                                _logger.LogError("No se pudo encontrar la factura {InvoiceId} en el tenant {Tenant} referenciado por la auditoría.", invoiceId, tenant.Name);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError("Tenant {TenantId} no encontrado para la factura {InvoiceId} en el fallback de auditoría.", audit.TenantId, invoiceId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al intentar actualizar mediante registro de auditoría para {InvoiceId}", invoiceId);
        }
        return false;
    }

    private async Task PerformGlobalTenantFallbackAsync(IServiceProvider sp, string invoiceId, string sellerNif, InvoiceAction action, RespuestaRegFactuSistemaFacturacion? aeatResponse, Exception? error)
    {
        var tenantProvider = sp.GetService<DevExpress.ExpressApp.MultiTenancy.ITenantProvider>();
        if (tenantProvider == null) return;

        var originalTenantId = tenantProvider.TenantId;
        List<(Guid Oid, string Name)> tenants;
        try
        {
            tenantProvider.TenantId = null;
            using (var hostOS = CreateHostObjectSpace(sp))
            {
                tenants = hostOS.GetObjects<DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant>()
                    .Select(t => (t.Oid, t.Name))
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recuperando lista de tenants del host para fallback global.");
            return;
        }
        finally
        {
            tenantProvider.TenantId = originalTenantId;
        }

        var foundCandidates = new List<(Guid TenantOid, string TenantName, FacturaBase Invoice, IObjectSpace OS)>();
        var objectSpaceFactory = sp.GetRequiredService<IObjectSpaceFactory>();
        
        foreach (var tenant in tenants)
        {
            try
            {
                var tenantOS = objectSpaceFactory.CreateObjectSpace(typeof(InformacionEmpresa), tenant.Oid.ToString());
                var info = tenantOS.FindObject<InformacionEmpresa>(null);
                if (IsNifMatch(info?.Nif, sellerNif))
                {
                    var invoice = FindInvoiceInTenant(tenantOS, invoiceId);
                    if (invoice != null)
                    {
                        foundCandidates.Add((tenant.Oid, tenant.Name, invoice, tenantOS));
                    }
                }
                
                // Si no es la factura que buscamos, liberamos el OS
                if (foundCandidates.All(c => c.TenantOid != tenant.Oid))
                {
                    tenantOS.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogTrace("Búsqueda en tenant {Tenant} fallida: {Msg}", tenant.Name, ex.Message);
            }
        }

                if (foundCandidates.Count == 1)
                {
                    var candidate = foundCandidates[0];
                    _logger.LogInformation("Factura {InvoiceId} resuelta mediante fallback global en tenant {Tenant}.", invoiceId, candidate.TenantName);
                    
                    try 
                    {
                        await UpdateInvoiceInternalAsync(sp, candidate.OS, candidate.Invoice, action, aeatResponse, error);
                    }
                    finally
                    {
                        foreach (var c in foundCandidates) c.OS.Dispose();
                    }
                }
                else if (foundCandidates.Count > 1)
                {
                    _logger.LogError("[AMBIGÜEDAD CORRELACIÓN] Ambigüedad crítica en fallback global: Se encontró la factura {InvoiceId} en {Count} tenants ({Tenants}). No se aplicará ninguna actualización.", 
                        invoiceId, foundCandidates.Count, string.Join(", ", foundCandidates.Select(c => c.TenantName)));
                    foreach (var c in foundCandidates) c.OS.Dispose();
                }
                else
                {
                    _logger.LogError("[ERROR CORRELACIÓN] No se encontró la factura {InvoiceId} (NIF: {SellerNif}) en ningún tenant tras fallback global.", invoiceId, sellerNif);
                }
    }

    private FacturaBase? FindInvoiceInTenant(IObjectSpace os, string invoiceId)
    {
        if (Guid.TryParse(invoiceId, out Guid invoiceGuid))
        {
            var invoice = os.GetObjectByKey<FacturaBase>(invoiceGuid);
            if (invoice != null) return invoice;
        }

        var foundInvoices = os.GetObjects<FacturaBase>(CriteriaOperator.Parse("Secuencia = ?", invoiceId));
        if (foundInvoices.Count == 1) return foundInvoices[0];
        if (foundInvoices.Count > 1)
        {
            _logger.LogWarning("Múltiples facturas encontradas con secuencia {Secuencia} en el mismo tenant. No se puede elegir una automáticamente.", invoiceId);
            return null;
        }

        return null;
    }

    private bool IsNifMatch(string? tenantNif, string sellerNif)
    {
        if (string.IsNullOrEmpty(tenantNif)) return false;
        string tNif = tenantNif.Trim().ToUpper();
        string sNif = sellerNif.Trim().ToUpper();
        return tNif == sNif || tNif == "ES" + sNif || sNif == "ES" + tNif;
    }

    private void UpdateAuditStatus(VeriFactuAudit audit, InvoiceAction action, Exception? error)
    {
        string prefix = "";
        if (error != null)
        {
            prefix = "[ERROR TÉCNICO] ";
        }
        else if (action.Status.ToString().Contains("Rechazada", StringComparison.OrdinalIgnoreCase) || 
                 action.Status.ToString().Contains("Error", StringComparison.OrdinalIgnoreCase))
        {
            prefix = "[ERROR RESPUESTA AEAT] ";
        }

        audit.EstadoEnvio = prefix + action.Status.ToString();
        if (error != null) audit.EstadoEnvio += " - Detalle: " + error.Message;
        
        // Registrar BatchId si está disponible en la acción
        try
        {
            var batchIdProp = action.GetType().GetProperty("BatchId") ?? action.GetType().GetProperty("TransactionId");
            var batchId = batchIdProp?.GetValue(action)?.ToString();
            if (!string.IsNullOrEmpty(batchId))
            {
                audit.BatchId = batchId;
            }
        }
        catch { /* Ignorar errores de reflexión */ }
    }

    private async Task UpdateInvoiceInternalAsync(IServiceProvider serviceProvider, IObjectSpace tenantOS, FacturaBase invoice, InvoiceAction action, RespuestaRegFactuSistemaFacturacion? aeatResponse, Exception? error)
    {
        var veriFactuService = serviceProvider.GetRequiredService<VeriFactuService>();
        var invoiceId = invoice.Secuencia;
        
        var statusStr = action.Status.ToString();
        var status = error != null ? VeriFactuConstants.ErrorTecnico : 
                     (statusStr.Contains("Rechazada", StringComparison.OrdinalIgnoreCase) || statusStr.Contains("Error", StringComparison.OrdinalIgnoreCase) ? VeriFactuConstants.Rechazada : VeriFactuConstants.Correcto);

        // Actualizar factura con CorrelationId si no lo tenía
        var (tenantIdFromAction, correlationIdFromAction) = GetTenantAndCorrelationId(action);
        if (correlationIdFromAction.HasValue && invoice.CorrelationId == Guid.Empty)
        {
            invoice.CorrelationId = correlationIdFromAction.Value;
        }

        // Si el estado de la acción es "Pendiente" y no hay error, mantenemos el estado PendienteVeriFactu
        if (error == null && statusStr.Contains("Pendiente", StringComparison.OrdinalIgnoreCase))
        {
            status = VeriFactuConstants.PendienteVeriFactu;
        }

        string? csv = null;
        string? validationUrl = null;
        byte[]? qrData = null;
        byte[]? xmlData = null;

        try
        {
            if (aeatResponse != null)
            {
                _logger.LogTrace("Extrayendo detalles de aeatResponse para {Id}", invoiceId);
                
                // Mostrar en log de la respuesta completa
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    try
                    {
                        _logger.LogDebug("VeriFactu: Procesando respuesta completa para factura {Id}:\n{FullResponse}", 
                            invoiceId, SerializeRespuesta(aeatResponse));

                        // Intentar obtener el XML crudo para el log
                        var responseEnvelopePropDebug = action.GetType().GetProperty("ResponseEnvelope");
                        var envelopeDebug = responseEnvelopePropDebug?.GetValue(action);
                        if (envelopeDebug != null)
                        {
                            _logger.LogDebug("VeriFactu: XML de respuesta para factura {Id} (ResponseEnvelope):\n{XmlResponse}", invoiceId, envelopeDebug);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug("VeriFactu: No se pudo obtener detalles de la respuesta AEAT para la factura {Id}: {Msg}", invoiceId, ex.Message);
                    }
                }
                
                // Extraer CSV de la respuesta global
                var csvProp = aeatResponse.GetType().GetProperty("CSV");
                csv = csvProp?.GetValue(aeatResponse) as string;

                // Intentar obtener el XML de la respuesta de action.ResponseEnvelope (si existe)
                var responseEnvelopeProp = action.GetType().GetProperty("ResponseEnvelope");
                if (responseEnvelopeProp != null)
                {
                    var envelope = responseEnvelopeProp.GetValue(action);
                    if (envelope is string envelopeStr)
                    {
                        xmlData = Encoding.UTF8.GetBytes(envelopeStr);
                    }
                    else if (envelope is byte[] envelopeBytes)
                    {
                        xmlData = envelopeBytes;
                    }
                }

                // Buscar en la línea de respuesta correspondiente
                var lineasProp = aeatResponse.GetType().GetProperty("RespuestaLinea");
                var lineas = lineasProp?.GetValue(aeatResponse) as System.Collections.IEnumerable;
                if (lineas != null)
                {
                    foreach (var linea in lineas)
                    {
                        var idFacturaProp = linea.GetType().GetProperty("IDFactura");
                        var idFactura = idFacturaProp?.GetValue(linea);
                        if (idFactura != null)
                        {
                            var numSerieProp = idFactura.GetType().GetProperty("NumSerieFactura");
                            var numSerie = numSerieProp?.GetValue(idFactura) as string;
                            
                            if (numSerie != null && invoice.Secuencia != null && numSerie.Replace(" ", "").ToUpper() == invoice.Secuencia.Replace(" ", "").ToUpper())
                            {
                                // Estado de la línea
                                var estadoLineaProp = linea.GetType().GetProperty("EstadoRegistro");
                                var estadoLinea = estadoLineaProp?.GetValue(linea)?.ToString();
                                
                                if (estadoLinea != null)
                                {
                                    if (estadoLinea.Contains("Rechazada", StringComparison.OrdinalIgnoreCase) || 
                                        estadoLinea.Contains("Error", StringComparison.OrdinalIgnoreCase))
                                    {
                                        status = VeriFactuConstants.Rechazada;
                                    }
                                    else if (estadoLinea.Contains("Correcto", StringComparison.OrdinalIgnoreCase))
                                    {
                                        status = VeriFactuConstants.Correcto;
                                    }
                                }
                                
                                // Intentar obtener QR de la línea si existe
                                var qrLineaProp = linea.GetType().GetProperty("QrCode") ?? linea.GetType().GetProperty("QR");
                                qrData = qrLineaProp?.GetValue(linea) as byte[];
                                
                                break;
                            }
                        }
                    }
                }

                // Si no hay QR en la línea, buscar en action o aeatResponse raíz
                if (qrData == null)
                {
                    var qrProp = aeatResponse.GetType().GetProperty("QrCode") ?? aeatResponse.GetType().GetProperty("QR") ?? aeatResponse.GetType().GetProperty("DatosQR");
                    if (qrProp != null)
                    {
                        qrData = qrProp.GetValue(aeatResponse) as byte[];
                    }
                    
                    if (qrData == null)
                    {
                        qrProp = action.GetType().GetProperty("QrCode") ?? action.GetType().GetProperty("QR") ?? action.GetType().GetProperty("DatosQR");
                        if (qrProp != null)
                        {
                            qrData = qrProp.GetValue(action) as byte[];
                        }
                    }
                }
                
                var urlProp = aeatResponse.GetType().GetProperty("ValidationUrl") ?? aeatResponse.GetType().GetProperty("Url") ?? aeatResponse.GetType().GetProperty("URL");
                if (urlProp != null)
                {
                    validationUrl = urlProp.GetValue(aeatResponse) as string;
                }
                
                if (string.IsNullOrEmpty(validationUrl))
                {
                    urlProp = action.GetType().GetProperty("ValidationUrl") ?? action.GetType().GetProperty("Url") ?? action.GetType().GetProperty("URL");
                    if (urlProp != null)
                    {
                        validationUrl = urlProp.GetValue(action) as string;
                    }
                }

                // Fallback: Si no hay URL de validación o QR, intentamos generarlos localmente desde la factura de VeriFactu
                if (status == VeriFactuConstants.Correcto && (string.IsNullOrEmpty(validationUrl) || qrData == null))
                {
                    try
                    {
                        _logger.LogInformation("VeriFactu: Intentando generar URL de validación local para {Id}", invoiceId);
                        var registro = action.Invoice.GetRegistroAlta();
                        
                        if (string.IsNullOrEmpty(validationUrl))
                        {
                            validationUrl = registro.GetUrlValidate();
                            _logger.LogInformation("VeriFactu: URL de validación generada localmente: {Url}", validationUrl);
                        }
                        
                        if (qrData == null)
                        {
                            var qrProp = registro.GetType().GetProperty("QrCode") ?? registro.GetType().GetProperty("QR") ?? registro.GetType().GetProperty("Qr");
                            if (qrProp != null)
                            {
                                qrData = qrProp.GetValue(registro) as byte[];
                            }

                            if (qrData == null)
                            {
                                // Intentar a través de GetValidateQr() (recomendado por usuario)
                                var qrMethod = registro.GetType().GetMethod("GetValidateQr");
                                if (qrMethod != null)
                                {
                                    qrData = qrMethod.Invoke(registro, null) as byte[];
                                }
                            }

                            if (qrData == null)
                            {
                                // Intentar a través de GetQrCode() si existe como método
                                var qrMethod = registro.GetType().GetMethod("GetQrCode") ?? registro.GetType().GetMethod("GetQR");
                                if (qrMethod != null)
                                {
                                    qrData = qrMethod.Invoke(registro, null) as byte[];
                                }
                            }
                            
                            if (qrData != null)
                            {
                                _logger.LogInformation("VeriFactu: QR obtenido localmente del registro para {Id}", invoiceId);
                            }
                            else
                            {
                                _logger.LogWarning("VeriFactu: No se pudo obtener el QR localmente del registro para {Id} (Propiedades intentadas: QrCode, QR, Qr; Métodos: GetValidateQr, GetQrCode, GetQR)", invoiceId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("VeriFactu: No se pudo generar la URL de validación o QR localmente para {Id}: {Msg}", invoiceId, ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extrayendo detalles de la respuesta VeriFactu para {Id}", invoiceId);
        }
        
        var response = new VeriFactuResponse(
            status,
            error != null ? "TECH_ERROR" : null,
            error != null ? error.Message : (aeatResponse != null ? SerializeRespuesta(aeatResponse) : "No hay respuesta de la AEAT"),
            xmlData,
            csv,
            validationUrl,
            qrData);

        try
        {
            _logger.LogInformation("Actualizando factura {Secuencia} (ID: {Id}) con respuesta de VeriFactu. Estado: {Status}, Error: {Error}", 
                invoice.Secuencia, invoice.Oid, status, error?.Message ?? "(null)");
            
            veriFactuService.UpdateInvoiceFromResponse(tenantOS, invoice, response, action.Invoice);
            tenantOS.CommitChanges();
            _logger.LogInformation("Factura {Id} actualizada correctamente.", invoiceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando cambios en la factura {Id} del tenant.", invoice.Oid);
        }
    }

    private string SerializeRespuesta(object obj)
    {
        if (obj == null) return "null";
        var sb = new StringBuilder();
        SerializeObject(obj, sb, 0);
        return sb.ToString();
    }

    private void SerializeObject(object obj, StringBuilder sb, int indent)
    {
        if (obj == null) return;
        var prefix = new string(' ', indent * 2);
        var type = obj.GetType();
        
        if (type.IsPrimitive || obj is string || obj is decimal || obj is DateTime || obj is Guid)
        {
            sb.AppendLine($"{prefix}{obj}");
            return;
        }

        if (obj is System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                sb.AppendLine($"{prefix}- Item:");
                SerializeObject(item, sb, indent + 1);
            }
            return;
        }

        foreach (var prop in type.GetProperties())
        {
            try
            {
                var val = prop.GetValue(obj);
                if (val == null)
                {
                    sb.AppendLine($"{prefix}{prop.Name}: (null)");
                    continue;
                }

                var valType = val.GetType();
                if (valType.IsPrimitive || val is string || val is decimal || val is DateTime || val is Guid)
                {
                    sb.AppendLine($"{prefix}{prop.Name}: {val}");
                }
                else if (indent < 3) // Limitar profundidad para evitar ciclos o logs infinitos
                {
                    sb.AppendLine($"{prefix}{prop.Name}:");
                    SerializeObject(val, sb, indent + 1);
                }
                else
                {
                    sb.AppendLine($"{prefix}{prop.Name}: {valType.Name}");
                }
            }
            catch { /* Ignorar errores de acceso */ }
        }
    }

    private IObjectSpace CreateHostObjectSpace(IServiceProvider sp)
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("ConnectionString");
        var xpoObjectSpaceProvider = new DevExpress.ExpressApp.Xpo.XPObjectSpaceProvider(connectionString, null, true, true);
        return xpoObjectSpaceProvider.CreateObjectSpace();
    }
}
