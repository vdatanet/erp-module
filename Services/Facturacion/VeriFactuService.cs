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
using erp.Module.Models.VeriFactu;

namespace erp.Module.Services.Facturacion;

public class VeriFactuService(ILogger<VeriFactuService> logger, IVeriFactuAdapter veriFactuAdapter)
{
    public record SendResult(bool Success, string Message, string? ErrorCode = null);

    public async Task<SendResult> SendFacturaAsync(IObjectSpace objectSpace, FacturaBase invoice)
    {
        ArgumentNullException.ThrowIfNull(objectSpace);
        ArgumentNullException.ThrowIfNull(invoice);

        logger.LogInformation("VeriFactuService: Iniciando SendFacturaAsync para factura {Secuencia}", invoice.Secuencia);

        try
        {
            var companyInfo = objectSpace.FindObject<InformacionEmpresa>(null);
            if (companyInfo == null)
            {
                logger.LogWarning("VeriFactuService: No se encontró la configuración de la empresa.");
                return new SendResult(false, "No se encontró la configuración de la empresa.");
            }

            var validationResult = ValidateFiscalData(invoice, companyInfo);
            if (!validationResult.Success)
            {
                logger.LogWarning("VeriFactuService: Validación fiscal fallida para {Secuencia}: {Message}", invoice.Secuencia, validationResult.Message);
                return validationResult;
            }

            // Generar CorrelationId único para esta sesión de envío
            invoice.CorrelationId = Guid.NewGuid();

            var veriFactuInvoice = MapToVeriFactuInvoice(invoice, companyInfo);
            
            var startTime = InformacionEmpresaHelper.GetLocalTime(invoice.Session);
            logger.LogInformation("VeriFactuService: Llamando a VeriFactuAdapter para factura {Secuencia}", invoice.Secuencia);
            var response = await veriFactuAdapter.SendInvoiceAsync(veriFactuInvoice, invoice, companyInfo);
            var duration = InformacionEmpresaHelper.GetLocalTime(invoice.Session) - startTime;

            logger.LogInformation("VeriFactuService: Respuesta de VeriFactu para {Secuencia}: Status={Status}, ErrorCode={ErrorCode}", 
                invoice.Secuencia, response.Status, response.ErrorCode);

            UpdateInvoiceFromResponse(objectSpace, invoice, response, veriFactuInvoice, companyInfo);
            objectSpace.CommitChanges();

            if (response.Status == EstadoVeriFactu.Correcto || 
                response.Status == EstadoVeriFactu.EnviadaVeriFactu || 
                response.Status == EstadoVeriFactu.Pendiente)
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
            objectSpace.CommitChanges();
            return new SendResult(false, $"Error técnico: {ex.Message}");
        }
    }

