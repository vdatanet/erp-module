using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Ventas;

namespace erp.Module.Services.Ventas.StateMachines;

public class OfertaVentaStateMachine(OfertaVenta documento) : DocumentoVentaStateMachineBase(documento)
{
    public override IEnumerable<EstadoDocumentoVenta> GetEstadosAlcanzables()
    {
        return EstadoActual switch
        {
            EstadoDocumentoVenta.Borrador => [EstadoDocumentoVenta.Confirmado, EstadoDocumentoVenta.Anulado],
            EstadoDocumentoVenta.Confirmado => [EstadoDocumentoVenta.Emitido, EstadoDocumentoVenta.Anulado, EstadoDocumentoVenta.Borrador],
            EstadoDocumentoVenta.Emitido => [EstadoDocumentoVenta.Impreso, EstadoDocumentoVenta.Anulado, EstadoDocumentoVenta.Bloqueado],
            EstadoDocumentoVenta.Impreso => [EstadoDocumentoVenta.Anulado, EstadoDocumentoVenta.Bloqueado],
            EstadoDocumentoVenta.Bloqueado => [EstadoDocumentoVenta.Anulado],
            _ => []
        };
    }
}
