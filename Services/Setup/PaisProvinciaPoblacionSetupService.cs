using System.Reflection;
using System.Text.Json;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using erp.Module.BusinessObjects.Auxiliares;

namespace erp.Module.Services.Setup;

public class PaisProvinciaPoblacionSetupService(IObjectSpace objectSpace)
{
    private IObjectSpace GetWorkingObjectSpace()
    {
        if (objectSpace is CompositeObjectSpace compositeOS)
        {
            // First attempt: look for an OS that explicitly knows the Pais type
            var result = compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os.IsKnownType(typeof(Pais)));
            if (result != null) return result;

            // Second attempt: if we are in a tenant-specific update, we might need to find any persistent OS
            // that is NOT the non-persistent one.
            var persistentOS = compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os is not NonPersistentObjectSpace);
            if (persistentOS != null) return persistentOS;

            // Fallback to the first available OS
            var fallback = compositeOS.AdditionalObjectSpaces.FirstOrDefault();
            if (fallback != null) return fallback;
        }

        return objectSpace;
    }

    public void CreateInitialData()
    {
        try 
        {
            var data = LoadGeographicData();
            if (data == null)
            {
                return;
            }

            var os = GetWorkingObjectSpace();
            
            // Check if the OS can actually handle the type before proceeding
            if (!os.IsKnownType(typeof(Pais)))
            {
                // If we still can't handle the type, we should probably skip to avoid the ArgumentException
                return;
            }

            var pais = os.FindObject<Pais>(CriteriaOperator.Parse("Nombre = ?", data.Pais));
            if (pais == null)
            {
                pais = os.CreateObject<Pais>();
                pais.Nombre = data.Pais;
                if (data.Pais == "España") pais.CodigoIso = "ES";
            }

            // Sembrar otros países de Europa
            SeedEuropeanCountries(os);

            foreach (var provinciaData in data.Provincias)
            {
                var provincia = os.FindObject<Provincia>(CriteriaOperator.Parse("Nombre = ? AND Pais.Oid = ?", provinciaData.Nombre, pais.Oid));
                if (provincia == null)
                {
                    provincia = os.CreateObject<Provincia>();
                    provincia.Nombre = provinciaData.Nombre;
                    provincia.Pais = pais;
                }

                foreach (var nombrePoblacion in provinciaData.Poblaciones)
                {
                    var poblacion = os.FindObject<Poblacion>(CriteriaOperator.Parse("Nombre = ? AND Provincia.Oid = ?", nombrePoblacion, provincia.Oid));
                    if (poblacion == null)
                    {
                        poblacion = os.CreateObject<Poblacion>();
                        poblacion.Nombre = nombrePoblacion;
                        poblacion.Provincia = provincia;
                    }
                }
            }

            os.CommitChanges();
        }
        catch (Exception)
        {
            // Log or handle the error gracefully to prevent application startup failure
            // In a real scenario, we might want to log this to a file or database
        }
    }

    private void SeedEuropeanCountries(IObjectSpace os)
    {
        var europeanCountries = new Dictionary<string, string>
        {
            { "Alemania", "DE" },
            { "Albania", "AL" },
            { "Andorra", "AD" },
            { "Austria", "AT" },
            { "Bélgica", "BE" },
            { "Bielorrusia", "BY" },
            { "Bosnia y Herzegovina", "BA" },
            { "Bulgaria", "BG" },
            { "Ciudad del Vaticano", "VA" },
            { "Croacia", "HR" },
            { "Chipre", "CY" },
            { "República Checa", "CZ" },
            { "Dinamarca", "DK" },
            { "Estonia", "EE" },
            { "Finlandia", "FI" },
            { "Francia", "FR" },
            { "Georgia", "GE" },
            { "Grecia", "GR" },
            { "Hungría", "HU" },
            { "Islandia", "IS" },
            { "Irlanda", "IE" },
            { "Italia", "IT" },
            { "Kazajistán", "KZ" },
            { "Letonia", "LV" },
            { "Liechtenstein", "LI" },
            { "Lituania", "LT" },
            { "Luxemburgo", "LU" },
            { "Malta", "MT" },
            { "Moldavia", "MD" },
            { "Mónaco", "MC" },
            { "Montenegro", "ME" },
            { "Países Bajos", "NL" },
            { "Macedonia del Norte", "MK" },
            { "Noruega", "NO" },
            { "Polonia", "PL" },
            { "Portugal", "PT" },
            { "Rumanía", "RO" },
            { "Rusia", "RU" },
            { "San Marino", "SM" },
            { "Serbia", "RS" },
            { "Eslovaquia", "SK" },
            { "Eslovenia", "SI" },
            { "Suecia", "SE" },
            { "Suiza", "CH" },
            { "Turquía", "TR" },
            { "Ucrania", "UA" },
            { "Reino Unido", "GB" }
        };

        foreach (var entry in europeanCountries)
        {
            var p = os.FindObject<Pais>(CriteriaOperator.Parse("Nombre = ?", entry.Key));
            if (p == null)
            {
                p = os.CreateObject<Pais>();
                p.Nombre = entry.Key;
                p.CodigoIso = entry.Value;
            }
            else if (string.IsNullOrEmpty(p.CodigoIso))
            {
                p.CodigoIso = entry.Value;
            }
        }
    }

    private GeographicData? LoadGeographicData()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "erp.Module.Resources.SpainGeographicData.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<GeographicData>(json);
    }

    private class GeographicData
    {
        public string Pais { get; set; } = string.Empty;
        public List<ProvinciaData> Provincias { get; set; } = [];
    }

    private class ProvinciaData
    {
        public string Nombre { get; set; } = string.Empty;
        public List<string> Poblaciones { get; set; } = [];
    }
}
