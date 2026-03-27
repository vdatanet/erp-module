using System.Collections.Generic;
using System.Linq;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Ventas;

namespace erp.Module.Services.Ventas.StateMachines;

public class FacturaVentaStateMachine(FacturaVenta documento) : FacturaStateMachineBase(documento)
{
    public override IEnumerable<EstadoDocumentoVenta> GetEstadosAlcanzables()
    {
        return EstadoActual switch
        {
            EstadoDocumentoVenta.Borrador => [EstadoDocumentoVenta.Validada],
            EstadoDocumentoVenta.Validada => [EstadoDocumentoVenta.EnviadaVerifactu],
            EstadoDocumentoVenta.EnviadaVerifactu => [EstadoDocumentoVenta.Contabilizada],
            EstadoDocumentoVenta.Contabilizada => [],
            _ => []
        };
    }

    public override bool PuedeCambiarA(EstadoDocumentoVenta nuevoEstado)
    {
        // El flujo es lineal estricto sin saltos ni retrocesos
        return GetEstadosAlcanzables().Contains(nuevoEstado);
    }
}
