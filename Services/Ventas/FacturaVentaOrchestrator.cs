using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.Services.Contabilidad;

namespace erp.Module.Services.Ventas;

public class FacturaVentaOrchestrator
{
    public void Validar(FacturaVenta factura)
    {
        if (factura == null) return;

        if (!factura.EsValida())
        {
            throw new UserFriendlyException("La factura no cumple con los requisitos mínimos para ser validada.");
        }

        factura.StateMachine.CambiarA(EstadoDocumentoVenta.Validada);
    }

    public void EnviarAVerifactu(FacturaVenta factura)
    {
        if (factura == null) return;
        
        // Aquí iría la lógica de envío a Verifactu. 
        // Por ahora simulamos que el envío es correcto asignando el estado de Verifactu.
        factura.EstadoVeriFactu = erp.Module.BusinessObjects.Base.Facturacion.EstadoVeriFactu.AceptadaVeriFactu;
        
        factura.StateMachine.CambiarA(EstadoDocumentoVenta.EnviadaVerifactu);
    }

    public void Contabilizar(FacturaVenta factura)
    {
        if (factura == null) return;

        // Ejecutar la acción de contabilización existente
        ContabilidadService.ContabilizarFactura(factura);

        // Cambiar el estado a Contabilizada
        factura.StateMachine.CambiarA(EstadoDocumentoVenta.Contabilizada);
    }
}
