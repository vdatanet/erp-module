using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace erp.Module.Helpers.Facturacion;

public static class HardwareFingerprintHelper
{
    public static string GetFingerprint()
    {
        var sb = new StringBuilder();
        sb.Append(Environment.MachineName);
        sb.Append(Environment.ProcessorCount);
        sb.Append(RuntimeInformation.OSDescription);
        
        // En una implementación real más robusta, usaríamos WMI en Windows 
        // o información de hardware específica del sistema operativo.
        // Para este ejemplo, combinamos varios factores disponibles en .NET Core.

        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToBase64String(bytes);
    }
}
