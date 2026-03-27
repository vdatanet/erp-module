using System.Collections.Concurrent;
using System.IO;
using VeriFactu.Business;
using VeriFactu.Config;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Helpers.Comun;

namespace erp.Module.Services.Facturacion;

public class VeriFactuAdapter : IVeriFactuAdapter
{
    private static readonly ConcurrentDictionary<string, object> TenantLocks = new();

    public VeriFactuResponse SendInvoice(Invoice veriFactuInvoice, InformacionEmpresa companyInfo)
    {
        ArgumentNullException.ThrowIfNull(veriFactuInvoice);
        ArgumentNullException.ThrowIfNull(companyInfo);

        string tenantNif = companyInfo.Nif ?? "Global";
        object tenantLock = TenantLocks.GetOrAdd(tenantNif, _ => new object());

        lock (tenantLock)
        {
            ConfigureVeriFactu(companyInfo);

            var invoiceEntry = new InvoiceEntry(veriFactuInvoice);
            invoiceEntry.Save();

            string? validationUrl = null;
            byte[]? qrData = null;

            if (invoiceEntry.Status == VeriFactuConstants.Correcto)
            {
                var registroAlta = veriFactuInvoice.GetRegistroAlta();
                validationUrl = registroAlta.GetUrlValidate();
                qrData = registroAlta.GetValidateQr();
            }

            return new VeriFactuResponse(
                invoiceEntry.Status,
                invoiceEntry.ErrorCode,
                invoiceEntry.Response,
                invoiceEntry.Xml,
                invoiceEntry.CSV,
                validationUrl,
                qrData);
        }
    }

    private void ConfigureVeriFactu(InformacionEmpresa companyInfo)
    {
        // 1. Determinar el nombre del archivo y la ruta (aislamiento por NIF)
        string nif = companyInfo.Nif ?? "Global";
        string fileName = companyInfo.NombreArchivoConfigVeriFactu ?? "config.json";

        // Asegurar que el archivo esté en una subcarpeta por NIF para aislamiento multi-tenant
        string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VeriFactuConfig", nif);
        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        string fullPath = Path.Combine(configDir, fileName);
        
        // Si el archivo no existe en la carpeta del tenant, intentar copiarlo del raíz si existe o dejar que el SDK lo cree
        if (!File.Exists(fullPath) && File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName)))
        {
            File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), fullPath);
        }

        Settings.SetConfigFileName(fullPath);

        // 2. Sobrescribir con valores de la base de datos (prioridad tenant)
        Settings.Current.CertificateSerial = companyInfo.SerieCertificadoVeriFactu ?? Settings.Current.CertificateSerial;
        Settings.Current.VeriFactuEndPointPrefix = companyInfo.PrefijoUrlVeriFactu ?? Settings.Current.VeriFactuEndPointPrefix;

        if (Settings.Current.SistemaInformatico != null)
        {
            Settings.Current.SistemaInformatico.NombreSistemaInformatico = companyInfo.NombreSistemaVeriFactu ?? Settings.Current.SistemaInformatico.NombreSistemaInformatico;
            Settings.Current.SistemaInformatico.Version = companyInfo.VersionSistemaVeriFactu ?? Settings.Current.SistemaInformatico.Version;
            Settings.Current.SistemaInformatico.NombreRazon = companyInfo.NombreAdministradorSistemaVeriFactu ?? Settings.Current.SistemaInformatico.NombreRazon;
            Settings.Current.SistemaInformatico.NIF = companyInfo.NifAdministradorSistemaVeriFactu ?? Settings.Current.SistemaInformatico.NIF;
            Settings.Current.SistemaInformatico.NumeroInstalacion = MachineIdentifier.GetMachineId();
        }

        Settings.Save();
    }
}
