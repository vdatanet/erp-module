using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VeriFactu.Business;
using VeriFactu.Config;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Helpers.Comun;

namespace erp.Module.Services.Facturacion;

public class VeriFactuAdapter : IVeriFactuAdapter
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> TenantSemaphores = new();

    public async Task<VeriFactuResponse> SendInvoiceAsync(Invoice veriFactuInvoice, InformacionEmpresa companyInfo)
    {
        ArgumentNullException.ThrowIfNull(veriFactuInvoice);
        ArgumentNullException.ThrowIfNull(companyInfo);

        string tenantNif = companyInfo.Nif ?? "Global";
        var semaphore = TenantSemaphores.GetOrAdd(tenantNif, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            await ConfigureVeriFactuAsync(companyInfo);

            var invoiceEntry = new InvoiceEntry(veriFactuInvoice);
            
            // Si el SDK no tiene SaveAsync, usamos Task.Run para no bloquear el hilo de ejecución actual
            // En Blazor Server, esto libera el hilo de la UI mientras dura la operación de red.
            await Task.Run(() => invoiceEntry.Save());

            string? validationUrl = null;
            byte[]? qrData = null;

            var status = invoiceEntry.Status; 
            var errorCode = invoiceEntry.ErrorCode;
            var responseStr = null as string;

            if (invoiceEntry.Status == VeriFactuConstants.Correcto)
            {
                var registroAlta = veriFactuInvoice.GetRegistroAlta();
                validationUrl = registroAlta?.GetUrlValidate();
                qrData = registroAlta?.GetValidateQr();
            }

            return new VeriFactuResponse(
                status,
                errorCode,
                responseStr,
                null,
                null,
                validationUrl,
                qrData);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private async Task ConfigureVeriFactuAsync(InformacionEmpresa companyInfo)
    {
        // 1. Sobrescribir con valores de la base de datos (prioridad tenant)
        if (companyInfo.CertificadoVeriFactu != null && !string.IsNullOrEmpty(companyInfo.CertificadoVeriFactu.FileName))
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "VeriFactu", companyInfo.Nif ?? "Global");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
            string certPath = Path.Combine(tempDir, "cert_verifactu.pfx");
            
            // Usar FileStream asíncrono si es posible
            using (var stream = new FileStream(certPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                var ms = new MemoryStream();
                companyInfo.CertificadoVeriFactu.SaveToStream(ms);
                ms.Position = 0;
                await ms.CopyToAsync(stream);
            }

            Settings.Current.CertificatePath = certPath;
            Settings.Current.CertificatePassword = companyInfo.PasswordCertificadoVeriFactu;
        }
        
        Settings.Current.VeriFactuEndPointPrefix = companyInfo.PrefijoUrlVeriFactu ?? Settings.Current.VeriFactuEndPointPrefix;

        if (Settings.Current.SistemaInformatico != null)
        {
            Settings.Current.SistemaInformatico.NombreSistemaInformatico = companyInfo.NombreSistemaVeriFactu ?? Settings.Current.SistemaInformatico.NombreSistemaInformatico;
            Settings.Current.SistemaInformatico.Version = companyInfo.VersionSistemaVeriFactu ?? Settings.Current.SistemaInformatico.Version;
            Settings.Current.SistemaInformatico.NombreRazon = companyInfo.NombreAdministradorSistemaVeriFactu ?? Settings.Current.SistemaInformatico.NombreRazon;
            Settings.Current.SistemaInformatico.NIF = companyInfo.NifAdministradorSistemaVeriFactu ?? Settings.Current.SistemaInformatico.NIF;
            Settings.Current.SistemaInformatico.NumeroInstalacion = MachineIdentifier.GetMachineId();
        }
    }
}
