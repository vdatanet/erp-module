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
            
            // Ejecutamos Save() de forma síncrona dentro del hilo protegido por el semáforo.
            // Esto asegura que Settings.Current no cambie entre la configuración y el envío.
            // Aunque sea síncrono, el impacto en la UI es mínimo ya que solo bloquea un hilo por tenant.
            invoiceEntry.Save();

            string? validationUrl = null;
            byte[]? qrData = null;

            var status = invoiceEntry.Status; 
            var errorCode = invoiceEntry.ErrorCode;
            var responseStr = invoiceEntry.Response;
            var xml = invoiceEntry.Xml;
            var csv = invoiceEntry.CSV;

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
                xml,
                csv,
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
        if (!string.IsNullOrEmpty(companyInfo.NombreArchivoConfigVeriFactu))
        {
            Settings.SetConfigFileName(companyInfo.NombreArchivoConfigVeriFactu);
        }

        // 1. Sobrescribir con valores de la base de datos (prioridad tenant)
        if (companyInfo.CertificadoVeriFactu != null && !string.IsNullOrEmpty(companyInfo.CertificadoVeriFactu.FileName))
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "VeriFactu", companyInfo.Nif ?? "Global");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
            string certPath = Path.Combine(tempDir, "cert_verifactu.pfx");
            
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
        
        Settings.Current.VeriFactuEndPointPrefix = companyInfo.PrefijoUrlVeriFactu ?? Settings.Current.VeriFactuEndPointPrefix;
        Settings.Current.VeriFactuEndPointValidatePrefix = companyInfo.PrefijoUrlValidacionVeriFactu ?? Settings.Current.VeriFactuEndPointValidatePrefix;
        
        if (!companyInfo.ActivarVeriFactu)
        {
            Settings.Current.VeriFactuEndPointPrefix = VeriFactuEndPointPrefixes.Prod;
            Settings.Current.VeriFactuEndPointValidatePrefix = VeriFactuEndPointPrefixes.ProdValidate;
        }

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
    }
}
