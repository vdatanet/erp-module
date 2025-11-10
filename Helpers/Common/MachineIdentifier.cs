using System.Net.NetworkInformation;

namespace erp.Module.Helpers.Common;

public static class MachineIdentifier
{
    public static string GetMachineId()
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