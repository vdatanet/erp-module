using System.Threading.Tasks;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Models.VeriFactu;

namespace erp.Module.Services.Facturacion;

public interface IVeriFactuAdapter
{
    Task<VeriFactuResponse> SendInvoiceAsync(Invoice veriFactuInvoice, FacturaBase invoice, InformacionEmpresa companyInfo);
    Task<VeriFactuResponse> GetStatusAsync(string uuid, InformacionEmpresa companyInfo);
}
