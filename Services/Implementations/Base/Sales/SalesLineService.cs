using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.Services.Interfaces.Base.Sales;

namespace erp.Module.Services.Implementations.Base.Sales;

public class SalesLineService : ISalesLineService
{
    public void ApplyProductSnapshot(SalesDocumentLine line)
    {
        //if (line is null) return;

        //decimal gross = line.Quantity * line.UnitPrice;
        //decimal discount = (line.DiscountPercent / 100m) * gross;
        //line.TaxableAmount = Math.Round(gross - discount, 2);
    }
}