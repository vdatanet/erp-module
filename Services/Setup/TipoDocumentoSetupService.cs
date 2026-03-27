using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Documentos;

namespace erp.Module.Services.Setup;

public class TipoDocumentoSetupService(IObjectSpace objectSpace)
{
    public void CreateInitialData()
    {
        CreateTipoDocumento("Factura de Venta", "Documentos relacionados con ventas a clientes");
        CreateTipoDocumento("Factura de Compra", "Documentos relacionados con compras a proveedores");
        CreateTipoDocumento("Producto", "Fichas técnicas, manuales o imágenes de productos");
        CreateTipoDocumento("Contacto", "Documentos de identidad, contratos o información de contactos");
        CreateTipoDocumento("Oportunidad", "Presupuestos, requisitos o información de oportunidades de negocio");
        CreateTipoDocumento("Tarea", "Documentación adjunta a tareas específicas");
        CreateTipoDocumento("General", "Documentos generales no clasificados");
    }

    private void CreateTipoDocumento(string nombre, string descripcion)
    {
        var tipo = objectSpace.FirstOrDefault<TipoDocumento>(t => t.Nombre == nombre);
        if (tipo == null)
        {
            tipo = objectSpace.CreateObject<TipoDocumento>();
            tipo.Nombre = nombre;
            tipo.Descripcion = descripcion;
        }
    }
}
