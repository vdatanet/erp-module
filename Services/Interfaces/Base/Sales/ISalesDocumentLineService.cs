using erp.Module.BusinessObjects.Base.Sales;

namespace erp.Module.Services.Interfaces.Base.Sales;

public interface ISalesDocumentLineService
{
    void CalculateLineTaxableAmount(SalesDocumentLine line);
}