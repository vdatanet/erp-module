using erp.Module.BusinessObjects.Base.Ventas;

namespace erp.Module.Services.Ventas.StateMachines;

public interface IDocumentoVentaStateMachine
{
    EstadoDocumentoVenta EstadoActual { get; }
    bool PuedeCambiarA(EstadoDocumentoVenta nuevoEstado);
    void CambiarA(EstadoDocumentoVenta nuevoEstado);
    IEnumerable<EstadoDocumentoVenta> GetEstadosAlcanzables();
}
