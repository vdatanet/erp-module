using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.Services.Interfaces.Base.Sales;

namespace erp.Module.Services.Implementations.Base.Sales;

public class SalesDocumentService : ISalesDocumentService
{
    public void RebuildTaxSummary(SalesDocument salesDocument)
    {
        foreach (var row in salesDocument.Taxes.ToList())
            row.Delete();
    
        var groups = salesDocument.Lines.SelectMany(l => l.Taxes)
            .GroupBy(t => t.TaxKind)
            .Select(g => new
            {
                TaxType = g.Key,
                BaseSum = g.Sum(x => x.TaxableAmount),
                AmountSum = g.Sum(x => x.TaxAmount)
            })
            .OrderBy(x => x.TaxType.Sequence)
            .ToList();
    
        var newTaxes = groups.Select(g => new SalesDocumentTax(salesDocument.Session)
        {
            SalesDocument = salesDocument,
            TaxKind = g.TaxType,
            Sequence = g.TaxType.Sequence,
            TaxableAmount = g.BaseSum,
            TaxAmount = g.AmountSum
        });

        salesDocument.Taxes.AddRange(newTaxes);
        
        salesDocument.TaxableAmount = salesDocument.Taxes.Sum(t => t.TaxableAmount);
        salesDocument.TaxAmount = salesDocument.Taxes.Sum(t => t.TaxAmount);
        salesDocument.TotalAmount = salesDocument.TaxableAmount + salesDocument.TaxAmount;
    }
}