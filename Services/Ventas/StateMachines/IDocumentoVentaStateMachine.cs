using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Base.Facturacion;

namespace erp.Module.Services.Ventas.StateMachines;

public interface IFacturaStateMachine
{
    EstadoFactura EstadoActual { get; }
    bool PuedeCambiarA(EstadoFactura nuevoEstado);
    void CambiarA(EstadoFactura nuevoEstado);
    IEnumerable<EstadoFactura> GetEstadosAlcanzables();
}
