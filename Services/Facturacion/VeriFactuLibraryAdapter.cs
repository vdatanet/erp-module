using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Models.VeriFactu;
using Microsoft.Extensions.Logging;
using VeriFactu.Business;
using Settings = VeriFactu.Config.Settings;
using LibInvoice = VeriFactu.Business.Invoice;
using LibTaxItem = VeriFactu.Business.TaxItem;

namespace erp.Module.Services.Facturacion;

public class VeriFactuLibraryAdapter(ILogger<VeriFactuLibraryAdapter> logger) : IVeriFactuAdapter
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<VeriFactuResponse> SendInvoiceAsync(erp.Module.Models.VeriFactu.Invoice veriFactuInvoice, FacturaBase invoice,
        InformacionEmpresa companyInfo)
    {
        return await ExecuteInContextAsync(async () =>
        {
            logger.LogInformation("VeriFactuLibraryAdapter: Enviando factura {Sequence} mediante librería local",
                veriFactuInvoice.Sequence);

            // 2. Mapear a VeriFactu.Xml.Factu.Alta.Invoice
            var libInvoice = new LibInvoice(veriFactuInvoice.Sequence, veriFactuInvoice.Date, veriFactuInvoice.SellerID)
            {
                InvoiceType = MapTipoFactura(veriFactuInvoice.InvoiceType),
                SellerName = veriFactuInvoice.SellerName,
                BuyerID = veriFactuInvoice.BuyerID,
                BuyerName = veriFactuInvoice.BuyerName,
                Text = veriFactuInvoice.Text,
                TaxItems = []
            };

            foreach (var taxItem in veriFactuInvoice.TaxItems)
            {
                var lTaxItem = new LibTaxItem
                {
                    TaxRate = taxItem.TaxRate,
                    TaxBase = taxItem.TaxBase,
                    TaxAmount = taxItem.TaxAmount
                };

                // Mapeo defensivo de enums
                try { lTaxItem.Tax = (VeriFactu.Xml.Factu.Impuesto)(int)taxItem.Tax; } catch { }
                try { lTaxItem.TaxType = (VeriFactu.Xml.Factu.Alta.CalificacionOperacion)(int)taxItem.TaxType; } catch { }
                try { lTaxItem.TaxScheme = (VeriFactu.Xml.Factu.Alta.ClaveRegimen)(int)taxItem.TaxScheme; } catch { }
                if (taxItem.TaxException.HasValue)
                {
                    try { lTaxItem.TaxException = (VeriFactu.Xml.Factu.Alta.CausaExencion)(int)taxItem.TaxException.Value; } catch { }
                }

                libInvoice.TaxItems.Add(lTaxItem);
            }

            // 3. Enviar
            var invoiceEntry = new InvoiceEntry(libInvoice);
            invoiceEntry.Save();

            // 4. Mapear respuesta
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
        }, companyInfo, veriFactuInvoice.Sequence);
    }

    public async Task<VeriFactuResponse> GetStatusAsync(string uuid, InformacionEmpresa companyInfo)
    {
        return await ExecuteInContextAsync(async () =>
        {
            logger.LogInformation("VeriFactuLibraryAdapter: Consultando estado para UUID {Uuid} (Simulado)", uuid);

            // Aunque sea simulado por ahora, el patrón de acceso debe ser consistente
            return new VeriFactuResponse
            {
                Status = EstadoVeriFactu.Correcto,
                Uuid = uuid
            };
        }, companyInfo, uuid);
    }

    private async Task<VeriFactuResponse> ExecuteInContextAsync(Func<Task<VeriFactuResponse>> action,
        InformacionEmpresa companyInfo, string identifier)
    {
        try
        {
            await _semaphore.WaitAsync();
            
            // Capturamos el estado anterior (usando reflexión si es necesario, pero aquí asumimos 
            // que la mejor práctica si no conocemos el getter es limpiar al terminar)
            
            try
            {
                // 1. Configurar Settings para el tenant actual
                Settings.SetConfigFileName(companyInfo.ConfiguracionVeriFactuLibrary);
                Settings.Save();

                return await action();
            }
            finally
            {
                // Limpiar configuración al terminar para evitar fuga de datos
                try
                {
                    Settings.SetConfigFileName(string.Empty);
                    Settings.Save();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error al limpiar la configuración de VeriFactu tras la operación");
                }
                
                _semaphore.Release();
            }
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

    private VeriFactu.Xml.Factu.Alta.TipoFactura MapTipoFactura(TipoFacturaAmigable type)
    {
        return type switch
        {
            TipoFacturaAmigable.F1 => VeriFactu.Xml.Factu.Alta.TipoFactura.F1,
            TipoFacturaAmigable.F2 => VeriFactu.Xml.Factu.Alta.TipoFactura.F2,
            TipoFacturaAmigable.F3 => VeriFactu.Xml.Factu.Alta.TipoFactura.F3,
            TipoFacturaAmigable.F4 => VeriFactu.Xml.Factu.Alta.TipoFactura
                .F1, // F4 no existe en la librería, mapeamos a F1
            TipoFacturaAmigable.R1 => VeriFactu.Xml.Factu.Alta.TipoFactura.R1,
            TipoFacturaAmigable.R2 => VeriFactu.Xml.Factu.Alta.TipoFactura.R2,
            TipoFacturaAmigable.R3 => VeriFactu.Xml.Factu.Alta.TipoFactura.R3,
            TipoFacturaAmigable.R4 => VeriFactu.Xml.Factu.Alta.TipoFactura.R4,
            TipoFacturaAmigable.R5 => VeriFactu.Xml.Factu.Alta.TipoFactura.R5,
            _ => VeriFactu.Xml.Factu.Alta.TipoFactura.F1
        };
    }
}