// using erp.Module.BusinessObjects.Base.Ventas;
// using erp.Module.Services.Interfaces.Base.Sales;
//
// namespace erp.Module.Services.Implementations.Base.Sales;
//
// public class DocumentoVentaService : IDocumentoVentaService
// {
//     public void DeleteTaxes(DocumentoVenta salesDocument)
//     {
//         for (var i = salesDocument.Taxes.Count - 1; i >= 0; i--)
//             salesDocument.Taxes[i].Delete();
//     }
//     
//     public void RebuildTaxSummary(DocumentoVenta salesDocument)
//     {
//         var groups = salesDocument.Lines.SelectMany(l => l.Taxes)
//             .GroupBy(t => t.TipoImpuesto)
//             .Select(g => new
//             {
//                 TaxType = g.Key,
//                 BaseSum = g.Sum(x => x.TaxableAmount),
//                 AmountSum = g.Sum(x => x.TaxAmount)
//             })
//             .OrderBy(x => x.TaxType.Sequence)
//             .ToList();
//     
//         var newTaxes = groups.Select(g => new ImpuestoDocumentoVenta(salesDocument.Session)
//         {
//             DocumentoVenta = salesDocument,
//             TipoImpuesto = g.TaxType,
//             Sequence = g.TaxType.Sequence,
//             TaxableAmount = g.BaseSum,
//             TaxAmount = g.AmountSum
//         });
//
//         salesDocument.Taxes.AddRange(newTaxes);
//         
//         salesDocument.TaxableAmount = salesDocument.Lines.Sum(t => t.TaxableAmount);
//         salesDocument.TaxAmount = salesDocument.Lines.Sum(t => t.TaxAmount);
//         salesDocument.TotalAmount = salesDocument.TaxableAmount + salesDocument.TaxAmount;
//     }
// }