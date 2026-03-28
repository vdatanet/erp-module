using erp.Module.BusinessObjects.Base.Facturacion;

namespace erp.Module.Services.Ventas.StateMachines;

public interface IFacturaStateMachine
{
    Enum EstadoActual { get; }
    bool PuedeCambiarA(Enum nuevoEstado);
    void CambiarA(Enum nuevoEstado);
    IEnumerable<Enum> GetEstadosAlcanzables();
}
