using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Models.VeriFactu;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace erp.Module.Services.Facturacion;

public class VeriFactuAdapterDispatcher(
    IServiceProvider serviceProvider,
    ILogger<VeriFactuAdapterDispatcher> logger) : IVeriFactuAdapter
{
    private IVeriFactuAdapter GetImplementation(InformacionEmpresa companyInfo)
    {
        if (companyInfo.VeriFactuProvider == VeriFactuProvider.Library)
        {
            logger.LogDebug("VeriFactuAdapterDispatcher: Usando VeriFactuLibraryAdapter");
            return serviceProvider.GetRequiredService<VeriFactuLibraryAdapter>();
        }

        logger.LogDebug("VeriFactuAdapterDispatcher: Usando VeriFactuApiAdapter");
        return serviceProvider.GetRequiredService<VeriFactuApiAdapter>();
    }

    public async Task<VeriFactuResponse> SendInvoiceAsync(Invoice veriFactuInvoice, FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        var implementation = GetImplementation(companyInfo);
        return await implementation.SendInvoiceAsync(veriFactuInvoice, invoice, companyInfo);
    }

    public async Task<VeriFactuResponse> GetStatusAsync(string uuid, InformacionEmpresa companyInfo)
    {
        var implementation = GetImplementation(companyInfo);
        return await implementation.GetStatusAsync(uuid, companyInfo);
    }
}
