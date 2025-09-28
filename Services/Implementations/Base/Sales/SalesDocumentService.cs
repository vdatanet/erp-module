using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.Services.Interfaces.Base.Sales;

namespace erp.Module.Services.Implementations.Base.Sales;

public class SalesDocumentService : ISalesDocumentService
{
    public void ComputeTotals(SalesDocument salesDocument)
    {
        if (salesDocument == null) return;

        salesDocument.TaxableAmount = salesDocument.Lines.Sum(l => l.TaxableAmount);
        salesDocument.TaxAmount = salesDocument.Lines.Sum(l => l.TaxAmount);
        salesDocument.TotalAmount = salesDocument.Lines.Sum(l => l.TotalAmount);    
    }
}