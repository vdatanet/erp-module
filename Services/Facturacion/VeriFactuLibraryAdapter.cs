using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Models.VeriFactu;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VeriFactu.Business;
using LibInvoice = VeriFactu.Business.Invoice;
using LibTaxItem = VeriFactu.Business.TaxItem;

namespace erp.Module.Services.Facturacion;

public class VeriFactuLibraryAdapter(ILogger<VeriFactuLibraryAdapter> logger, IHttpContextAccessor? httpContextAccessor = null) : IVeriFactuAdapter
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<VeriFactuResponse> SendInvoiceAsync(erp.Module.Models.VeriFactu.Invoice veriFactuInvoice, FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        return await ExecuteInContextAsync(async () =>
        {
            // logger.LogInformation("VeriFactuLibraryAdapter: Enviando factura {Sequence} mediante librería local", veriFactuInvoice.Sequence);

            var libInvoice = CreateLibraryInvoice(veriFactuInvoice);
            var invoiceEntry = new InvoiceEntry(libInvoice);
            
            invoiceEntry.Save();
            
            string? validationUrl = null;
            byte[]? qrData = null;

            try
            {
                dynamic registro = ((dynamic)libInvoice).GetRegistroAlta();
                if (registro != null)
                {
                    validationUrl = registro.GetUrlValidate();
                    qrData = registro.GetValidateQr();
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "VeriFactuLibraryAdapter: No se pudo obtener RegistroAlta o sus datos (Url/QR) de la factura {Sequence}.", veriFactuInvoice.Sequence);
            }
            
            // logger.LogInformation("VeriFactuLibraryAdapter: Resultado para factura {Sequence}: Status={Status}, ErrorCode={ErrorCode}, ErrorDescription={ErrorDescription}",
            //    veriFactuInvoice.Sequence, invoiceEntry.Status, invoiceEntry.ErrorCode, invoiceEntry.ErrorDescription);

            return MapResponse(invoiceEntry, validationUrl, qrData);
        }, companyInfo, veriFactuInvoice.Sequence);
    }

    public async Task<VeriFactuResponse> GetStatusAsync(string uuid, InformacionEmpresa companyInfo)
    {
        return await ExecuteInContextAsync(async () =>
        {
            // logger.LogInformation("VeriFactuLibraryAdapter: Consultando estado para UUID {Uuid} (Simulado)", uuid);

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
                logger.LogDebug(ex, "VeriFactuLibraryAdapter: No se pudo asignar TipoRectificativa a la factura {Sequence}. Es posible que esta propiedad no esté disponible en la versión de la librería cargada.", veriFactuInvoice.Sequence);
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

    private VeriFactuResponse MapResponse(InvoiceEntry invoiceEntry, string? validationUrl = null, byte[]? qrData = null)
    {
        EstadoVeriFactu status;
        if (invoiceEntry.Status == "Correcto" || invoiceEntry.Status == "Enviado")
        {
            status = EstadoVeriFactu.Correcto;
        }
        else if (invoiceEntry.Status == "Pendiente")
        {
            status = EstadoVeriFactu.Pendiente;
        }
        else
        {
            status = EstadoVeriFactu.Incorrecto;
        }
        
        /*
        logger.LogInformation("VeriFactuLibraryAdapter.MapResponse: InvoiceEntry.Status={LibStatus} -> EstadoVeriFactu={MappedStatus}, CSV={CSV}", 
            invoiceEntry.Status, status, invoiceEntry.CSV);
        */

        var response = new VeriFactuResponse
        {
            Status = status,
            ErrorMessage = (status == EstadoVeriFactu.Incorrecto)
                ? $"{invoiceEntry.ErrorCode}: {invoiceEntry.ErrorDescription}"
                : null,
            ErrorCode = invoiceEntry.ErrorCode,
            RawResponse = invoiceEntry.Response,
            Uuid = invoiceEntry.CSV,
            ValidationUrl = validationUrl,
            QrData = qrData
        };

        // Si no vinieron por parámetros, intentamos extraer QR y URL de validación si están disponibles en la librería
        if (string.IsNullOrEmpty(response.ValidationUrl) || response.QrData == null)
        {
            try
            {
                dynamic entry = invoiceEntry;
                if (string.IsNullOrEmpty(response.ValidationUrl))
                {
                    response.ValidationUrl = entry.ValidationUrl;
                    if (string.IsNullOrEmpty(response.ValidationUrl)) response.ValidationUrl = entry.UrlValidacion;
                }

                if (response.QrData == null)
                {
                    response.QrData = entry.QrData;
                    if (response.QrData == null) response.QrData = entry.QR;
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "VeriFactuLibraryAdapter: Las propiedades de QR o URL de validación no están disponibles dinámicamente en InvoiceEntry.");
            }
        }

        return response;
    }

    private async Task<VeriFactuResponse> ExecuteInContextAsync(Func<Task<VeriFactuResponse>> action, InformacionEmpresa companyInfo, string identifier)
    {
        try
        {
            using var scope = await VeriFactuConfigScope.BeginAsync(companyInfo, _semaphore, logger, httpContextAccessor);
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
