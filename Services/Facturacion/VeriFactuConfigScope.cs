using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Facturacion;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VeriFactu.Config;

using erp.Module.Helpers.Facturacion;

namespace erp.Module.Services.Facturacion;

public sealed class VeriFactuConfigScope : IDisposable
{
    private readonly ILogger? _logger;
    private readonly string? _tempCertificatePath;
    private readonly SemaphoreSlim _semaphore;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private bool _disposed;

    private VeriFactuConfigScope(InformacionEmpresa companyInfo, SemaphoreSlim semaphore, ILogger? logger, IHttpContextAccessor? httpContextAccessor)
    {
        _logger = logger;
        _semaphore = semaphore;
        _httpContextAccessor = httpContextAccessor;

        // 1. Configurar Settings para el tenant actual
        Settings.SetConfigFileName(companyInfo.ConfiguracionVeriFactuLibrary);

        // 2. Extraer certificado a archivo temporal si existe
        if (companyInfo.CertificadoVeriFactu != null)
        {
            _tempCertificatePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{companyInfo.CertificadoVeriFactu.FileName}");
            using (var fs = new FileStream(_tempCertificatePath, FileMode.Create, FileAccess.Write))
            {
                companyInfo.CertificadoVeriFactu.SaveToStream(fs);
            }
        }

        // 3. Establecer nuevos valores en Settings.Current
        Settings.Current.CertificatePath = _tempCertificatePath;
        Settings.Current.CertificatePassword = companyInfo.PasswordCertificadoVeriFactu;
        Settings.Current.SistemaInformatico.NombreSistemaInformatico = companyInfo.VeriFactuNombreSistemaInformatico;
        Settings.Current.SistemaInformatico.NombreRazon = companyInfo.VeriFactuNombreRazon;
        Settings.Current.SistemaInformatico.NIF = companyInfo.VeriFactuNif;
        Settings.Current.SistemaInformatico.Version = companyInfo.VeriFactuVersion;
        
        // NumeroInstalacion persistente por puesto
        var clientContext = GetClientContext();
        var actualFingerprint = HardwareFingerprintHelper.GetFingerprint(clientContext);
        string? numeroInstalacion = null;

        // Si es la huella del servidor (empresa), usamos el principal
        if (companyInfo.VeriFactuHardwareFingerprint == actualFingerprint || string.IsNullOrEmpty(companyInfo.VeriFactuHardwareFingerprint))
        {
            numeroInstalacion = companyInfo.VeriFactuNumeroInstalacion;
            if (string.IsNullOrEmpty(numeroInstalacion))
            {
                numeroInstalacion = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
                companyInfo.VeriFactuNumeroInstalacion = numeroInstalacion;
                companyInfo.VeriFactuHardwareFingerprint = actualFingerprint;
            }
        }
        else
        {
            // Es un cliente distinto al servidor, buscamos su puesto
            var puesto = companyInfo.VeriFactuPuestos.FirstOrDefault(p => p.HardwareFingerprint == actualFingerprint && p.Activo);
            if (puesto == null)
            {
                // Crear nuevo puesto para este cliente
                puesto = new VeriFactuPuesto(companyInfo.Session)
                {
                    Empresa = companyInfo,
                    HardwareFingerprint = actualFingerprint,
                    NumeroInstalacion = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                    NombrePuesto = Environment.MachineName
                };
                companyInfo.Session.Save(puesto);
            }
            numeroInstalacion = puesto.NumeroInstalacion;
        }

        Settings.Current.SistemaInformatico.NumeroInstalacion = numeroInstalacion;
        Settings.Save();
    }

    private string? GetClientContext()
    {
        try
        {
            var context = _httpContextAccessor?.HttpContext;
            if (context == null) return null;

            // Intentar obtener la IP del cliente
            var ip = context.Connection?.RemoteIpAddress?.ToString();
            
            // Intentar obtener el User-Agent para mayor distinción si hay NAT
            var userAgent = context.Request?.Headers["User-Agent"].ToString();

            if (string.IsNullOrEmpty(ip) && string.IsNullOrEmpty(userAgent)) return null;

            return $"{ip}|{userAgent}";
        }
        catch
        {
            return null;
        }
    }

    public static async Task<VeriFactuConfigScope> BeginAsync(InformacionEmpresa companyInfo, SemaphoreSlim semaphore, ILogger? logger = null, IHttpContextAccessor? httpContextAccessor = null)
    {
        await semaphore.WaitAsync();
        try
        {
            return new VeriFactuConfigScope(companyInfo, semaphore, logger, httpContextAccessor);
        }
        catch
        {
            semaphore.Release();
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            if (_tempCertificatePath != null && File.Exists(_tempCertificatePath))
            {
                File.Delete(_tempCertificatePath);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al eliminar el certificado temporal {Path}", _tempCertificatePath);
        }
        finally
        {
            _semaphore.Release();
            _disposed = true;
        }
    }
}
