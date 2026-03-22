using System.Reflection;
using System.Text.Json;
using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Auxiliares;

namespace erp.Module.Services.Setup;

public class PaisProvinciaPoblacionSetupService(IObjectSpace objectSpace)
{
    public void CreateInitialData()
    {
        var data = LoadGeographicData();
        if (data == null) return;

        var pais = objectSpace.FirstOrDefault<Pais>(p => p.Nombre == data.Pais);
        if (pais == null)
        {
            pais = objectSpace.CreateObject<Pais>();
            pais.Nombre = data.Pais;
        }

        foreach (var provinciaData in data.Provincias)
        {
            var provincia = objectSpace.FirstOrDefault<Provincia>(p => p.Nombre == provinciaData.Nombre && p.Pais != null && p.Pais.Oid == pais.Oid);
            if (provincia == null)
            {
                provincia = objectSpace.CreateObject<Provincia>();
                provincia.Nombre = provinciaData.Nombre;
                provincia.Pais = pais;
            }

            foreach (var nombrePoblacion in provinciaData.Poblaciones)
            {
                var poblacion = objectSpace.FirstOrDefault<Poblacion>(p => p.Nombre == nombrePoblacion && p.Provincia != null && p.Provincia.Oid == provincia.Oid);
                if (poblacion == null)
                {
                    poblacion = objectSpace.CreateObject<Poblacion>();
                    poblacion.Nombre = nombrePoblacion;
                    poblacion.Provincia = provincia;
                }
            }
        }

        objectSpace.CommitChanges();
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
