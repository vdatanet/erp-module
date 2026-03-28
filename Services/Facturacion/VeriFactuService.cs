using System.Text;
using System.Xml.Linq;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Helpers.Comun;
using erp.Module.Helpers.Contactos;
using Microsoft.Extensions.Logging;
using VeriFactu.Business;
using VeriFactu.Config;

namespace erp.Module.Services.Facturacion;

public class VeriFactuService(ILogger<VeriFactuService> logger, IVeriFactuAdapter veriFactuAdapter)
{
    public record SendResult(bool Success, string Message, string? ErrorCode = null);

    public SendResult SendFactura(IObjectSpace objectSpace, FacturaBase invoice)
    {
        ArgumentNullException.ThrowIfNull(objectSpace);
        ArgumentNullException.ThrowIfNull(invoice);

        try
        {
            var companyInfo = objectSpace.FindObject<InformacionEmpresa>(null);
            if (companyInfo == null)
                return new SendResult(false, "No se encontró la configuración de la empresa.");

            var validationResult = ValidateFiscalData(invoice, companyInfo);
            if (!validationResult.Success)
                return validationResult;

            PrepareInvoice(objectSpace, invoice);

            logger.LogInformation("Iniciando envío de factura {Secuencia} a VeriFactu. Tenant: {Tenant}",
                invoice.Secuencia, companyInfo.NombreArchivoConfigVeriFactu);

            var veriFactuInvoice = MapToVeriFactuInvoice(invoice, companyInfo);
            
            var startTime = DateTime.Now;
            var response = veriFactuAdapter.SendInvoice(veriFactuInvoice, companyInfo);
            var duration = DateTime.Now - startTime;

            logger.LogInformation("Respuesta recibida en {Duration}ms. Status: {Status}", 
                duration.TotalMilliseconds, response.Status);

            UpdateInvoiceFromResponse(objectSpace, invoice, response, veriFactuInvoice);
            objectSpace.CommitChanges();

            if (response.Status == VeriFactuConstants.Correcto)
            {
                return new SendResult(true, "Factura enviada correctamente.");
            }

            return new SendResult(false, 
                $"Error al enviar a VeriFactu: {response.Status} - {response.ErrorCode}", 
                response.ErrorCode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error técnico al enviar factura {Secuencia}", invoice.Secuencia);
            invoice.EstadoVeriFactu = EstadoVeriFactu.ErrorTecnico;
            invoice.RespuestaAgenciaTributaria = $"Error técnico: {ex.Message}";
            objectSpace.CommitChanges();
            return new SendResult(false, $"Error técnico: {ex.Message}");
        }
    }

    private SendResult ValidateFiscalData(FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        if (!invoice.EsValida())
            return new SendResult(false, "La factura no es válida. Revise que tenga Cliente (si aplica), Texto e Impuestos.");

        if (string.IsNullOrEmpty(companyInfo.Nombre) || string.IsNullOrEmpty(companyInfo.Nif))
            return new SendResult(false, "La información de la empresa (Nombre/NIF) es incompleta.");

        if (invoice.ImporteTotal == 0)
            return new SendResult(false, "No se puede enviar una factura con importe total cero.");

        if (invoice.Fecha > InformacionEmpresaHelper.GetLocalTime(invoice.Session))
             return new SendResult(false, "La fecha de la factura no puede ser posterior a la fecha actual.");

        foreach (var tax in invoice.Impuestos)
        {
            if (tax.TipoImpuesto == null)
                return new SendResult(false, "Hay impuestos en las líneas sin configuración técnica.");
            
            if (tax.TipoImpuesto.Impuesto == null)
                return new SendResult(false, $"El impuesto '{tax.TipoImpuesto.Nombre}' no tiene asignado el tipo VeriFactu.");
        }

        return new SendResult(true, string.Empty);
    }

    private void PrepareInvoice(IObjectSpace objectSpace, FacturaBase invoice)
    {
        invoice.Fecha = invoice.Fecha == DateTime.MinValue
            ? InformacionEmpresaHelper.GetLocalTime(objectSpace).Date
            : invoice.Fecha;

        if (string.IsNullOrEmpty(invoice.Secuencia))
            invoice.AsignarNumero();
    }

    private Invoice MapToVeriFactuInvoice(FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        var veriFactuFactura = new Invoice(invoice.Secuencia, invoice.Fecha, companyInfo.Nif)
        {
            InvoiceType = invoice.TipoFactura,
            SellerName = companyInfo.Nombre,
            BuyerID = invoice.Cliente?.Nif,
            BuyerName = invoice.Cliente?.Nombre,
            Text = invoice.Texto,
            TaxItems = []
        };

        foreach (var tax in invoice.Impuestos)
        {
            if (tax.TipoImpuesto == null) continue;

            var taxItem = new TaxItem
            {
                TaxRate = tax.Tipo,
                TaxBase = tax.BaseImponible,
                TaxAmount = tax.ImporteImpuestos,
                Tax = tax.TipoImpuesto.Impuesto ?? default,
                TaxType = tax.TipoImpuesto.TipoOperacion ?? default,
                TaxScheme = tax.TipoImpuesto.RegimenFiscal ?? default,
                TaxException = tax.TipoImpuesto.CausaExencion ?? default
            };

            veriFactuFactura.TaxItems.Add(taxItem);
        }

        return veriFactuFactura;
    }

    private void UpdateInvoiceFromResponse(IObjectSpace objectSpace, FacturaBase invoice, VeriFactuResponse veriFactuResponse,
        Invoice veriFactuFactura)
    {
        invoice.EstadoEntradaFactura = veriFactuResponse.Status;
        invoice.CodigoErrorEntradaFactura = veriFactuResponse.ErrorCode;
        
        // Conservar respuesta técnica completa
        invoice.RespuestaAgenciaTributaria = veriFactuResponse.Response;

        if (veriFactuResponse.Status == VeriFactuConstants.Correcto)
        {
            invoice.EstadoVeriFactu = EstadoVeriFactu.AceptadaVeriFactu;
            
            invoice.UrlValidacion = veriFactuResponse.ValidationUrl;
            invoice.Csv = veriFactuResponse.CSV;

            if (veriFactuResponse.QrData != null)
            {
                var qrMedia = objectSpace.CreateObject<MediaDataObject>();
                qrMedia.MediaData = veriFactuResponse.QrData;
                invoice.Qr = qrMedia;
            }
        }
        else if (veriFactuResponse.Status == VeriFactuConstants.Parcial)
        {
            invoice.EstadoVeriFactu = EstadoVeriFactu.EnviadaVeriFactu;
        }
        else
        {
            invoice.EstadoVeriFactu = EstadoVeriFactu.RechazadaVeriFactu;
        }

        try
        {
            if (!string.IsNullOrEmpty(veriFactuResponse.Response))
            {
                var response = XDocument.Parse(veriFactuResponse.Response);
                invoice.RespuestaAgenciaTributaria = response.ToString();
            }

            if (veriFactuResponse.Xml is { Length: > 0 })
            {
                var xmlString = Encoding.UTF8.GetString(veriFactuResponse.Xml);
                var xml = XDocument.Parse(xmlString);
                invoice.XmlAgenciaTributaria = xml.ToString();
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error procesando el detalle XML de la respuesta para factura {Secuencia}", invoice.Secuencia);
            invoice.RespuestaAgenciaTributaria += $"\nError processing XML detail: {ex.Message}";
        }
    }
}