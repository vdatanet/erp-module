using erp.Module.BusinessObjects.Base.Ventas;

namespace erp.Module.Services.Interfaces.Base.Sales;

public interface ISalesLineService
{
    void ApplyProductSnapshot(LineaDocumentoVenta line);
    void SetTaxableAmount(LineaDocumentoVenta line);
    void RebuildTaxes(LineaDocumentoVenta line);
    void DeleteTaxes(LineaDocumentoVenta line);
}