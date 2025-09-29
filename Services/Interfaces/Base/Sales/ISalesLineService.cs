using erp.Module.BusinessObjects.Base.Sales;

namespace erp.Module.Services.Interfaces.Base.Sales;

public interface ISalesLineService
{
    void ApplyProductSnapshot(SalesDocumentLine line);
}