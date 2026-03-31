using DevExpress.ExpressApp;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Models.VeriFactu;
using erp.Verifactu.Client;
using Newtonsoft.Json;

namespace erp.Module.Services.Facturacion;

public class VeriFactuAdapter(ILogger<VeriFactuAdapter> logger, VerifactuClient client) : IVeriFactuAdapter
{
    public async Task<VeriFactuResponse> SendInvoiceAsync(Invoice veriFactuInvoice, FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        try
        {
            logger.LogInformation("VeriFactuAdapter: Enviando factura {Secuencia} a la API", invoice.Secuencia);

            // Configurar API Key del cliente desde la configuración de la empresa
            client.ApiKey = companyInfo.ApiKeyVeriFactu;
            
            if (string.IsNullOrEmpty(client.ApiKey))
            {
                logger.LogWarning("VeriFactuAdapter: ApiKeyVeriFactu está vacía en la configuración de la empresa.");
            }
            else
            {
                var maskedKey = client.ApiKey.Length > 8 
                    ? client.ApiKey.Substring(0, 4) + "..." + client.ApiKey.Substring(client.ApiKey.Length - 4)
                    : "****";
                logger.LogInformation("VeriFactuAdapter: Usando API Key: {MaskedKey} (Longitud: {Length})", maskedKey, client.ApiKey.Length);
            }

            var body = MapToBody2(veriFactuInvoice, companyInfo);
            logger.LogInformation("VeriFactuAdapter: Payload a enviar para {Secuencia}: {Payload}", 
                invoice.Secuencia, JsonConvert.SerializeObject(body));
            
            // Según la actualización del usuario, usamos el endpoint individual "create"
            // Hemos implementado CreateAsync en la clase parcial VerifactuClient
            var apiResponse = await client.CreateAsync(body);
            
            // Log de la respuesta cruda para depuración si falla o para ver qué devuelve
            logger.LogInformation("VeriFactuAdapter: Respuesta recibida de la API para {Secuencia}: {Response}", 
                invoice.Secuencia, JsonConvert.SerializeObject(apiResponse));
            
            if (apiResponse != null)
            {
                logger.LogInformation("VeriFactuAdapter: Factura {Secuencia} procesada. UUID: {Uuid}, Estado: {Estado}", 
                    invoice.Secuencia, apiResponse.Uuid, apiResponse.Estado);

                var status = apiResponse.Estado == "Pendiente" 
                    ? EstadoVeriFactu.Pendiente 
                    : EstadoVeriFactu.AceptadaVeriFactu;

                return new VeriFactuResponse
                {
                    Status = status,
                    RawResponse = JsonConvert.SerializeObject(apiResponse),
                    ValidationUrl = apiResponse.Url,
                    HuellaFiscal = apiResponse.Huella,
                    QrData = !string.IsNullOrEmpty(apiResponse.Qr) ? Convert.FromBase64String(apiResponse.Qr) : null,
                    BatchId = apiResponse.Uuid
                };
            }

            return new VeriFactuResponse
            {
                Status = EstadoVeriFactu.RechazadaVeriFactu,
                ErrorMessage = "La API no devolvió una respuesta válida.",
                RawResponse = "Respuesta vacía de BulkAsync"
            };
        }
        catch (ApiException ex)
        {
            logger.LogError(ex, "Error de la API VeriFactu para factura {Secuencia}: {Response}", invoice.Secuencia, ex.Response);
            return new VeriFactuResponse
            {
                Status = EstadoVeriFactu.RechazadaVeriFactu,
                ErrorCode = ex.StatusCode.ToString(),
                ErrorMessage = ex.Message,
                RawResponse = ex.Response
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error técnico en VeriFactuAdapter para factura {Secuencia}", invoice.Secuencia);
            return new VeriFactuResponse
            {
                Status = EstadoVeriFactu.ErrorTecnico,
                ErrorMessage = ex.Message,
                RawResponse = ex.ToString()
            };
        }
    }

    private Body2 MapToBody2(Invoice invoice, InformacionEmpresa companyInfo)
    {
        // Dividir secuencia en serie y número si es posible, o usar la secuencia completa como número
        string serie = "";
        string numero = invoice.Sequence;
        
        // Intento simple de separar serie de número (asumiendo formato "SERIE-NUMERO" o similar si existe)
        if (invoice.Sequence.Contains('-'))
        {
            var parts = invoice.Sequence.Split('-', 2);
            serie = parts[0];
            numero = parts[1];
        }

        var body = new Body2
        {
            Serie = serie,
            Numero = numero,
            Fecha_expedicion = invoice.Date.ToString("dd-MM-yyyy"),
            Tipo_factura = MapTipoFactura(invoice.InvoiceType),
            Descripcion = invoice.Text ?? "Factura de venta",
            Importe_total = invoice.TaxItems.Sum(t => t.TaxBase + t.TaxAmount).ToString("F2").Replace(",", "."),
            Nif = invoice.BuyerID,
            Nombre = invoice.BuyerName
        };

        // Requerido y permitido únicamente para facturas rectificativas (R1, R2, R3, R4, R5).
        if (invoice.InvoiceType is TipoFacturaAmigable.R1 or TipoFacturaAmigable.R2 or TipoFacturaAmigable.R3 or TipoFacturaAmigable.R4 or TipoFacturaAmigable.R5)
        {
            body.Tipo_rectificativa = Body2Tipo_rectificativa.S; // Por defecto Sustitución, ya que no tenemos este campo en el modelo aún
        }

        foreach (var taxItem in invoice.TaxItems)
        {
            var linea = new Lineas
            {
                Base_imponible = taxItem.TaxBase.ToString("F2").Replace(",", "."),
                Impuesto = MapImpuesto(taxItem.Tax),
                Clave_regimen = MapRegimen(taxItem.TaxScheme)
            };
            
            if (taxItem.TaxException.HasValue)
            {
                linea.Operacion_exenta = MapExencion(taxItem.TaxException.Value);
                // Si hay exención, no se deben enviar tipos ni cuotas impositivas ni calificación según la API
                linea.Tipo_impositivo = null;
                linea.Cuota_repercutida = null;
                // Además, tampoco se deben informar los campos de recargo de equivalencia
                linea.Tipo_recargo_equivalencia = null;
                linea.Cuota_recargo_equivalencia = null;
                // Calificación se anula para evitar el 400
                linea.Calificacion_operacion = null;
            }
            else
            {
                linea.Calificacion_operacion = MapCalificacion(taxItem.TaxType);
                linea.Tipo_impositivo = taxItem.TaxRate.ToString("F2").Replace(",", ".");
                linea.Cuota_repercutida = taxItem.TaxAmount.ToString("F2").Replace(",", ".");
                // Operacion_exenta se mantiene nula
                linea.Operacion_exenta = null;
            }
            
            body.Lineas.Add(linea);
        }

        if (!string.IsNullOrEmpty(invoice.BuyerCountryID) && invoice.BuyerCountryID != "ES")
        {
            body.Id_otro = new Id_otro
            {
                Id_type = MapIDType(invoice.BuyerIDType),
                Id = invoice.BuyerID,
                Codigo_pais = invoice.BuyerCountryID
            };
            // Si usamos id_otro, AEAT a veces prefiere que NIF sea nulo si no es un NIF español
            if (body.Id_otro.Id_type != Id_otroId_type._02)
            {
                body.Nif = null;
            }
        }

        return body;
    }

    private LineasOperacion_exenta MapExencion(CausaExencion taxException) => taxException switch
    {
        CausaExencion.E1 => LineasOperacion_exenta.E1,
        CausaExencion.E2 => LineasOperacion_exenta.E2,
        CausaExencion.E3 => LineasOperacion_exenta.E3,
        CausaExencion.E4 => LineasOperacion_exenta.E4,
        CausaExencion.E5 => LineasOperacion_exenta.E5,
        CausaExencion.E6 => LineasOperacion_exenta.E6,
        _ => LineasOperacion_exenta.E6
    };

    private Body2Tipo_factura MapTipoFactura(TipoFacturaAmigable type) => type switch
    {
        TipoFacturaAmigable.F1 => Body2Tipo_factura.F1,
        TipoFacturaAmigable.F2 => Body2Tipo_factura.F2,
        TipoFacturaAmigable.R1 => Body2Tipo_factura.R1,
        TipoFacturaAmigable.R2 => Body2Tipo_factura.R2,
        TipoFacturaAmigable.R3 => Body2Tipo_factura.R3,
        TipoFacturaAmigable.R4 => Body2Tipo_factura.R4,
        TipoFacturaAmigable.R5 => Body2Tipo_factura.R5,
        TipoFacturaAmigable.F3 => Body2Tipo_factura.F3,
        _ => Body2Tipo_factura.F1
    };

    private LineasImpuesto MapImpuesto(Impuesto tax) => tax switch
    {
        Impuesto.IVA => LineasImpuesto._01,
        Impuesto.IPSI => LineasImpuesto._02,
        Impuesto.IGIC => LineasImpuesto._03,
        _ => LineasImpuesto._01
    };

    private LineasCalificacion_operacion MapCalificacion(CalificacionOperacion op) => op switch
    {
        CalificacionOperacion.S1 => LineasCalificacion_operacion.S1,
        CalificacionOperacion.S2 => LineasCalificacion_operacion.S2,
        CalificacionOperacion.S3 => LineasCalificacion_operacion.S1, // ISP suele mapearse a S1 en algunas APIs si no hay S3 específica
        _ => LineasCalificacion_operacion.S1
    };

    private LineasClave_regimen MapRegimen(ClaveRegimen regime)
    {
        var code = "_" + ((int)regime).ToString("D2");
        if (Enum.TryParse<LineasClave_regimen>(code, out var result))
            return result;
        return LineasClave_regimen._01;
    }

    private Id_otroId_type MapIDType(IDType? type) => type switch
    {
        IDType.NIF_IVA => Id_otroId_type._02,
        IDType.Pasaporte => Id_otroId_type._03,
        IDType.DocumentoOficial => Id_otroId_type._04,
        IDType.CertificadoResidencia => Id_otroId_type._05,
        IDType.OtroDocumento => Id_otroId_type._06,
        _ => Id_otroId_type._06
    };
}
