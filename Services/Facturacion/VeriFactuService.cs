using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Helpers.Comun;
using erp.Module.Helpers.Contactos;
using Microsoft.Extensions.Logging;
using erp.Module.BusinessObjects.Base.Ventas;
using VeriFactu.Business;
using VeriFactu.Config;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.Services.Facturacion;

public class VeriFactuService(ILogger<VeriFactuService> logger, IVeriFactuAdapter veriFactuAdapter)
{
    public record SendResult(bool Success, string Message, string? ErrorCode = null);

    public async Task<SendResult> SendFacturaAsync(IObjectSpace objectSpace, FacturaBase invoice)
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

            // PrepareInvoice(objectSpace, invoice);

            var veriFactuInvoice = MapToVeriFactuInvoice(invoice, companyInfo);
            
            var startTime = InformacionEmpresaHelper.GetLocalTime(invoice.Session);
            var response = await veriFactuAdapter.SendInvoiceAsync(veriFactuInvoice, companyInfo);
            var duration = InformacionEmpresaHelper.GetLocalTime(invoice.Session) - startTime;

            UpdateInvoiceFromResponse(objectSpace, invoice, response, veriFactuInvoice);
            objectSpace.CommitChanges();

            if (response.Status == VeriFactuConstants.Correcto || response.Status == VeriFactuConstants.PendienteVeriFactu)
            {
                return new SendResult(true, "Factura enviada o encolada correctamente.");
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

        if (companyInfo.CertificadoVeriFactu == null || string.IsNullOrEmpty(companyInfo.CertificadoVeriFactu.FileName))
        {
            return new SendResult(false, "El certificado de VeriFactu (.pfx) no está cargado en la configuración de la empresa.");
        }

        // Para facturas que requieren cliente, validar que tengamos los datos en el snapshot o en el cliente
        var requiereBuyer = invoice.TipoFactura.ToString() == "F1";
        if (requiereBuyer &&
            string.IsNullOrEmpty(invoice.DocumentoIdentificacionCliente) &&
            string.IsNullOrEmpty(invoice.Cliente?.Nif))
        {
            return new SendResult(false, "El NIF del cliente es obligatorio para facturas de tipo F1.");
        }

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
        // La factura ya debe tener fecha y número desde el estado Emitida
        if (invoice.Fecha == DateTime.MinValue)
        {
            invoice.Fecha = InformacionEmpresaHelper.GetLocalTime(objectSpace).Date;
        }

        if (string.IsNullOrEmpty(invoice.Secuencia))
        {
            invoice.AsignarNumero();
        }
    }

    public Invoice MapToVeriFactuInvoice(FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        var veriFactuFactura = new Invoice(invoice.Secuencia, invoice.Fecha, companyInfo.Nif)
        {
            InvoiceType = invoice.TipoFactura,
            SellerName = companyInfo.Nombre,
            Text = invoice.Texto,
            TaxItems = []
        };

        if (RequiresBuyerData(invoice))
        {
            veriFactuFactura.BuyerID = !string.IsNullOrEmpty(invoice.DocumentoIdentificacionCliente)
                ? invoice.DocumentoIdentificacionCliente
                : invoice.Cliente?.Nif;

            veriFactuFactura.BuyerName = !string.IsNullOrEmpty(invoice.NombreCliente)
                ? invoice.NombreCliente
                : invoice.Cliente?.Nombre;

            if (invoice.TipoIdentificacionCliente != default)
            {
                // El log de envío anterior mostraba veriFactuInvoice.BuyerIDType
                // Intentamos un mapeo directo por valor numérico o nombre si el enum coincide
                try 
                {
                    // Usamos dynamic o object temporal para evitar errores de compilación directos si el tipo es problemático
                    // pero el objetivo es asignar el valor del enum del negocio al enum de la librería
                    var idTypeValue = (int)invoice.TipoIdentificacionCliente;
                    if (Enum.IsDefined(typeof(VeriFactu.Xml.Factu.IDType), idTypeValue))
                    {
                        veriFactuFactura.BuyerIDType = (VeriFactu.Xml.Factu.IDType)idTypeValue;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "No se pudo mapear IDType para la factura {Secuencia}", invoice.Secuencia);
                }
            }

            // Intentamos obtener el código ISO del snapshot de la factura (o del objeto País si no está en el snapshot)
            var isoCode = !string.IsNullOrEmpty(invoice.CodigoIsoPaisCliente)
                ? invoice.CodigoIsoPaisCliente
                : (invoice.PaisCliente?.CodigoIso ?? invoice.Cliente?.Pais?.CodigoIso);

            if (!string.IsNullOrEmpty(isoCode))
            {
                veriFactuFactura.BuyerCountryID = isoCode;
            }
            else
            {
                // Por defecto, si no hay ISO, asumimos ES (España) si el NIF tiene formato español 
                // o si simplemente no hay datos de país, ya que VeriFactu es para España.
                veriFactuFactura.BuyerCountryID = "ES";
            }
        }

        foreach (var tax in invoice.Impuestos)
        {
            if (tax.TipoImpuesto == null) continue;

            var opType = tax.TipoImpuesto.TipoOperacion != null
                ? (VeriFactu.Xml.Factu.Alta.CalificacionOperacion)(int)tax.TipoImpuesto.TipoOperacion
                : default;

            var taxItem = new TaxItem
            {
                TaxBase = tax.BaseImponible,
                Tax = tax.TipoImpuesto.Impuesto != null ? (VeriFactu.Xml.Factu.Impuesto)(int)tax.TipoImpuesto.Impuesto : default,
                TaxType = opType,
                TaxScheme = tax.TipoImpuesto.RegimenFiscal ?? default,
                TaxException = tax.TipoImpuesto.CausaExencion != null ? (VeriFactu.Xml.Factu.Alta.CausaExencion)(int)tax.TipoImpuesto.CausaExencion : default
            };

            // Cuando CalificacionOperacion sea “S2” TipoImpositivo y CuotaRepercutida deben ser 0
            if (opType == VeriFactu.Xml.Factu.Alta.CalificacionOperacion.S2)
            {
                taxItem.TaxRate = 0;
                taxItem.TaxAmount = 0;
            }
            else
            {
                taxItem.TaxRate = tax.Tipo;
                taxItem.TaxAmount = tax.ImporteImpuestos;
            }

            veriFactuFactura.TaxItems.Add(taxItem);
        }

        return veriFactuFactura;
    }

