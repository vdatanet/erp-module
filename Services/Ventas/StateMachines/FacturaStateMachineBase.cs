using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Base.Ventas;

namespace erp.Module.Services.Ventas.StateMachines;

public abstract class FacturaStateMachineBase(FacturaBase documento) : DocumentoVentaStateMachineBase(documento)
{
    protected readonly FacturaBase Factura = documento;

    public override IEnumerable<EstadoDocumentoVenta> GetEstadosAlcanzables()
    {
        // Las facturas tienen un flujo más restringido una vez emitidas/sincronizadas por temas fiscales (VeriFactu)
        return EstadoActual switch
        {
            EstadoDocumentoVenta.Borrador => [EstadoDocumentoVenta.Confirmado, EstadoDocumentoVenta.Anulado],
            EstadoDocumentoVenta.Confirmado => [EstadoDocumentoVenta.Emitido, EstadoDocumentoVenta.Anulado, EstadoDocumentoVenta.Borrador],
            EstadoDocumentoVenta.Emitido => [EstadoDocumentoVenta.Impreso, EstadoDocumentoVenta.Sincronizado, EstadoDocumentoVenta.Anulado, EstadoDocumentoVenta.Bloqueado],
            EstadoDocumentoVenta.Impreso => [EstadoDocumentoVenta.Sincronizado, EstadoDocumentoVenta.Anulado, EstadoDocumentoVenta.Bloqueado],
            EstadoDocumentoVenta.Sincronizado => [EstadoDocumentoVenta.Impreso, EstadoDocumentoVenta.Bloqueado], // Una vez sincronizada, anular suele requerir factura rectificativa
            EstadoDocumentoVenta.Bloqueado => [], 
            _ => []
        };
    }

    public override bool PuedeCambiarA(EstadoDocumentoVenta nuevoEstado)
    {
        // Regla base de facturas: si está enviada a VeriFactu, no se puede volver a borrador ni anular directamente
        if (Factura.EstadoVeriFactu is EstadoVeriFactu.EnviadaVeriFactu or EstadoVeriFactu.AceptadaVeriFactu)
        {
            if (nuevoEstado is EstadoDocumentoVenta.Borrador or EstadoDocumentoVenta.Anulado)
                return false;
        }

        return base.PuedeCambiarA(nuevoEstado);
    }
}