    public async Task<SendResult> GetStatusAsync(IObjectSpace objectSpace, FacturaBase invoice)
    {
        if (string.IsNullOrEmpty(invoice.Uuid))
        {
            return new SendResult(false, "La factura no tiene UUID asignado.");
        }

        try
        {
            var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(((DevExpress.ExpressApp.Xpo.XPObjectSpace)objectSpace).Session);
            if (companyInfo == null)
            {
                return new SendResult(false, "No se encontró la configuración de la empresa.");
            }

            var response = await veriFactuAdapter.GetStatusAsync(invoice.Uuid, companyInfo);

            UpdateInvoiceFromResponse(objectSpace, invoice, response, null!, companyInfo, onlyUpdateStatus: true);
            objectSpace.CommitChanges();

            return new SendResult(true, $"Estado actualizado: {invoice.EstadoVeriFactu}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener estado de VeriFactu para {Secuencia}", invoice.Secuencia);
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

        if (companyInfo.VeriFactuProvider == VeriFactuProvider.Api && string.IsNullOrEmpty(companyInfo.ApiKeyVeriFactu))
        {
            return new SendResult(false, "La API Key de VeriFactu no está configurada en la empresa.");
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
            var impSnapshot = tax.Impuesto;
            if (impSnapshot != null && Enum.TryParse<Impuesto>(impSnapshot.ToString(), out _))
                return new SendResult(true, string.Empty);
            
            if (tax.TipoImpuesto?.Impuesto != null && Enum.TryParse<Impuesto>(tax.TipoImpuesto.Impuesto.ToString(), out _))
                return new SendResult(true, string.Empty);
        }

        return new SendResult(false, "La factura no tiene ningún impuesto con tipo VeriFactu asignado.");
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
        if (string.IsNullOrEmpty(invoice.Secuencia))
        {
            throw new InvalidOperationException($"La factura no tiene número de secuencia asignado.");
        }

        if (string.IsNullOrEmpty(companyInfo.Nif))
        {
            throw new InvalidOperationException("No se ha configurado el NIF de la empresa.");
        }

        var veriFactuFactura = new Invoice(invoice.Secuencia, invoice.Fecha, companyInfo.Nif)
        {
            InvoiceType = invoice.TipoFacturaAmigable,
            CorrectionType = invoice.TipoRectificativaAmigable,
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
                    if (Enum.TryParse<IDType>(invoice.TipoIdentificacionCliente.ToString(), out var idType))
                    {
                        veriFactuFactura.BuyerIDType = idType;
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
            // Omitir los impuestos que no tienen tipo VeriFactu indicado (o que no tienen el maestro de impuesto configurado)
            var impSnapshot = tax.Impuesto;
            Impuesto? impFinal = null;
            if (impSnapshot != null && Enum.TryParse<Impuesto>(impSnapshot.ToString(), out var impS))
            {
                impFinal = impS;
            }
            else if (tax.TipoImpuesto?.Impuesto != null && Enum.TryParse<Impuesto>(tax.TipoImpuesto.Impuesto.ToString(), out var impM))
            {
                impFinal = impM;
            }

            if (impFinal == null)
            {
                logger.LogInformation("VeriFactuService: Omitiendo impuesto para factura {Secuencia} por no tener tipo VeriFactu indicado (Base: {Base})", 
                    invoice.Secuencia, tax.BaseImponible);
                continue;
            }

            CalificacionOperacion opType = CalificacionOperacion.S1;
            // Primero intentamos obtener la calificación de la instantánea del impuesto en la factura
            if (tax.TipoOperacion != null && Enum.TryParse<CalificacionOperacion>(tax.TipoOperacion.ToString(), out var opSnapshot))
            {
                opType = opSnapshot;
            }
            // Si no está en la factura, intentamos obtenerlo del maestro (fallback)
            else if (tax.TipoImpuesto?.TipoOperacion != null && Enum.TryParse<CalificacionOperacion>(tax.TipoImpuesto.TipoOperacion.ToString(), out var opMaster))
            {
                opType = opMaster;
            }

            var taxItem = new TaxItem
            {
                TaxBase = tax.BaseImponible,
                TaxType = opType,
                TaxScheme = tax.RegimenFiscal ?? tax.TipoImpuesto?.RegimenFiscal ?? ClaveRegimen.General,
                Tax = impFinal.Value
            };

            var cauSnapshot = tax.CausaExencion;
            if (cauSnapshot != null && opType == CalificacionOperacion.S2 && Enum.TryParse<CausaExencion>(cauSnapshot.ToString(), out var cauS))
            {
                taxItem.TaxException = cauS;
            }
            else if (tax.TipoImpuesto?.CausaExencion != null && opType == CalificacionOperacion.S2 && Enum.TryParse<CausaExencion>(tax.TipoImpuesto.CausaExencion.ToString(), out var cauM))
            {
                taxItem.TaxException = cauM;
            }

            taxItem.TaxRate = tax.Tipo;
            taxItem.TaxAmount = tax.ImporteImpuestos;

            veriFactuFactura.TaxItems.Add(taxItem);
        }

        return veriFactuFactura;
    }

    private static bool RequiresBuyerData(FacturaBase invoice)
    {
        var invoiceType = invoice.TipoFactura.ToString();
        // Las facturas simplificadas (F2) y rectificativas de simplificadas (R5) no suelen requerir datos del comprador en VeriFactu.
        // Otros tipos como F1, R1, R2, R3, R4 son facturas completas o rectificativas de completas que sí requieren datos del comprador.
        return invoiceType != "F2" && invoiceType != "R5";
    }

    public void UpdateInvoiceFromResponse(IObjectSpace objectSpace, FacturaBase invoice, VeriFactuResponse veriFactuResponse,
        Invoice veriFactuFactura, InformacionEmpresa companyInfo, bool onlyUpdateStatus = false)
    {
        /*
        logger.LogInformation("VeriFactuService.UpdateInvoiceFromResponse: Invoice={Sequence}, Status={Status}, Provider={Provider}, OnlyUpdateStatus={OnlyUpdateStatus}",
            invoice.Secuencia, veriFactuResponse.Status, companyInfo.VeriFactuProvider, onlyUpdateStatus);
        */

        if (onlyUpdateStatus)
        {
            if (veriFactuResponse.Status != default)
            {
                // Para la Librería Local, 'Correcto', 'EnviadaVeriFactu' o 'Pendiente' se consideran estados de éxito inmediatos
                if (companyInfo.VeriFactuProvider == VeriFactuProvider.Library && 
                    (veriFactuResponse.Status == EstadoVeriFactu.Correcto || 
                     veriFactuResponse.Status == EstadoVeriFactu.EnviadaVeriFactu ||
                     veriFactuResponse.Status == EstadoVeriFactu.Pendiente))
                {
                    // logger.LogInformation("VeriFactuService.UpdateInvoiceFromResponse: Marcando factura {Sequence} como Correcto (Library Provider, OnlyUpdateStatus=true, Status={Status})", invoice.Secuencia, veriFactuResponse.Status);
                    invoice.EstadoVeriFactu = EstadoVeriFactu.Correcto;
                }
                else
                {
                    invoice.EstadoVeriFactu = veriFactuResponse.Status;
                }
            }
            return;
        }

        invoice.CodigoErrorEntradaFactura = veriFactuResponse.ErrorCode;
        
        // Respuesta técnica completa ya no se persiste en factura

        if (veriFactuResponse.Status == EstadoVeriFactu.Correcto || 
            veriFactuResponse.Status == EstadoVeriFactu.Pendiente ||
            veriFactuResponse.Status == EstadoVeriFactu.EnviadaVeriFactu ||
            veriFactuResponse.Status == EstadoVeriFactu.AceptadoConErrores ||
            veriFactuResponse.Status == EstadoVeriFactu.Incorrecto ||
            veriFactuResponse.Status == EstadoVeriFactu.Duplicado ||
            veriFactuResponse.Status == EstadoVeriFactu.Anulado ||
            veriFactuResponse.Status == EstadoVeriFactu.FacturaInexistente ||
            veriFactuResponse.Status == EstadoVeriFactu.NoRegistrado ||
            veriFactuResponse.Status == EstadoVeriFactu.ErrorServidorAEAT)
        {
            // Después de éxito al enviar verifactu, o al recibir un estado válido, actualizamos el estado
            // Para la API, si es un envío inicial (Correcto o EnviadaVeriFactu), lo dejamos en Pendiente para su posterior confirmación
            // Para la Librería Local, cualquier estado que no sea un error es 'Correcto' porque el envío es inmediato y aceptado por AEAT
            if (companyInfo.VeriFactuProvider == VeriFactuProvider.Api)
            {
                if (veriFactuResponse.Status == EstadoVeriFactu.Correcto || 
                    veriFactuResponse.Status == EstadoVeriFactu.EnviadaVeriFactu)
                {
                    invoice.EstadoVeriFactu = EstadoVeriFactu.Pendiente;
                }
                else
                {
                    invoice.EstadoVeriFactu = veriFactuResponse.Status;
                }
            }
            else // Provider Library
            {
                // Si la respuesta es de éxito, forzamos Correcto
                if (veriFactuResponse.Status == EstadoVeriFactu.Correcto || 
                    veriFactuResponse.Status == EstadoVeriFactu.EnviadaVeriFactu ||
                    veriFactuResponse.Status == EstadoVeriFactu.Pendiente)
                {
                    // logger.LogInformation("VeriFactuService.UpdateInvoiceFromResponse: Forzando factura {Sequence} como Correcto (Library Provider, Status={Status})", invoice.Secuencia, veriFactuResponse.Status);
                    invoice.EstadoVeriFactu = EstadoVeriFactu.Correcto;
                }
                else
                {
                    invoice.EstadoVeriFactu = veriFactuResponse.Status;
                }
            }

            if (invoice.EstadoFactura == EstadoFactura.Emitida)
            {
                invoice.StateMachine.CambiarA(EstadoFactura.Enviada);
            }
            invoice.UrlValidacion = veriFactuResponse.ValidationUrl;
            invoice.HuellaFiscal = veriFactuResponse.HuellaFiscal;
            invoice.Uuid = veriFactuResponse.Uuid;
            
            if (veriFactuResponse.QrData != null)
            {
                var qrMedia = objectSpace.CreateObject<MediaDataObject>();
                qrMedia.MediaData = veriFactuResponse.QrData;
                invoice.Qr = qrMedia;
            }
        }
        else
        {
            invoice.EstadoVeriFactu = EstadoVeriFactu.RechazadaVeriFactu;
        }

        // No persistimos detalle técnico de respuesta ni XML en la factura
    }
}