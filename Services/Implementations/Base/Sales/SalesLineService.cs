using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.Services.Interfaces.Base.Sales;

namespace erp.Module.Services.Implementations.Base.Sales;

public class SalesLineService : ISalesLineService
{
    public void ApplyProductSnapshot(SalesDocumentLine line)
    {
        if (line.Product is null)
        {
            line.ProductName = null;
            line.Notes = null;
            line.Quantity = 0;
            line.UnitPrice = 0m;
            line.DiscountPercent = 0m;
            return;
        }

        line.ProductName = line.Product.Name;
        line.Notes = line.Product.Notes;
        line.UnitPrice = line.Product.PriceList;

        if (line.Quantity == 0m) 
            line.Quantity = 1m;
        
        foreach (var tax in line.Product.SalesTaxes.OrderBy(t => t.Sequence))
        {
            _ = new SalesDocumentLineTax(line.Session)
            {
                SalesDocumentLine = line,
                TaxKind = tax
            };
        }
    }

    public void DeleteAllTaxes(SalesDocumentLine line)
    {
        for (var i = line.Taxes.Count - 1; i >= 0; i--) 
            line.Taxes[i].Delete();
    }
}