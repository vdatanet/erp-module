using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Facturacion;

namespace erp.Module.Services.Facturacion;

public interface IFacturaeService
{
    /// <summary>
    /// Genera el XML de Facturae para la factura proporcionada.
    /// </summary>
    /// <param name="objectSpace">ObjectSpace para consultas adicionales.</param>
    /// <param name="invoice">Factura a procesar.</param>
    /// <returns>Contenido del XML generado.</returns>
    string GenerateFacturaeXml(IObjectSpace objectSpace, FacturaBase invoice);

    /// <summary>
    /// Genera y firma el XML de Facturae para la factura proporcionada.
    /// </summary>
    /// <param name="objectSpace">ObjectSpace para consultas adicionales.</param>
    /// <param name="invoice">Factura a procesar.</param>
    /// <returns>Contenido del XML firmado.</returns>
    byte[] GenerateSignedFacturae(IObjectSpace objectSpace, FacturaBase invoice);
}
