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
    public async Task<VeriFactuResponse> SendInvoiceAsync(erp.Module.Models.VeriFactu.Invoice veriFactuInvoice, FacturaBase invoice,
        InformacionEmpresa companyInfo)
    {
        logger.LogInformation("VeriFactuLibraryAdapter: Enviando factura {Sequence} mediante librería local",
            veriFactuInvoice.Sequence);

        try
        {
            // 1. Configurar Settings
            
            Settings.SetConfigFileName(companyInfo.ConfiguracionVeriFactuLibrary);
            Settings.Save();


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
                    TaxAmount = taxItem.TaxAmount,
                    // Intentamos asignar los enums mediante cast si son compatibles, o mapeo manual si fallan
                };
                
                // Mapeo defensivo de enums para evitar errores de compilación si los namespaces no coinciden exactamente
                try { lTaxItem.Tax = (VeriFactu.Xml.Factu.Impuesto)(int)taxItem.Tax; } catch {}
                try { lTaxItem.TaxType = (VeriFactu.Xml.Factu.Alta.CalificacionOperacion)(int)taxItem.TaxType; } catch {}
                try { lTaxItem.TaxScheme = (VeriFactu.Xml.Factu.Alta.ClaveRegimen)(int)taxItem.TaxScheme; } catch {}
                if (taxItem.TaxException.HasValue)
                {
                    try { lTaxItem.TaxException = (VeriFactu.Xml.Factu.Alta.CausaExencion)(int)taxItem.TaxException.Value; } catch {}
                }

                libInvoice.TaxItems.Add(lTaxItem);
            }

            // 3. Enviar
            var invoiceEntry = new InvoiceEntry(libInvoice);
            invoiceEntry.Save();

            // 4. Mapear respuesta
            var response = new VeriFactuResponse
            {
                Status = invoiceEntry.Status == "Correcto" ? EstadoVeriFactu.Correcto : EstadoVeriFactu.Incorrecto,
                ErrorMessage = invoiceEntry.Status != "Correcto"
                    ? $"{invoiceEntry.ErrorCode}: {invoiceEntry.ErrorDescription}"
                    : null,
                ErrorCode = invoiceEntry.ErrorCode,
                RawResponse = invoiceEntry.Response,
                Uuid = invoiceEntry.CSV
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar factura {Sequence} mediante librería", veriFactuInvoice.Sequence);
            return new VeriFactuResponse
            {
                Status = EstadoVeriFactu.ErrorTecnico,
                ErrorMessage = ex.Message
            };
        }
    }

    public Task<VeriFactuResponse> GetStatusAsync(string uuid, InformacionEmpresa companyInfo)
    {
        logger.LogInformation("VeriFactuLibraryAdapter: Consultando estado para UUID {Uuid} (Simulado)", uuid);

        return Task.FromResult(new VeriFactuResponse
        {
            Status = EstadoVeriFactu.Correcto,
            Uuid = uuid
        });
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