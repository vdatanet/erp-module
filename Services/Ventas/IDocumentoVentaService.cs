using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Models.Ventas;

namespace erp.Module.Services.Ventas;

public interface IDocumentoVentaService
{
    TotalesDocumento CalcularTotales(DocumentoVenta documento);
    void RecalcularTotales(DocumentoVenta documento);
}
