using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Config.VeriFactu;
using erp.Module.Models.VeriFactu;
using Microsoft.Extensions.Logging;

namespace erp.Module.Services.Facturacion;

public class VeriFactuLibraryAdapter(ILogger<VeriFactuLibraryAdapter> logger) : IVeriFactuAdapter
{
    public async Task<VeriFactuResponse> SendInvoiceAsync(Models.VeriFactu.Invoice veriFactuInvoice, FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        logger.LogInformation("VeriFactuLibraryAdapter: Enviando factura {Sequence} mediante librería local", veriFactuInvoice.Sequence);

        try
        {
            // 1. Configurar Settings
            await Settings.UpdateAsync(s =>
            {
                // Configuración de certificado desde InformacionEmpresa
                if (companyInfo.CertificadoVeriFactu != null)
                {
                    string certPath = Path.Combine(Settings.Path, "cert_verifactu.pfx");
                    using (var stream = new FileStream(certPath, FileMode.Create))
                    {
                        companyInfo.CertificadoVeriFactu.SaveToStream(stream);
                    }
                    s.CertificatePath = certPath;
                    s.CertificatePassword = companyInfo.PasswordCertificadoVeriFactu;
                }

                s.VeriFactuEndPointPrefix = companyInfo.VeriFactuEntornoProduccion 
                    ? VeriFactuEndPointPrefixes.Prod 
                    : "https://www1.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/VeriFactuFE.wsdl"; 
                
                s.VeriFactuEndPointValidatePrefix = companyInfo.VeriFactuEntornoProduccion
                    ? VeriFactuEndPointPrefixes.ProdValidate
                    : "https://www1.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/VeriFactuFE_Validacion.wsdl";

                s.SistemaInformatico.NIF = companyInfo.Nif;
                s.SistemaInformatico.NombreRazon = companyInfo.Nombre;
            });

            // 2. Mapear a VeriFactu.Xml.Factu.Alta.Invoice
            var libInvoice = Invoice(veriFactuInvoice.Sequence, veriFactuInvoice.Date, veriFactuInvoice.SellerID)
            {
                TipoFactura = MapTipoFactura(veriFactuInvoice.InvoiceType),
                SellerName = veriFactuInvoice.SellerName,
                BuyerID = veriFactuInvoice.BuyerID,
                BuyerName = veriFactuInvoice.BuyerName,
                Text = veriFactuInvoice.Text
            };

            foreach (var taxItem in veriFactuInvoice.TaxItems)
            {
                libInvoice.TaxItems.Add(new global::VeriFactu.Xml.Factu.Alta.TaxItem()
                {
                    TaxRate = taxItem.TaxRate,
                    TaxBase = taxItem.TaxBase,
                    TaxAmount = taxItem.TaxAmount
                });
            }

            // 3. Enviar
            var invoiceEntry = new global::VeriFactu.Business.InvoiceEntry(libInvoice);
            invoiceEntry.Save();

            // 4. Mapear respuesta
            var response = new VeriFactuResponse
            {
                Status = invoiceEntry.Status == "Correcto" ? EstadoVeriFactu.Correcto : EstadoVeriFactu.Incorrecto,
                ErrorMessage = invoiceEntry.Status != "Correcto" ? $"{invoiceEntry.ErrorCode}: {invoiceEntry.ErrorDescription}" : null,
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

    private global::VeriFactu.Xml.Factu.Alta.TipoFactura MapTipoFactura(TipoFacturaAmigable type)
    {
        return type switch
        {
            TipoFacturaAmigable.F1 => global::VeriFactu.Xml.Factu.Alta.TipoFactura.F1,
            TipoFacturaAmigable.F2 => global::VeriFactu.Xml.Factu.Alta.TipoFactura.F2,
            TipoFacturaAmigable.F3 => global::VeriFactu.Xml.Factu.Alta.TipoFactura.F3,
            TipoFacturaAmigable.F4 => global::VeriFactu.Xml.Factu.Alta.TipoFactura.F1, // F4 no existe en la librería, mapeamos a F1
            TipoFacturaAmigable.R1 => global::VeriFactu.Xml.Factu.Alta.TipoFactura.R1,
            TipoFacturaAmigable.R2 => global::VeriFactu.Xml.Factu.Alta.TipoFactura.R2,
            TipoFacturaAmigable.R3 => global::VeriFactu.Xml.Factu.Alta.TipoFactura.R3,
            TipoFacturaAmigable.R4 => global::VeriFactu.Xml.Factu.Alta.TipoFactura.R4,
            TipoFacturaAmigable.R5 => global::VeriFactu.Xml.Factu.Alta.TipoFactura.R5,
            _ => global::VeriFactu.Xml.Factu.Alta.TipoFactura.F1
        };
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
}
