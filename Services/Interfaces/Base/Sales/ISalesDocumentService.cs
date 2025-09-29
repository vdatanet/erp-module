using erp.Module.BusinessObjects.Base.Sales;

namespace erp.Module.Services.Interfaces.Base.Sales;

public interface ISalesDocumentService
{ 
    void DeleteTaxes(SalesDocument salesDocument);
    void RebuildTaxSummary(SalesDocument salesDocument);
}