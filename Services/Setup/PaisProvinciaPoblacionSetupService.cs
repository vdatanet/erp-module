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
            var result = compositeOS.AdditionalObjectSpaces.FirstOrDefault(os => os.IsKnownType(typeof(Pais)));
            if (result != null) return result;
        }

        return objectSpace;
    }

    public void CreateInitialData()
    {
        var data = LoadGeographicData();
        if (data == null) return;

        var os = GetWorkingObjectSpace();

        var pais = os.FindObject<Pais>(CriteriaOperator.Parse("Nombre = ?", data.Pais));
        if (pais == null)
        {
            pais = os.CreateObject<Pais>();
            pais.Nombre = data.Pais;
        }

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
