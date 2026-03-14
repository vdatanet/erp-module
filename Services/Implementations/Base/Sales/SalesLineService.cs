// using erp.Module.BusinessObjects.Base.Ventas;
// using erp.Module.BusinessObjects.Helpers.Comun;
// using erp.Module.Services.Interfaces.Base.Sales;
//
// namespace erp.Module.Services.Implementations.Base.Sales;
//
// public class SalesLineService : ISalesLineService
// {
//     public void ApplyProductSnapshot(LineaDocumentoVenta line)
//     {
//         if (line.Product is null)
//         {
//             line.ProductName = null;
//             line.Notes = null;
//             line.Quantity = 0;
//             line.UnitPrice = 0m;
//             line.DiscountPercent = 0m;
//             return;
//         }
//
//         line.ProductName = line.Product.Name;
//         line.Notes = line.Product.Notes;
//         line.UnitPrice = line.Product.PriceList;
//
//         if (line.Quantity == 0m)
//             line.Quantity = 1m;
//
//         foreach (var tax in line.Product.SalesTaxes.OrderBy(t => t.Sequence))
//         {
//             _ = new LineaImpuestoDocumentoVenta(line.Session)
//             {
//                 LineaDocumentoVenta = line,
//                 TipoImpuesto = tax
//             };
//         }
//     }
//
//     public void SetTaxableAmount(LineaDocumentoVenta line)
//     {
//         line.TaxableAmount = AmountCalculator.GetTaxableAmount(line.Quantity, line.UnitPrice, line.DiscountPercent);
//     }
//
//     public void RebuildTaxes(LineaDocumentoVenta line)
//     {
//         foreach (var tax in line.Taxes)
//         {
//             tax.TaxableAmount = line.TaxableAmount;
//             tax.TaxAmount =
//                 AmountCalculator.GetTaxAmount(tax.TaxableAmount, tax.TipoImpuesto.Rate, tax.TipoImpuesto.IsWithHolding);
//         }
//
//         line.TaxAmount = line.Taxes.Sum(t => t.TaxAmount);
//         line.TotalAmount = line.TaxableAmount + line.TaxAmount;
//     }
//
//     public void DeleteTaxes(LineaDocumentoVenta line)
//     {
//         for (var i = line.Taxes.Count - 1; i >= 0; i--)
//             line.Taxes[i].Delete();
//     }
// }