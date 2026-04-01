using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Models.VeriFactu;
using Microsoft.Extensions.Logging;

namespace erp.Module.Services.Facturacion;

public class VeriFactuLibraryAdapter(ILogger<VeriFactuLibraryAdapter> logger) : IVeriFactuAdapter
{
    public Task<VeriFactuResponse> SendInvoiceAsync(Invoice veriFactuInvoice, FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        logger.LogInformation("VeriFactuLibraryAdapter: Enviando factura {Sequence} mediante librería local (Simulado)", veriFactuInvoice.Sequence);
        
        // TODO: Implementar integración real con la librería Verifactu
        
        return Task.FromResult(new VeriFactuResponse
        {
            Status = EstadoVeriFactu.Correcto,
            ErrorMessage = "Enviado mediante librería local (Simulado)",
            Uuid = Guid.NewGuid().ToString()
        });
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
