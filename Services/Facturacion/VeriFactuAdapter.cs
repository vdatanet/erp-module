using DevExpress.ExpressApp;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Models.VeriFactu;

namespace erp.Module.Services.Facturacion;

public class VeriFactuAdapter(ILogger<VeriFactuAdapter> logger) : IVeriFactuAdapter
{
    public Task<VeriFactuResponse> SendInvoiceAsync(Invoice veriFactuInvoice, FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        logger.LogInformation("VeriFactuAdapter: Simulación de envío para la factura {Secuencia}", invoice.Secuencia);
        
        // Simulación de respuesta exitosa para no romper el flujo del negocio
        var response = new VeriFactuResponse
        {
            Status = EstadoVeriFactu.AceptadaVeriFactu,
            RawResponse = "Librería VeriFactu.dll desactivada. Envío simulado directo.",
            ErrorCode = "SIMULATED_SUCCESS"
        };

        return Task.FromResult(response);
    }
}
