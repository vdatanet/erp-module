using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace erp.Module.Helpers.Comun;

public static class MachineIdentifier
{
    public static string GetMachineId()
    {
        string? hardwareId = null;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            hardwareId = GetWindowsHardwareId();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            hardwareId = GetLinuxHardwareId();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            hardwareId = GetMacHardwareId();
        }

        // Si no se pudo obtener un ID de hardware específico, 
        // usamos el fallback basado en MAC y nombre de equipo.
        if (string.IsNullOrEmpty(hardwareId))
        {
            hardwareId = GetFallbackId();
        }

        return hardwareId;
    }

    private static string? GetLinuxHardwareId()
    {
        try
        {
            // En Linux, /etc/machine-id suele ser el identificador más estable
            if (File.Exists("/etc/machine-id"))
            {
                return File.ReadAllText("/etc/machine-id").Trim();
            }
            if (File.Exists("/var/lib/dbus/machine-id"))
            {
                return File.ReadAllText("/var/lib/dbus/machine-id").Trim();
            }
        }
        catch
        {
            // Ignorar errores
        }

        return null;
    }

    private static string? GetMacHardwareId()
    {
        try
        {
            // En macOS, usamos ioreg para obtener el IOPlatformUUID
            var startInfo = new ProcessStartInfo
            {
                FileName = "ioreg",
                Arguments = "-rd1 -c IOPlatformExpertDevice",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Buscamos la línea que contiene "IOPlatformUUID"
                var match = System.Text.RegularExpressions.Regex.Match(output, "\"IOPlatformUUID\" = \"([^\"]+)\"");
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
        }
        catch
        {
            // Ignorar errores
        }

        return null;
    }

    private static string? GetWindowsHardwareId()
    {
        try
        {
            // Intentamos obtener el UUID de la placa base vía WMIC
            var startInfo = new ProcessStartInfo
            {
                FileName = "wmic",
                Arguments = "csproduct get uuid",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lines = output.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length > 1)
                {
                    string uuid = lines[1].Trim();
                    if (!string.IsNullOrEmpty(uuid) && uuid != "FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF")
                    {
                        return uuid;
                    }
                }
            }
        }
        catch
        {
            // Ignorar errores y volver al fallback
        }

        return null;
    }

    private static string GetFallbackId()
    {
        // Obtiene la MAC address de la primera tarjeta de red activa
        var mac = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                          nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Select(nic => nic.GetPhysicalAddress().ToString())
            .FirstOrDefault();

        // Combina con el nombre del equipo
        return $"{Environment.MachineName}-{mac}";
    }
}