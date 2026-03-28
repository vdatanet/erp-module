using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Tpv;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.Services.Contabilidad;
using erp.Module.Services.Facturacion;

namespace erp.Module.Services.Ventas;

public class FacturaOrchestrator
{
    public void Validar(FacturaBase factura)
    {
        if (factura == null) return;

        var validationResult = factura.ValidarParaEmision();
        if (!validationResult.IsValid)
        {
            throw new UserFriendlyException($"No se puede validar la factura:\n{validationResult.ErrorMessage}");
        }

        factura.StateMachine.CambiarA(EstadoFactura.Validada);
    }

    public VeriFactuService.SendResult EnviarAVerifactu(IObjectSpace objectSpace, FacturaBase factura, VeriFactuService veriFactuService)
    {
        if (factura == null) return new VeriFactuService.SendResult(false, "La factura es nula.");
        if (veriFactuService == null) return new VeriFactuService.SendResult(false, "El servicio VeriFactu no está disponible.");

        var validationResult = factura.ValidarParaEmision();
        if (!validationResult.IsValid)
        {
            throw new UserFriendlyException($"No se puede enviar a VeriFactu:\n{validationResult.ErrorMessage}");
        }

        var result = veriFactuService.SendFactura(objectSpace, factura);

        if (result.Success)
        {
            factura.StateMachine.CambiarA(EstadoFactura.EnviadaVerifactu);
        }

        return result;
    }

    public void Contabilizar(FacturaBase factura)
    {
        if (factura == null) return;

        // Ejecutar la acción de contabilización existente
        ContabilidadService.ContabilizarFactura(factura);

        // Cambiar el estado a Contabilizada
        factura.StateMachine.CambiarA(EstadoFactura.Contabilizada);
    }

    public void RevertirABorrador(FacturaBase factura)
    {
        if (factura == null) return;

        if (factura.EstadoFactura != EstadoFactura.Validada)
        {
            throw new UserFriendlyException("Solo se pueden revertir a borrador las facturas validadas.");
        }

        factura.StateMachine.CambiarA(EstadoFactura.Borrador);
    }
}