    private static bool RequiresBuyerData(FacturaBase invoice)
    {
        return invoice.TipoFactura.ToString() == "F1";
    }

    public void UpdateInvoiceFromResponse(IObjectSpace objectSpace, FacturaBase invoice, VeriFactuResponse veriFactuResponse,
        Invoice veriFactuFactura)
    {
        invoice.EstadoEntradaFactura = veriFactuResponse.Status;
        invoice.CodigoErrorEntradaFactura = veriFactuResponse.ErrorCode;
        
        // Conservar respuesta técnica completa
        invoice.RespuestaAgenciaTributaria = veriFactuResponse.Response;

        if (veriFactuResponse.Status == VeriFactuConstants.Correcto)
        {
            invoice.EstadoVeriFactu = EstadoVeriFactu.AceptadaVeriFactu;
            if (invoice.EstadoFactura == EstadoFactura.Emitida)
            {
                invoice.StateMachine.CambiarA(EstadoFactura.Enviada);
            }
            invoice.UrlValidacion = veriFactuResponse.ValidationUrl;
            invoice.Csv = veriFactuResponse.CSV;
            
            if (veriFactuResponse.QrData != null)
            {
                var qrMedia = objectSpace.CreateObject<MediaDataObject>();
                qrMedia.MediaData = veriFactuResponse.QrData;
                invoice.Qr = qrMedia;
            }
        }
        else if (veriFactuResponse.Status == VeriFactuConstants.PendienteVeriFactu)
        {
            invoice.EstadoVeriFactu = EstadoVeriFactu.PendienteVeriFactu;
            if (invoice.EstadoFactura == EstadoFactura.Emitida)
            {
                invoice.StateMachine.CambiarA(EstadoFactura.Enviada);
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
            if (!string.IsNullOrWhiteSpace(veriFactuResponse.Response))
            {
                var trimmedResponse = veriFactuResponse.Response.Trim();
                if (trimmedResponse.StartsWith('<'))
                {
                    try
                    {
                        var response = XDocument.Parse(trimmedResponse);
                        invoice.RespuestaAgenciaTributaria = response.ToString();
                    }
                    catch (XmlException)
                    {
                        // No es XML válido, mantenemos el texto original
                        invoice.RespuestaAgenciaTributaria = veriFactuResponse.Response;
                    }
                }
                else
                {
                    invoice.RespuestaAgenciaTributaria = veriFactuResponse.Response;
                }
            }

            if (veriFactuResponse.Xml is { Length: > 0 })
            {
                var xmlString = Encoding.UTF8.GetString(veriFactuResponse.Xml);
                if (!string.IsNullOrWhiteSpace(xmlString))
                {
                    var trimmedXml = xmlString.Trim();
                    if (trimmedXml.StartsWith('<'))
                    {
                        try
                        {
                            var xml = XDocument.Parse(trimmedXml);
                            invoice.XmlAgenciaTributaria = xml.ToString();
                        }
                        catch (XmlException)
                        {
                            invoice.XmlAgenciaTributaria = xmlString;
                        }
                    }
                    else
                    {
                        invoice.XmlAgenciaTributaria = xmlString;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error procesando el detalle XML de la respuesta para factura {Secuencia}", invoice.Secuencia);
            if (!string.IsNullOrEmpty(invoice.RespuestaAgenciaTributaria))
                invoice.RespuestaAgenciaTributaria += $"\n[Error processing XML detail: {ex.Message}]";
            else
                invoice.RespuestaAgenciaTributaria = $"Error processing XML: {ex.Message}";
        }
    }
}