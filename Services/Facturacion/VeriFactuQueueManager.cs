using System.Text;
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

        foreach (var action in invoiceActionList)
        {
            try
            {
                ProcessInvoiceAction(action, aeatResponse, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando el resultado de la factura {InvoiceID} en el callback.", action.Invoice?.InvoiceID);
            }
        }
    }

    /// <summary>
    /// Callback para el fallo del envío asíncrono.
    /// </summary>
    private void OnSentError(List<InvoiceAction> invoiceActionList, Exception ex)
    {
        _logger.LogError(ex, "VeriFactu: Error al enviar lote a la AEAT.");

        foreach (var action in invoiceActionList)
        {
            try
            {
                ProcessInvoiceAction(action, null, ex);
            }
            catch (Exception innerEx)
            {
                _logger.LogError(innerEx, "Error procesando el error de la factura {InvoiceID} en el callback.", action.Invoice?.InvoiceID);
            }
        }
    }

    private void ProcessInvoiceAction(InvoiceAction action, RespuestaRegFactuSistemaFacturacion? aeatResponse, Exception? error)
    {
        _logger.LogInformation("Procesando acción de factura {InvoiceID}. Resultado: {Status}. Error: {Error}", 
            action.Invoice?.InvoiceID, action.Status, error?.Message ?? "Ninguno");

        if (action.Invoice == null) return;

        Task.Run(async () => {
            try 
            {
                await UpdateInvoiceAsync(action, aeatResponse, error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error asíncrono en UpdateInvoiceAsync para la factura {Id}", action.Invoice?.InvoiceID);
            }
        });
    }

    private async Task UpdateInvoiceAsync(InvoiceAction action, RespuestaRegFactuSistemaFacturacion? aeatResponse, Exception? error)
    {
        string sellerNif = action.Invoice.SellerID;
        string invoiceId = action.Invoice.InvoiceID;

        // Intentar obtener información de tenant y correlación desde la entrada de la cola
        Guid? tenantIdFromAction = null;
        Guid? correlationIdFromAction = null;

        try
        {
            // InvoiceAction.Entry suele ser de tipo InvoiceEntry.
            // En VeriFactuAdapter.SendInvoiceAsync estamos usando TenantAwareInvoiceEntry.
            var entryProp = action.GetType().GetProperty("Entry");
            var entry = entryProp?.GetValue(action);
            
            if (entry is TenantAwareInvoiceEntry tenantEntry)
            {
                tenantIdFromAction = tenantEntry.TenantId;
                correlationIdFromAction = tenantEntry.CorrelationId;
                _logger.LogInformation("VeriFactu: Información de tenant {TenantId} y correlación {CorrelationId} recuperada de la cola para factura {InvoiceId}", 
                    tenantIdFromAction, correlationIdFromAction, invoiceId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug("No se pudo extraer TenantAwareInvoiceEntry de la acción: {Msg}", ex.Message);
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var objectSpaceFactory = scope.ServiceProvider.GetRequiredService<IObjectSpaceFactory>();
            var tenantProvider = scope.ServiceProvider.GetService<DevExpress.ExpressApp.MultiTenancy.ITenantProvider>();
            
            if (tenantProvider == null) return;

            // 1. Intentar resolución directa por CorrelationId y TenantId (NUEVA LÓGICA OPTIMIZADA)
            if (tenantIdFromAction.HasValue && correlationIdFromAction.HasValue)
            {
                try
                {
                    using (var hostOS = CreateHostObjectSpace(scope.ServiceProvider))
                    {
                        var audit = hostOS.FindObject<VeriFactuAudit>(CriteriaOperator.Parse("CorrelationId = ? AND TenantId = ?", correlationIdFromAction.Value, tenantIdFromAction.Value));
                        if (audit != null)
                        {
                            _logger.LogInformation("VeriFactu: Registro de auditoría encontrado por CorrelationId {CorrelationId} en tenant {TenantID}", correlationIdFromAction, audit.TenantId);
                            
                            // Actualizar el registro de auditoría
                            audit.EstadoEnvio = action.Status.ToString();
                            if (error != null) audit.EstadoEnvio += " - Error: " + error.Message;
                            hostOS.CommitChanges();

                            var tenant = hostOS.GetObjectByKey<DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant>(audit.TenantId);
                            if (tenant != null)
                            {
                                using (var tenantOS = objectSpaceFactory.CreateObjectSpace(typeof(InformacionEmpresa), tenant.Name))
                                {
                                    FacturaBase? invoice = null;
                                    if (Guid.TryParse(invoiceId, out Guid invoiceGuid))
                                        invoice = tenantOS.GetObjectByKey<FacturaBase>(invoiceGuid);
                                    
                                    invoice ??= tenantOS.FindObject<FacturaBase>(CriteriaOperator.Parse("Secuencia = ?", invoiceId));

                                    if (invoice != null)
                                    {
                                        await UpdateInvoiceInternalAsync(scope.ServiceProvider, tenantOS, invoice, action, aeatResponse, error);
                                        return; // ÉXITO: Resolución directa completada
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error en resolución directa por CorrelationId para la factura {InvoiceId}", invoiceId);
                }
            }

            // 2. Obtener todos los tenants del host (necesario para fallbacks)
            var originalTenantId = tenantProvider.TenantId;
            List<(Guid Oid, string Name)> tenants;
            try
            {
                tenantProvider.TenantId = null; // Contexto host
                using (var hostOS = CreateHostObjectSpace(scope.ServiceProvider))
                {
                    tenants = hostOS.GetObjects<DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant>()
                        .Select(t => (t.Oid, t.Name))
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recuperando lista de tenants del host.");
                return;
            }
            finally
            {
                tenantProvider.TenantId = originalTenantId;
            }

            // 3. Fallback 1: Buscar en qué tenant coincide el NIF y el ID de factura a través de VeriFactuAudit
            bool updatedByAudit = false;
            try
            {
                using (var hostOS = CreateHostObjectSpace(scope.ServiceProvider))
                {
                    var audit = hostOS.FindObject<VeriFactuAudit>(CriteriaOperator.Parse("InvoiceId = ? AND NifEmisor = ?", invoiceId, sellerNif));
                    if (audit != null)
                    {
                        _logger.LogInformation("Registro de auditoría VeriFactuAudit encontrado para {InvoiceID} en tenant {TenantID}", invoiceId, audit.TenantId);
                        
                        // Actualizar el registro de auditoría
                        audit.EstadoEnvio = action.Status.ToString();
                        if (error != null) audit.EstadoEnvio += " - Error: " + error.Message;
                        hostOS.CommitChanges();

                        // Obtener el nombre del tenant para poder crear su ObjectSpace (DevExpress MultiTenancy usa el Name)
                        string? tenantName = null;
                        var tenant = hostOS.GetObjectByKey<DevExpress.Persistent.BaseImpl.MultiTenancy.Tenant>(audit.TenantId);
                        tenantName = tenant?.Name;

                        if (tenantName != null)
                        {
                            // Ahora buscar la factura en el tenant indicado por el audit
                            using (var tenantOS = objectSpaceFactory.CreateObjectSpace(typeof(InformacionEmpresa), tenantName))
                            {
                                // Verificamos si invoiceId es un Guid para evitar el error de casting en Oid
                                FacturaBase? invoice = null;
                                if (Guid.TryParse(invoiceId, out Guid invoiceGuid))
                                {
                                    invoice = tenantOS.FindObject<FacturaBase>(CriteriaOperator.Parse("Oid = ?", invoiceGuid));
                                }
                                    
                                if (invoice == null)
                                {
                                    invoice = tenantOS.FindObject<FacturaBase>(CriteriaOperator.Parse("Secuencia = ?", invoiceId));
                                }

                                if (invoice == null)
                                {
                                    string cleanInvoiceId = invoiceId.Replace(" ", "").Replace("-", "").Replace("/", "").ToUpper();
                                    invoice = tenantOS.GetObjects<FacturaBase>(null).FirstOrDefault(f => 
                                        f.Secuencia != null && 
                                        f.Secuencia.Replace(" ", "").Replace("-", "").Replace("/", "").ToUpper() == cleanInvoiceId);
                                }

                                if (invoice != null)
                                {
                                    await UpdateInvoiceInternalAsync(scope.ServiceProvider, tenantOS, invoice, action, aeatResponse, error);
                                    updatedByAudit = true;
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("No se encontró el nombre del tenant {TenantID} para el registro de auditoría de {InvoiceID}", audit.TenantId, invoiceId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al intentar actualizar factura mediante VeriFactuAudit.");
            }

            if (updatedByAudit) return;

            // 4. Fallback 2: buscar en todos los tenants (lógica original)
            foreach (var tenant in tenants)
            {
                try
                {
                    using (var tenantOS = objectSpaceFactory.CreateObjectSpace(typeof(InformacionEmpresa), tenant.Oid.ToString()))
                    {
                        var info = tenantOS.FindObject<InformacionEmpresa>(null);
                        
                        // Normalizamos los NIFs para la comparación
                        string? infoNif = info?.Nif?.Trim().ToUpper();
                        string? normalizedSellerNif = sellerNif.Trim().ToUpper();

                        _logger.LogTrace("Comparando NIF emisor {SellerNif} con NIF tenant {TenantNif} (Tenant: {TenantName})", 
                            normalizedSellerNif, infoNif, tenant.Name);

                        if (infoNif != null && (infoNif == normalizedSellerNif || infoNif == "ES" + normalizedSellerNif || normalizedSellerNif == "ES" + infoNif))
                        {
                            // Buscamos la factura por Oid o Secuencia. 
                            // Verificamos si invoiceId es un Guid para evitar el error de casting en Oid
                            FacturaBase? invoice = null;
                            if (Guid.TryParse(invoiceId, out Guid invoiceGuid))
                            {
                                invoice = tenantOS.FindObject<FacturaBase>(CriteriaOperator.Parse("Oid = ?", invoiceGuid));
                            }
                            
                            if (invoice == null)
                            {
                                invoice = tenantOS.FindObject<FacturaBase>(CriteriaOperator.Parse("Secuencia = ?", invoiceId));
                            }
                            
                            if (invoice == null && invoiceId.Contains("/"))
                            {
                                // Si no se encuentra y parece un número de factura (serie/año/número), buscamos solo por Secuencia
                                // Usamos una comparación más flexible (ignorando espacios/case)
                                string normalizedInvoiceId = invoiceId.Replace(" ", "").ToUpper();
                                invoice = tenantOS.FindObject<FacturaBase>(CriteriaOperator.FromLambda<FacturaBase>(f => f.Secuencia != null && f.Secuencia.Replace(" ", "").ToUpper() == normalizedInvoiceId));
                            }

                            if (invoice == null)
                            {
                                // Si aún no se encuentra, intentamos una búsqueda por Secuencia ignorando espacios y separadores comunes
                                string cleanInvoiceId = invoiceId.Replace(" ", "").Replace("-", "").Replace("/", "").ToUpper();
                                invoice = tenantOS.FindObject<FacturaBase>(CriteriaOperator.FromLambda<FacturaBase>(f => 
                                    f.Secuencia != null && 
                                    f.Secuencia.Replace(" ", "").Replace("-", "").Replace("/", "").ToUpper() == cleanInvoiceId));
                            }

                            if (invoice == null)
                            {
                                // Si aún no se encuentra, logueamos las últimas facturas para diagnóstico con detalles de limpieza
                                var sorting = new List<SortProperty> { new SortProperty("Secuencia", DevExpress.Xpo.DB.SortingDirection.Descending) };
                                var recentInvoices = tenantOS.GetObjects<FacturaBase>(null, sorting, false).Take(20).ToList();
                                string recentSeqs = string.Join(", ", recentInvoices.Select(f => $"'{f.Secuencia}'"));
                                string cleanInvoiceId = invoiceId.Replace(" ", "").Replace("-", "").Replace("/", "").ToUpper();
                                
                                _logger.LogDebug("Factura {Id} (Limpia: {CleanId}) no encontrada en tenant {Tenant}. Facturas recientes: {Recent}", 
                                    invoiceId, cleanInvoiceId, tenant.Name, recentSeqs);
                            }

                            if (invoice != null)
                            {
                                await UpdateInvoiceInternalAsync(scope.ServiceProvider, tenantOS, invoice, action, aeatResponse, error);
                                return; // Éxito en el fallback
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogTrace("Búsqueda en tenant {Tenant} fallida o sin resultados: {Msg}", tenant.Name, ex.Message);
                }
            }
            
            _logger.LogWarning("No se encontró la factura {InvoiceID} del emisor {Nif} en ningún tenant.", invoiceId, sellerNif);
        }
    }

    private async Task UpdateInvoiceInternalAsync(IServiceProvider serviceProvider, IObjectSpace tenantOS, FacturaBase invoice, InvoiceAction action, RespuestaRegFactuSistemaFacturacion? aeatResponse, Exception? error)
    {
        var veriFactuService = serviceProvider.GetRequiredService<VeriFactuService>();
        var invoiceId = invoice.Secuencia;
        
        var statusStr = action.Status.ToString();
        var status = error != null ? VeriFactuConstants.ErrorTecnico : 
                     (statusStr.Contains("Rechazada", StringComparison.OrdinalIgnoreCase) ? VeriFactuConstants.Rechazada : VeriFactuConstants.Correcto);

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
