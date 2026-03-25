using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Helpers.Comun;
using erp.Module.Models.Ventas;

namespace erp.Module.Services.Ventas;

public class DocumentoVentaService : IDocumentoVentaService
{
    public TotalesDocumento CalcularTotales(DocumentoVenta documento)
    {
        var lineas = documento.Lineas;
        var baseImponible = MoneyMath.RoundMoney(lineas.Sum(l => l.BaseImponible));
        
        var impuestos = documento.Impuestos;
        var importeImpuestos = MoneyMath.RoundMoney(impuestos.Sum(i => i.ImporteImpuestos));
        
        var importeTotal = baseImponible + importeImpuestos;

        var importeIva = MoneyMath.RoundMoney(impuestos.Where(i => !i.EsRetencion).Sum(i => i.ImporteImpuestos));
        var importeRetencion = MoneyMath.RoundMoney(impuestos.Where(i => i.EsRetencion).Sum(i => i.ImporteImpuestos));

        return new TotalesDocumento
        {
            BaseImponible = baseImponible,
            ImporteImpuestos = importeImpuestos,
            ImporteTotal = importeTotal,
            TotalBruto = baseImponible, // Simplificado, podría ser diferente si hay descuentos en cabecera
            TotalNeto = baseImponible,
            ImporteIva = importeIva,
            ImporteRetencion = importeRetencion,
            ImportePagado = documento.ImportePagado,
            ImportePendiente = importeTotal - documento.ImportePagado
        };
    }

    public void RecalcularTotales(DocumentoVenta documento)
    {
        BorrarResumenImpuestos(documento);
        ReconstruirResumenImpuestos(documento);
        
        var totales = CalcularTotales(documento);
        
        documento.BaseImponible = totales.BaseImponible;
        documento.ImporteImpuestos = totales.ImporteImpuestos;
        documento.ImporteTotal = totales.ImporteTotal;
        documento.TotalBruto = totales.TotalBruto;
        documento.TotalNeto = totales.TotalNeto;
        documento.ImporteIva = totales.ImporteIva;
        documento.ImporteRetencion = totales.ImporteRetencion;
        documento.ImportePendiente = totales.ImportePendiente;
    }

    private void BorrarResumenImpuestos(DocumentoVenta documento)
    {
        for (var i = documento.Impuestos.Count - 1; i >= 0; i--)
            documento.Impuestos[i].Delete();
    }

    private void ReconstruirResumenImpuestos(DocumentoVenta documento)
    {
        var groups = documento.Lineas.SelectMany(l => l.Impuestos)
            .Where(t => t.TipoImpuesto != null)
            .GroupBy(t => t.TipoImpuesto!)
            .Select(g => new
            {
                TaxType = g.Key,
                BaseSum = g.Sum(x => x.BaseImponible)
            })
            .OrderBy(x => x.TaxType.Secuencia)
            .ToList();

        foreach (var g in groups)
        {
            _ = new ImpuestoDocumentoVenta(documento.Session)
            {
                DocumentoVenta = documento,
                TipoImpuesto = g.TaxType,
                BaseImponible = g.BaseSum,
                ImporteImpuestos = AmountCalculator.GetTaxAmount(g.BaseSum, g.TaxType.Tipo, g.TaxType.EsRetencion)
            };
        }
    }
}
