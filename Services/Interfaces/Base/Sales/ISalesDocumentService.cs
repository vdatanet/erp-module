using erp.Module.BusinessObjects.Base.Ventas;

namespace erp.Module.Services.Interfaces.Base.Sales;

public interface IDocumentoVentaService
{ 
    void DeleteTaxes(DocumentoVenta salesDocument);
    void RebuildTaxSummary(DocumentoVenta salesDocument);
}