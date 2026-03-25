using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Configuraciones;
using System.Runtime.InteropServices;

namespace erp.Module.Services.Setup;

public class ZonaHorariaSetupService(IObjectSpace objectSpace)
{
    private IObjectSpace? _os;
    private IObjectSpace OS => _os ??= GetWorkingObjectSpace();

    private IObjectSpace GetWorkingObjectSpace()
    {
        if (objectSpace is CompositeObjectSpace compositeOS)
        {
            var result = compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os.IsKnownType(typeof(ZonaHoraria)));
            if (result != null) return result;

            var fallback = compositeOS.AdditionalObjectSpaces.FirstOrDefault();
            if (fallback != null) return fallback;
        }

        return objectSpace;
    }

    public void CreateInitialData()
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        var timeZones = new[]
        {
            new { Nombre = "(UTC-12:00) International Date Line West", WinId = "Dateline Standard Time", IanaId = "Etc/GMT+12" },
            new { Nombre = "(UTC-11:00) Coordinated Universal Time-11", WinId = "UTC-11", IanaId = "Etc/GMT+11" },
            new { Nombre = "(UTC-10:00) Hawaii", WinId = "Hawaiian Standard Time", IanaId = "Pacific/Honolulu" },
            new { Nombre = "(UTC-09:00) Alaska", WinId = "Alaskan Standard Time", IanaId = "America/Anchorage" },
            new { Nombre = "(UTC-08:00) Pacific Time (US & Canada)", WinId = "Pacific Standard Time", IanaId = "America/Los_Angeles" },
            new { Nombre = "(UTC-07:00) Mountain Time (US & Canada)", WinId = "Mountain Standard Time", IanaId = "America/Denver" },
            new { Nombre = "(UTC-06:00) Central Time (US & Canada)", WinId = "Central Standard Time", IanaId = "America/Chicago" },
            new { Nombre = "(UTC-05:00) Eastern Time (US & Canada)", WinId = "Eastern Standard Time", IanaId = "America/New_York" },
            new { Nombre = "(UTC-04:00) Atlantic Time (Canada)", WinId = "Atlantic Standard Time", IanaId = "America/Halifax" },
            new { Nombre = "(UTC-03:00) Buenos Aires", WinId = "Argentina Standard Time", IanaId = "America/Argentina/Buenos_Aires" },
            new { Nombre = "(UTC-02:00) Coordinated Universal Time-02", WinId = "UTC-02", IanaId = "Etc/GMT+2" },
            new { Nombre = "(UTC-01:00) Azores", WinId = "Azores Standard Time", IanaId = "Atlantic/Azores" },
            new { Nombre = "(UTC) Coordinated Universal Time", WinId = "UTC", IanaId = "Etc/UTC" },
            new { Nombre = "(UTC+00:00) Dublin, Edinburgh, Lisbon, London", WinId = "GMT Standard Time", IanaId = "Europe/London" },
            new { Nombre = "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna", WinId = "W. Europe Standard Time", IanaId = "Europe/Berlin" },
            new { Nombre = "(UTC+01:00) Belgrade, Bratislava, Budapest, Ljubljana, Prague", WinId = "Central Europe Standard Time", IanaId = "Europe/Budapest" },
            new { Nombre = "(UTC+01:00) Brussels, Copenhagen, Madrid, Paris", WinId = "Romance Standard Time", IanaId = "Europe/Paris" },
            new { Nombre = "(UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb", WinId = "Central European Standard Time", IanaId = "Europe/Warsaw" },
            new { Nombre = "(UTC+01:00) West Central Africa", WinId = "W. Central Africa Standard Time", IanaId = "Africa/Lagos" },
            new { Nombre = "(UTC+02:00) Amman", WinId = "Jordan Standard Time", IanaId = "Asia/Amman" },
            new { Nombre = "(UTC+02:00) Athens, Bucharest", WinId = "GTB Standard Time", IanaId = "Europe/Bucharest" },
            new { Nombre = "(UTC+02:00) Beirut", WinId = "Middle East Standard Time", IanaId = "Asia/Beirut" },
            new { Nombre = "(UTC+02:00) Cairo", WinId = "Egypt Standard Time", IanaId = "Africa/Cairo" },
            new { Nombre = "(UTC+02:00) Harare, Pretoria", WinId = "South Africa Standard Time", IanaId = "Africa/Johannesburg" },
            new { Nombre = "(UTC+02:00) Helsinki, Kyiv, Riga, Sofia, Tallinn, Vilnius", WinId = "FLE Standard Time", IanaId = "Europe/Helsinki" },
            new { Nombre = "(UTC+02:00) Jerusalem", WinId = "Israel Standard Time", IanaId = "Asia/Jerusalem" },
            new { Nombre = "(UTC+02:00) Windhoek", WinId = "Namibia Standard Time", IanaId = "Africa/Windhoek" },
            new { Nombre = "(UTC+03:00) Baghdad", WinId = "Arabic Standard Time", IanaId = "Asia/Baghdad" },
            new { Nombre = "(UTC+03:00) Kuwait, Riyadh", WinId = "Arab Standard Time", IanaId = "Asia/Riyadh" },
            new { Nombre = "(UTC+03:00) Moscow, St. Petersburg, Volgograd", WinId = "Russian Standard Time", IanaId = "Europe/Moscow" },
            new { Nombre = "(UTC+03:00) Nairobi", WinId = "E. Africa Standard Time", IanaId = "Africa/Nairobi" },
            new { Nombre = "(UTC+03:30) Tehran", WinId = "Iran Standard Time", IanaId = "Asia/Tehran" },
            new { Nombre = "(UTC+04:00) Abu Dhabi, Muscat", WinId = "Arabian Standard Time", IanaId = "Asia/Dubai" },
            new { Nombre = "(UTC+04:00) Baku", WinId = "Azerbaijan Standard Time", IanaId = "Asia/Baku" },
            new { Nombre = "(UTC+04:00) Port Louis", WinId = "Mauritius Standard Time", IanaId = "Indian/Mauritius" },
            new { Nombre = "(UTC+04:00) Tbilisi", WinId = "Georgian Standard Time", IanaId = "Asia/Tbilisi" },
            new { Nombre = "(UTC+04:00) Yerevan", WinId = "Caucasus Standard Time", IanaId = "Asia/Yerevan" },
            new { Nombre = "(UTC+04:30) Kabul", WinId = "Afghanistan Standard Time", IanaId = "Asia/Kabul" },
            new { Nombre = "(UTC+05:00) Ekaterinburg", WinId = "Ekaterinburg Standard Time", IanaId = "Asia/Yekaterinburg" },
            new { Nombre = "(UTC+05:00) Islamabad, Karachi", WinId = "Pakistan Standard Time", IanaId = "Asia/Karachi" },
            new { Nombre = "(UTC+05:00) Tashkent", WinId = "West Asia Standard Time", IanaId = "Asia/Tashkent" },
            new { Nombre = "(UTC+05:30) Chennai, Kolkata, Mumbai, New Delhi", WinId = "India Standard Time", IanaId = "Asia/Kolkata" },
            new { Nombre = "(UTC+05:30) Sri Jayawardenepura", WinId = "Sri Lanka Standard Time", IanaId = "Asia/Colombo" },
            new { Nombre = "(UTC+05:45) Kathmandu", WinId = "Nepal Standard Time", IanaId = "Asia/Kathmandu" },
            new { Nombre = "(UTC+06:00) Almaty, Novosibirsk", WinId = "N. Central Asia Standard Time", IanaId = "Asia/Novosibirsk" },
            new { Nombre = "(UTC+06:00) Astana, Dhaka", WinId = "Central Asia Standard Time", IanaId = "Asia/Dhaka" },
            new { Nombre = "(UTC+06:30) Yangon (Rangoon)", WinId = "Myanmar Standard Time", IanaId = "Asia/Yangon" },
            new { Nombre = "(UTC+07:00) Bangkok, Hanoi, Jakarta", WinId = "SE Asia Standard Time", IanaId = "Asia/Bangkok" },
            new { Nombre = "(UTC+07:00) Krasnoyarsk", WinId = "North Asia Standard Time", IanaId = "Asia/Krasnoyarsk" },
            new { Nombre = "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi", WinId = "China Standard Time", IanaId = "Asia/Shanghai" },
            new { Nombre = "(UTC+08:00) Irkutsk, Ulaan Bataar", WinId = "North Asia East Standard Time", IanaId = "Asia/Irkutsk" },
            new { Nombre = "(UTC+08:00) Kuala Lumpur, Singapore", WinId = "Singapore Standard Time", IanaId = "Asia/Singapore" },
            new { Nombre = "(UTC+08:00) Perth", WinId = "W. Australia Standard Time", IanaId = "Australia/Perth" },
            new { Nombre = "(UTC+08:00) Taipei", WinId = "Taipei Standard Time", IanaId = "Asia/Taipei" },
            new { Nombre = "(UTC+09:00) Osaka, Sapporo, Tokyo", WinId = "Tokyo Standard Time", IanaId = "Asia/Tokyo" },
            new { Nombre = "(UTC+09:00) Seoul", WinId = "Korea Standard Time", IanaId = "Asia/Seoul" },
            new { Nombre = "(UTC+09:00) Yakutsk", WinId = "Yakutsk Standard Time", IanaId = "Asia/Yakutsk" },
            new { Nombre = "(UTC+09:30) Adelaide", WinId = "Cen. Australia Standard Time", IanaId = "Australia/Adelaide" },
            new { Nombre = "(UTC+09:30) Darwin", WinId = "AUS Central Standard Time", IanaId = "Australia/Darwin" },
            new { Nombre = "(UTC+10:00) Brisbane", WinId = "E. Australia Standard Time", IanaId = "Australia/Brisbane" },
            new { Nombre = "(UTC+10:00) Canberra, Melbourne, Sydney", WinId = "AUS Eastern Standard Time", IanaId = "Australia/Sydney" },
            new { Nombre = "(UTC+10:00) Guam, Port Moresby", WinId = "West Pacific Standard Time", IanaId = "Pacific/Port_Moresby" },
            new { Nombre = "(UTC+10:00) Hobart", WinId = "Tasmania Standard Time", IanaId = "Australia/Hobart" },
            new { Nombre = "(UTC+10:00) Vladivostok", WinId = "Vladivostok Standard Time", IanaId = "Asia/Vladivostok" },
            new { Nombre = "(UTC+11:00) Magadan, Solomon Is., New Caledonia", WinId = "Central Pacific Standard Time", IanaId = "Pacific/Guadalcanal" },
            new { Nombre = "(UTC+12:00) Auckland, Wellington", WinId = "New Zealand Standard Time", IanaId = "Pacific/Auckland" },
            new { Nombre = "(UTC+12:00) Coordinated Universal Time+12", WinId = "UTC+12", IanaId = "Etc/GMT-12" },
            new { Nombre = "(UTC+12:00) Fiji", WinId = "Fiji Standard Time", IanaId = "Pacific/Fiji" },
            new { Nombre = "(UTC+13:00) Nuku'alofa", WinId = "Tonga Standard Time", IanaId = "Pacific/Tongatapu" },
            // Especificas IANA que ya estaban
            new { Nombre = "Europe/Madrid", WinId = "Romance Standard Time", IanaId = "Europe/Madrid" },
            new { Nombre = "Europe/London", WinId = "GMT Standard Time", IanaId = "Europe/London" },
            new { Nombre = "Europe/Paris", WinId = "Romance Standard Time", IanaId = "Europe/Paris" },
            new { Nombre = "America/New_York", WinId = "Eastern Standard Time", IanaId = "America/New_York" },
            new { Nombre = "America/Los_Angeles", WinId = "Pacific Standard Time", IanaId = "America/Los_Angeles" },
            new { Nombre = "America/Chicago", WinId = "Central Standard Time", IanaId = "America/Chicago" },
            new { Nombre = "Asia/Tokyo", WinId = "Tokyo Standard Time", IanaId = "Asia/Tokyo" }
        };

        foreach (var tz in timeZones)
        {
            var targetId = isWindows ? tz.WinId : tz.IanaId;
            
            var existing = OS.FirstOrDefault<ZonaHoraria>(z => z.IdZonaHoraria == targetId);
            if (existing == null)
            {
                var zonaHoraria = OS.CreateObject<ZonaHoraria>();
                zonaHoraria.Nombre = tz.Nombre;
                zonaHoraria.IdZonaHoraria = targetId;
                zonaHoraria.Activo = true;
            }
        }

        OS.CommitChanges();
    }
}
