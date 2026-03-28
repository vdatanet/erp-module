using System.Threading.Tasks;
using VeriFactu.Business;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;

namespace erp.Module.Services.Facturacion;

public interface IVeriFactuAdapter
{
    Task<VeriFactuResponse> SendInvoiceAsync(Invoice veriFactuInvoice, InformacionEmpresa companyInfo);
}

public record VeriFactuResponse(
    string Status, 
    string? ErrorCode, 
    string? Response, 
    byte[]? Xml, 
    string? CSV,
    string? ValidationUrl,
    byte[]? QrData);
