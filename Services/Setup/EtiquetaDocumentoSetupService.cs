using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Documentos;

namespace erp.Module.Services.Setup;

public class EtiquetaDocumentoSetupService(IObjectSpace objectSpace)
{
    public void CreateInitialData()
    {
        CreateEtiqueta("Importante", "#FF0000"); // Rojo
        CreateEtiqueta("Revisado", "#00FF00");   // Verde
        CreateEtiqueta("Pendiente", "#FFFF00");  // Amarillo
        CreateEtiqueta("Urgente", "#FF00FF");    // Magenta
        CreateEtiqueta("Contabilidad", "#0000FF"); // Azul
    }

    private void CreateEtiqueta(string nombre, string color)
    {
        var etiqueta = objectSpace.FirstOrDefault<EtiquetaDocumento>(e => e.Nombre == nombre);
        if (etiqueta == null)
        {
            etiqueta = objectSpace.CreateObject<EtiquetaDocumento>();
            etiqueta.Nombre = nombre;
            etiqueta.Color = color;
        }
    }
}
