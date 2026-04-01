using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Models.VeriFactu;
using Microsoft.Extensions.Logging;
using VeriFactu.Business;
using LibInvoice = VeriFactu.Business.Invoice;
using LibTaxItem = VeriFactu.Business.TaxItem;

namespace erp.Module.Services.Facturacion;

public class VeriFactuLibraryAdapter(ILogger<VeriFactuLibraryAdapter> logger) : IVeriFactuAdapter
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<VeriFactuResponse> SendInvoiceAsync(erp.Module.Models.VeriFactu.Invoice veriFactuInvoice, FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        return await ExecuteInContextAsync(async () =>
        {
            logger.LogInformation("VeriFactuLibraryAdapter: Enviando factura {Sequence} mediante librería local", veriFactuInvoice.Sequence);

            var libInvoice = CreateLibraryInvoice(veriFactuInvoice);
            var invoiceEntry = new InvoiceEntry(libInvoice);
            
            invoiceEntry.Save();
            
            logger.LogInformation("VeriFactuLibraryAdapter: Resultado para factura {Sequence}: Status={Status}, ErrorCode={ErrorCode}, ErrorDescription={ErrorDescription}",
                veriFactuInvoice.Sequence, invoiceEntry.Status, invoiceEntry.ErrorCode, invoiceEntry.ErrorDescription);

            return MapResponse(invoiceEntry);
        }, companyInfo, veriFactuInvoice.Sequence);
    }

    public async Task<VeriFactuResponse> GetStatusAsync(string uuid, InformacionEmpresa companyInfo)
    {
        return await ExecuteInContextAsync(async () =>
        {
            logger.LogInformation("VeriFactuLibraryAdapter: Consultando estado para UUID {Uuid} (Simulado)", uuid);

            return new VeriFactuResponse
            {
                Status = EstadoVeriFactu.Correcto,
                Uuid = uuid
            };
        }, companyInfo, uuid);
    }

    private LibInvoice CreateLibraryInvoice(erp.Module.Models.VeriFactu.Invoice veriFactuInvoice)
    {
        var isSimplified = veriFactuInvoice.InvoiceType is TipoFacturaAmigable.F2 or TipoFacturaAmigable.R5;
        
        var libInvoice = new LibInvoice(veriFactuInvoice.Sequence, veriFactuInvoice.Date, veriFactuInvoice.SellerID)
        {
            InvoiceType = VeriFactuMapper.MapTipoFactura(veriFactuInvoice.InvoiceType, logger),
            SellerName = veriFactuInvoice.SellerName,
            BuyerID = isSimplified ? null : veriFactuInvoice.BuyerID,
            BuyerName = isSimplified ? null : veriFactuInvoice.BuyerName,
            Text = veriFactuInvoice.Text,
            TaxItems = []
        };

        // Asignación de TipoRectificativa con dynamic para seguridad frente a la librería externa
        if (veriFactuInvoice.CorrectionType.HasValue)
        {
            try
            {
                ((dynamic)libInvoice).TipoRectificativa = VeriFactuMapper.MapTipoRectificativa(veriFactuInvoice.CorrectionType.Value, logger);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "VeriFactuLibraryAdapter: No se pudo asignar TipoRectificativa a la factura {Sequence}. Es posible que esta propiedad no esté disponible en la versión de la librería cargada.", veriFactuInvoice.Sequence);
            }
        }

        foreach (var taxItem in veriFactuInvoice.TaxItems)
        {
            libInvoice.TaxItems.Add(CreateLibraryTaxItem(taxItem));
        }

        return libInvoice;
    }

    private LibTaxItem CreateLibraryTaxItem(erp.Module.Models.VeriFactu.TaxItem taxItem)
    {
        logger.LogInformation("VeriFactuLibraryAdapter: Procesando TaxItem - Base: {Base}, Rate: {Rate}, Amount: {Amount}, Tax: {Tax}, Type: {Type}, Scheme: {Scheme}",
            taxItem.TaxBase, taxItem.TaxRate, taxItem.TaxAmount, taxItem.Tax, taxItem.TaxType, taxItem.TaxScheme);

        var lTaxItem = new LibTaxItem
        {
            Tax = VeriFactuMapper.MapImpuesto(taxItem.Tax, logger),
            TaxType = VeriFactuMapper.MapCalificacion(taxItem.TaxType, logger),
            TaxScheme = VeriFactuMapper.MapRegimen(taxItem.TaxScheme, logger),
            TaxBase = taxItem.TaxBase,
            TaxRate = taxItem.TaxRate,
            TaxAmount = taxItem.TaxAmount
        };

        if (taxItem.TaxException.HasValue)
        {
            lTaxItem.TaxException = VeriFactuMapper.MapCausaExencion(taxItem.TaxException.Value, logger);
        }

        return lTaxItem;
    }

    private VeriFactuResponse MapResponse(InvoiceEntry invoiceEntry)
    {
        return new VeriFactuResponse
        {
            Status = invoiceEntry.Status == "Correcto" ? EstadoVeriFactu.Correcto : EstadoVeriFactu.Incorrecto,
            ErrorMessage = invoiceEntry.Status != "Correcto"
                ? $"{invoiceEntry.ErrorCode}: {invoiceEntry.ErrorDescription}"
                : null,
            ErrorCode = invoiceEntry.ErrorCode,
            RawResponse = invoiceEntry.Response,
            Uuid = invoiceEntry.CSV
        };
    }

    private async Task<VeriFactuResponse> ExecuteInContextAsync(Func<Task<VeriFactuResponse>> action, InformacionEmpresa companyInfo, string identifier)
    {
        try
        {
            using var scope = await VeriFactuConfigScope.BeginAsync(companyInfo, _semaphore, logger);
            return await action();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error en operación VeriFactu para {Identifier}", identifier);
            return new VeriFactuResponse
            {
                Status = EstadoVeriFactu.ErrorTecnico,
                ErrorMessage = ex.Message
            };
        }
    }
}
