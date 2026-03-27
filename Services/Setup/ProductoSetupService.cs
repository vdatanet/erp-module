using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Productos;

namespace erp.Module.Services.Setup;

public class ProductoSetupService(IObjectSpace objectSpace)
{
    public void CreateInitialData()
    {
        CreateAtributosYPlantillas();
    }

    private void CreateAtributosYPlantillas()
    {
        // 1. Crear Atributos Maestros
        var atPotencia = GetOrCreateAtributo("Potencia", TipoDatoAtributo.Decimal, "W");
        var atVoltaje = GetOrCreateAtributo("Voltaje", TipoDatoAtributo.Decimal, "V");
        var atPeso = GetOrCreateAtributo("Peso", TipoDatoAtributo.Decimal, "kg");
        var atColor = GetOrCreateAtributo("Color", TipoDatoAtributo.TextoCorto);
        var atMaterial = GetOrCreateAtributo("Material", TipoDatoAtributo.TextoCorto);
        var atDimensiones = GetOrCreateAtributo("Dimensiones (L x A x F)", TipoDatoAtributo.TextoCorto, "cm");
        var atGarantia = GetOrCreateAtributo("Garantía Extendida", TipoDatoAtributo.Booleano);
        var atFechaFabricacion = GetOrCreateAtributo("Fecha de Fabricación", TipoDatoAtributo.Fecha);
        var atConsumo = GetOrCreateAtributo("Consumo Energético", TipoDatoAtributo.ListaSeleccion);

        if (atConsumo.Oid == Guid.Empty || atConsumo.Opciones.Count == 0)
        {
            CreateOpcionAtributo(atConsumo, "A+++", 1);
            CreateOpcionAtributo(atConsumo, "A++", 2);
            CreateOpcionAtributo(atConsumo, "A+", 3);
            CreateOpcionAtributo(atConsumo, "A", 4);
            CreateOpcionAtributo(atConsumo, "B", 5);
            CreateOpcionAtributo(atConsumo, "C", 6);
        }

        // 2. Crear Plantilla: Electrodomésticos
        var plantillaElectro = objectSpace.FirstOrDefault<PlantillaAtributo>(p => p.Nombre == "Electrodomésticos");
        if (plantillaElectro == null)
        {
            plantillaElectro = objectSpace.CreateObject<PlantillaAtributo>();
            plantillaElectro.Nombre = "Electrodomésticos";

            CreateLineaPlantilla(plantillaElectro, atPotencia, 10, "1200");
            CreateLineaPlantilla(plantillaElectro, atVoltaje, 20, "220");
            CreateLineaPlantilla(plantillaElectro, atConsumo, 30, "A++");
            CreateLineaPlantilla(plantillaElectro, atPeso, 40);
            CreateLineaPlantilla(plantillaElectro, atGarantia, 50, "False");
        }

        // 3. Crear Plantilla: Muebles
        var plantillaMuebles = objectSpace.FirstOrDefault<PlantillaAtributo>(p => p.Nombre == "Muebles");
        if (plantillaMuebles == null)
        {
            plantillaMuebles = objectSpace.CreateObject<PlantillaAtributo>();
            plantillaMuebles.Nombre = "Muebles";

            CreateLineaPlantilla(plantillaMuebles, atMaterial, 10, "Madera");
            CreateLineaPlantilla(plantillaMuebles, atColor, 20, "Blanco");
            CreateLineaPlantilla(plantillaMuebles, atDimensiones, 30);
            CreateLineaPlantilla(plantillaMuebles, atPeso, 40);
        }
    }

    private Atributo GetOrCreateAtributo(string nombre, TipoDatoAtributo tipo, string? unidad = null)
    {
        var atributo = objectSpace.FirstOrDefault<Atributo>(a => a.Nombre == nombre);
        if (atributo == null)
        {
            atributo = objectSpace.CreateObject<Atributo>();
            atributo.Nombre = nombre;
            atributo.TipoDato = tipo;
            atributo.UnidadMedida = unidad;
        }
        return atributo;
    }

    private void CreateOpcionAtributo(Atributo atributo, string valor, int orden)
    {
        var opcion = objectSpace.CreateObject<AtributoOpcion>();
        opcion.Atributo = atributo;
        opcion.Valor = valor;
        opcion.Orden = orden;
    }

    private void CreateLineaPlantilla(PlantillaAtributo plantilla, Atributo atributo, int orden, string? valorDefecto = null)
    {
        var linea = objectSpace.CreateObject<PlantillaAtributoLinea>();
        linea.Plantilla = plantilla;
        linea.Atributo = atributo;
        linea.Orden = orden;
        linea.ValorPorDefecto = valorDefecto;
    }
}
