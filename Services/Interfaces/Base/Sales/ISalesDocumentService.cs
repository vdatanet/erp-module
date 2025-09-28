using erp.Module.BusinessObjects.Base.Sales;

namespace erp.Module.Services.Interfaces.Base.Sales;

public interface ISalesDocumentService
{
    void ComputeTotals(SalesDocument salesDocument);
}