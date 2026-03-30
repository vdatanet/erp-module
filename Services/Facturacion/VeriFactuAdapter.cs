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
    private static readonly SemaphoreSlim GlobalVeriFactuSemaphore = new(1, 1);

    public async Task<VeriFactuResponse> SendInvoiceAsync(Invoice veriFactuInvoice, FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        ArgumentNullException.ThrowIfNull(veriFactuInvoice);
        ArgumentNullException.ThrowIfNull(invoice);
        ArgumentNullException.ThrowIfNull(companyInfo);

        // Habilitar logging según documentación oficial
        Settings.Current.LoggingEnabled = true;

        var tenantProvider = companyInfo.Session.ServiceProvider?.GetService<DevExpress.ExpressApp.MultiTenancy.ITenantProvider>();
        var tenantId = tenantProvider?.TenantId ?? Guid.Empty;
        var correlationId = invoice.CorrelationId != Guid.Empty ? invoice.CorrelationId : Guid.NewGuid();
        var invoiceOid = invoice.Oid;

        string tenantNif = companyInfo.Nif ?? "Global";
        
        // Usamos un semáforo global para evitar que dos tenants distintos configuren 
        // y encolen facturas simultáneamente, ya que la librería usa 'Settings.Current' estático global.
        // Limitamos el bloqueo a la configuración y el encolado, liberándolo lo antes posible.
        await GlobalVeriFactuSemaphore.WaitAsync();
        try
        {
            await ConfigureVeriFactuAsync(companyInfo);

            // Validar que la configuración cargada en Settings.Current coincide con el emisor esperado
            // AEAT requiere que el NIF del emisor en el XML coincida con el del certificado/configuración.
            if (Settings.Current.SistemaInformatico != null)
            {
                if (!string.IsNullOrEmpty(veriFactuInvoice.SellerID) && 
                    !string.Equals(Settings.Current.SistemaInformatico.NIF, companyInfo.Nif, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning("VeriFactu: Discrepancia detectada entre NIF de empresa {EmpNif} y NIF en Settings.Current {SettNif}. Reintentando configuración.", 
                        companyInfo.Nif, Settings.Current.SistemaInformatico.NIF);
                    await ConfigureVeriFactuAsync(companyInfo);
                }
            }

            var invoiceEntry = new TenantAwareInvoiceEntry(veriFactuInvoice, tenantId, correlationId);
            
            // Crear registro de auditoría preventivo en el HOST antes de añadir a la cola
            try
            {
                var sp = companyInfo.Session.ServiceProvider;
                if (sp != null)
                {
                    using var hostOS = CreateHostObjectSpace(sp);
                    var audit = hostOS.CreateObject<VeriFactuAudit>();
                    audit.TenantId = tenantId;
                    audit.CorrelationId = correlationId;
                    audit.InvoiceOid = invoiceOid;
                    audit.InvoiceId = veriFactuInvoice.InvoiceID ?? string.Empty;
                    audit.NifEmisor = veriFactuInvoice.SellerID ?? string.Empty;
                    audit.NumeroSerie = companyInfo.PrefijoFacturasVentaPorDefecto ?? string.Empty;
                    audit.ConfigName = companyInfo.NombreArchivoConfigVeriFactu ?? "default";
                    audit.BatchId = string.Empty; 
                    audit.EstadoEnvio = "Encolada";
                    audit.FechaEnvio = InformacionEmpresaHelper.GetLocalTime(companyInfo.Session);
                    hostOS.CommitChanges();
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error al crear VeriFactuAudit para {InvoiceID}", veriFactuInvoice.InvoiceID);
            }

            if (InvoiceQueue.ActiveInvoiceQueue != null)
            {
                InvoiceQueue.ActiveInvoiceQueue.Add(invoiceEntry);
            }
            else
            {
                throw new InvalidOperationException("ActiveInvoiceQueue no está inicializado.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error durante el encolado VeriFactu para {InvoiceID}", veriFactuInvoice.InvoiceID);
            throw;
        }
        finally
        {
            GlobalVeriFactuSemaphore.Release();
        }

        return new VeriFactuResponse(
            VeriFactuConstants.PendienteVeriFactu,
            null,
            "Enviado a la cola de procesamiento de VeriFactu",
            null,
            null,
            null,
            null);
    }

    private async Task ConfigureVeriFactuAsync(InformacionEmpresa companyInfo)
    {
        if (!string.IsNullOrEmpty(companyInfo.NombreArchivoConfigVeriFactu))
        {
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
        }

        // Según documentación oficial, activamos el log
        Settings.Current.LoggingEnabled = true;

        try
        {
            Settings.Save();
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
