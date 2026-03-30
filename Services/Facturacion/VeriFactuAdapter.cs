using DevExpress.ExpressApp;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VeriFactu.Business;
using VeriFactu.Business.FlowControl;
using VeriFactu.Config;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Helpers.Comun;
using erp.Module.Helpers.Contactos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace erp.Module.Services.Facturacion;

/// <summary>
/// Entrada de factura extendida para incluir el ID del tenant y un ID de correlación único.
/// </summary>
public class TenantAwareInvoiceEntry(Invoice invoice, Guid tenantId, Guid correlationId) : InvoiceEntry(invoice)
{
    public Guid TenantId { get; } = tenantId;
    public Guid CorrelationId { get; } = correlationId;
}

public class VeriFactuAdapter(ILogger<VeriFactuAdapter> logger) : IVeriFactuAdapter
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> TenantSemaphores = new();

    public async Task<VeriFactuResponse> SendInvoiceAsync(Invoice veriFactuInvoice, InformacionEmpresa companyInfo)
    {
        ArgumentNullException.ThrowIfNull(veriFactuInvoice);
        ArgumentNullException.ThrowIfNull(companyInfo);

        // Habilitar logging según documentación oficial
        Settings.Current.LoggingEnabled = true;

        var tenantProvider = companyInfo.Session.ServiceProvider?.GetService<DevExpress.ExpressApp.MultiTenancy.ITenantProvider>();
        var tenantId = tenantProvider?.TenantId ?? Guid.Empty;
        var correlationId = Guid.NewGuid();

        string tenantNif = companyInfo.Nif ?? "Global";
        var semaphore = TenantSemaphores.GetOrAdd(tenantNif, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            await ConfigureVeriFactuAsync(companyInfo);

            var invoiceEntry = new TenantAwareInvoiceEntry(veriFactuInvoice, tenantId, correlationId);
            
            // Según requerimiento AEAT 5. Control de flujo:
            // Añadimos el documento a la cola de procesamiento activa.
            // En la cola se irán realizando los envíos cuando los documentos en espera sean 1.000 
            // o cuando el tiempo de espera (establecido en las respuestas de la AEAT) haya finalizado.
            // El envío se realiza en un hilo separado gestionado por la librería.
            
            logger.LogInformation("--- Datos de Invoice a encolar ---");
            logger.LogInformation("InvoiceID: {InvoiceID}", veriFactuInvoice.InvoiceID);
            logger.LogInformation("SellerID: {SellerID}", veriFactuInvoice.SellerID);
            
            // Intentar obtener SellerName mediante reflexión si no está accesible directamente
            string sellerNameValue = string.Empty;
            try
            {
                var sellerNameProp = veriFactuInvoice.GetType().GetProperty("SellerName");
                if (sellerNameProp != null)
                {
                    sellerNameValue = sellerNameProp.GetValue(veriFactuInvoice)?.ToString() ?? string.Empty;
                }
            }
            catch { /* Ignorar errores de reflexión en logs */ }

            logger.LogInformation("SellerName: {SellerName}", sellerNameValue);
            
            logger.LogInformation("InvoiceDate: {InvoiceDate}", veriFactuInvoice.InvoiceDate);
            logger.LogInformation("InvoiceType: {InvoiceType}", veriFactuInvoice.InvoiceType);
            logger.LogInformation("TotalAmount: {TotalAmount}", veriFactuInvoice.TotalAmount);
            logger.LogInformation("BuyerID: {BuyerID}", veriFactuInvoice.BuyerID);

            // Loguear si InvoiceQueue.ActiveInvoiceQueue es nulo o si tiene elementos
            try 
            {
                if (InvoiceQueue.ActiveInvoiceQueue == null)
                {
                    logger.LogWarning("¡ATENCIÓN! InvoiceQueue.ActiveInvoiceQueue es nulo.");
                }
                else 
                {
                    logger.LogInformation("Estado de ActiveInvoiceQueue: Count={Count}", 
                        InvoiceQueue.ActiveInvoiceQueue.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning("No se pudo inspeccionar ActiveInvoiceQueue: {Message}", ex.Message);
            }
            
            // Loguear campos del emisor si están disponibles
            try
            {
                var invoiceType = veriFactuInvoice.GetType();
                var idEmisor = invoiceType.GetProperty("IDEmisor")?.GetValue(veriFactuInvoice) ?? 
                               invoiceType.GetProperty("IDEmisorFactura")?.GetValue(veriFactuInvoice);
                var idType = invoiceType.GetProperty("IDType")?.GetValue(veriFactuInvoice) ?? 
                             invoiceType.GetProperty("SellerIDType")?.GetValue(veriFactuInvoice);
                logger.LogInformation("IDEmisor (Mapeado): {IDEmisor}", idEmisor);
                logger.LogInformation("IDType (Mapeado): {IDType}", idType);
            }
            catch { }

            logger.LogInformation("TaxItems Count: {TaxItemsCount}", veriFactuInvoice.TaxItems?.Count ?? 0);
            logger.LogInformation("---------------------------------");

            // Crear registro de auditoría antes de añadir a la cola
            try
            {
                var sp = companyInfo.Session.ServiceProvider;
                if (sp != null)
                {
                    using var hostOS = CreateHostObjectSpace(sp);
                    var audit = hostOS.CreateObject<VeriFactuAudit>();
                    audit.TenantId = tenantId;
                    audit.CorrelationId = correlationId;
                    audit.InvoiceId = veriFactuInvoice.InvoiceID ?? string.Empty;
                    audit.NifEmisor = veriFactuInvoice.SellerID ?? string.Empty;
                    audit.NumeroSerie = companyInfo.PrefijoFacturasVentaPorDefecto ?? string.Empty;
                    audit.BatchId = string.Empty; 
                    audit.EstadoEnvio = "Encolada";
                    hostOS.CommitChanges();
                    logger.LogInformation("Registro de auditoría VeriFactuAudit creado en el HOST para {InvoiceID}", veriFactuInvoice.InvoiceID);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo crear el registro de auditoría VeriFactuAudit para {InvoiceID}", veriFactuInvoice.InvoiceID);
            }

            try
            {
                logger.LogInformation("Llamando a InvoiceQueue.ActiveInvoiceQueue.Add para la factura {InvoiceID}...", veriFactuInvoice.InvoiceID);
                if (InvoiceQueue.ActiveInvoiceQueue != null)
                {
                    InvoiceQueue.ActiveInvoiceQueue.Add(invoiceEntry);
                    logger.LogInformation("Factura {InvoiceID} añadida con éxito a ActiveInvoiceQueue.", veriFactuInvoice.InvoiceID);
                }
                else
                {
                    logger.LogWarning("ActiveInvoiceQueue es nulo. No se pudo añadir la factura {InvoiceID} a la cola.", veriFactuInvoice.InvoiceID);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error crítico al añadir factura a ActiveInvoiceQueue: {Message}. StackTrace: {StackTrace}", ex.Message, ex.StackTrace);
                throw;
            }

            logger.LogInformation("Factura {InvoiceID} añadida a la cola para el tenant {TenantId} (NIF: {Nif}). ConfigFile: {ConfigFile}. NIF Emisor: {SellerID}", 
                veriFactuInvoice.InvoiceID, tenantId, tenantNif, companyInfo.NombreArchivoConfigVeriFactu, veriFactuInvoice.SellerID);

            return new VeriFactuResponse(
                VeriFactuConstants.PendienteVeriFactu, // Usamos un estado que indique que está en cola
                null,
                "Enviado a la cola de procesamiento de VeriFactu",
                null,
                null,
                null,
                null);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task ConfigureVeriFactuAsync(InformacionEmpresa companyInfo)
    {
        logger.LogInformation("Iniciando configuración de VeriFactu para la empresa {Empresa} (NIF: {Nif})", companyInfo.Nombre, companyInfo.Nif);

        if (!string.IsNullOrEmpty(companyInfo.NombreArchivoConfigVeriFactu))
        {
            logger.LogDebug("Estableciendo nombre de archivo de configuración: {Archivo}", companyInfo.NombreArchivoConfigVeriFactu);
            Settings.SetConfigFileName(companyInfo.NombreArchivoConfigVeriFactu);
        }

        // Recuperamos la configuración actual (Settings.Current) cargada por la librería.
        // EVITAMOS cambiar LogPath, InboxPath, OutboxPath, BlockchainPath e InvoicePath 
        // para prevenir 'InvalidOperationException' cuando las carpetas no están vacías.
        
        // 2. Sobrescribir con valores de la base de datos (prioridad tenant)
        if (companyInfo.CertificadoVeriFactu != null && !string.IsNullOrEmpty(companyInfo.CertificadoVeriFactu.FileName))
        {
            // Intentamos asegurar que las carpetas de trabajo sean relativas al proyecto si es posible
            // PERO solo las asignamos si están vacías o son diferentes a las actuales para evitar InvalidOperationException
            string projectBaseDir = Path.Combine(Directory.GetCurrentDirectory(), "VeriFactu", "Data");
            if (!Directory.Exists(projectBaseDir)) Directory.CreateDirectory(projectBaseDir);

            // Intentamos cambiar las rutas solo si el objeto Settings permite cambios (habitualmente al inicio)
            // O si detectamos que no están apuntando a nuestro proyecto.
            // NOTA: Si esto falla con InvalidOperationException, el catch lo capturará y seguiremos con las rutas por defecto.
            try
            {
                // NO cambiamos las rutas si no es estrictamente necesario, para evitar problemas de generación de XML
                // que el usuario reporta. Revertimos a la lógica más simple de solo asignar si están vacías.
                
                if (IsDirectoryEmpty(Settings.Current.InboxPath))
                    Settings.Current.InboxPath = Path.Combine(projectBaseDir, "Inbox");
                if (IsDirectoryEmpty(Settings.Current.OutboxPath))
                    Settings.Current.OutboxPath = Path.Combine(projectBaseDir, "Outbox");
                if (IsDirectoryEmpty(Settings.Current.LogPath))
                    Settings.Current.LogPath = Path.Combine(projectBaseDir, "Log");
                if (IsDirectoryEmpty(Settings.Current.BlockchainPath))
                    Settings.Current.BlockchainPath = Path.Combine(projectBaseDir, "Blockchains");
                if (IsDirectoryEmpty(Settings.Current.InvoicePath))
                    Settings.Current.InvoicePath = Path.Combine(projectBaseDir, "Invoices");
            }
            catch (Exception ex)
            {
                logger.LogWarning("No se pudieron cambiar todas las rutas de VeriFactu (posiblemente ya están en uso): {Message}", ex.Message);
            }

            // Guardamos el certificado en una ubicación estable pero específica si es posible, 
            // o simplemente usamos una subcarpeta para el certificado sin tocar las rutas de la librería.
            string baseDataDir = Path.Combine(Directory.GetCurrentDirectory(), "VeriFactu", "Data", companyInfo.Nif ?? "Global");
            if (!Directory.Exists(baseDataDir)) Directory.CreateDirectory(baseDataDir);

            string certPath = Path.Combine(baseDataDir, "cert_verifactu.pfx");
            logger.LogDebug("Procesando certificado. Ruta: {CertPath}", certPath);
            
            // Usar FileStream síncrono para asegurar el cierre inmediato del archivo
            using (var stream = new FileStream(certPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var ms = new MemoryStream();
                companyInfo.CertificadoVeriFactu.SaveToStream(ms);
                ms.WriteTo(stream);
                stream.Flush(true);
            }

            Settings.Current.CertificatePath = certPath;
            Settings.Current.CertificatePassword = companyInfo.PasswordCertificadoVeriFactu;
        }
        
        if (companyInfo.ActivarVeriFactu)
        {
            Settings.Current.VeriFactuEndPointPrefix = companyInfo.PrefijoUrlVeriFactu ?? Settings.Current.VeriFactuEndPointPrefix;
            Settings.Current.VeriFactuEndPointValidatePrefix = companyInfo.PrefijoUrlValidacionVeriFactu ?? Settings.Current.VeriFactuEndPointValidatePrefix;
        }
        else
        {
            logger.LogDebug("VeriFactu no está activado, no se definen los endpoints para que la librería asigne los de prueba automáticamente");
        }

        // Según documentación oficial, activamos el log
        Settings.Current.LoggingEnabled = true;

        if (Settings.Current.SistemaInformatico != null)
        {
            Settings.Current.SistemaInformatico.NombreSistemaInformatico = companyInfo.NombreSistemaVeriFactu ?? Settings.Current.SistemaInformatico.NombreSistemaInformatico;
            Settings.Current.SistemaInformatico.Version = companyInfo.VersionSistemaVeriFactu ?? Settings.Current.SistemaInformatico.Version;
            Settings.Current.SistemaInformatico.NombreRazon = companyInfo.NombreAdministradorSistemaVeriFactu ?? Settings.Current.SistemaInformatico.NombreRazon;
            Settings.Current.SistemaInformatico.NIF = companyInfo.NifAdministradorSistemaVeriFactu ?? Settings.Current.SistemaInformatico.NIF;
            
            string hardwareId = MachineIdentifier.GetMachineId();
            string suffix = companyInfo.ActivarVeriFactu ? "PROD" : "TEST";
            Settings.Current.SistemaInformatico.NumeroInstalacion = $"INST-{hardwareId}-{suffix}";
            logger.LogDebug("Sistema informático configurado. NumeroInstalacion: {NumeroInstalacion}", Settings.Current.SistemaInformatico.NumeroInstalacion);
        }

        // Logs de rutas de configuración para diagnóstico
        logger.LogInformation("Rutas de trabajo de VeriFactu (Settings.Current): \n" +
                              "- LogPath: {LogPath}\n" +
                              "- InboxPath: {InboxPath}\n" +
                              "- OutboxPath: {OutboxPath}\n" +
                              "- BlockchainPath: {BlockchainPath}\n" +
                              "- InvoicePath: {InvoicePath}\n" +
                              "- CertificatePath: {CertificatePath}",
            Settings.Current.LogPath, Settings.Current.InboxPath, Settings.Current.OutboxPath, 
            Settings.Current.BlockchainPath, Settings.Current.InvoicePath, Settings.Current.CertificatePath);

        // Guardo los cambios según la documentación
        logger.LogInformation("Guardando configuración de VeriFactu llamando a Settings.Save().");
        try
        {
            Settings.Save();
            logger.LogInformation("Configuración de VeriFactu guardada correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al guardar la configuración de VeriFactu");
            throw;
        }
    }

    private bool IsDirectoryEmpty(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return true;
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
        catch
        {
            return false;
        }
    }

    private IObjectSpace CreateHostObjectSpace(IServiceProvider sp)
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("ConnectionString");
        var xpoObjectSpaceProvider = new DevExpress.ExpressApp.Xpo.XPObjectSpaceProvider(connectionString ?? string.Empty, null, true, true);
        return xpoObjectSpaceProvider.CreateObjectSpace();
    }
}
