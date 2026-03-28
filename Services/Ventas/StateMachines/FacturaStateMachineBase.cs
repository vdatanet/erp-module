using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Helpers.Contactos;

namespace erp.Module.Services.Ventas.StateMachines;

public abstract class FacturaStateMachineBase(FacturaBase documento) : IFacturaStateMachine
{
    protected readonly FacturaBase Factura = documento ?? throw new ArgumentNullException(nameof(documento));

    public Enum EstadoActual
    {
        get => Factura.EstadoFactura;
        protected set => Factura.EstadoFactura = (EstadoFactura)value;
    }

    public virtual bool PuedeCambiarA(Enum nuevoEstado)
    {
        if (Equals(EstadoActual, nuevoEstado)) return false;
        
        return GetEstadosAlcanzables().Contains(nuevoEstado);
    }

    public virtual void CambiarA(Enum nuevoEstado)
    {
        if (!PuedeCambiarA(nuevoEstado))
            throw new InvalidOperationException($"No se puede cambiar el estado de {EstadoActual} a {nuevoEstado} para esta factura.");

        var oldEstado = EstadoActual;
        
        EstadoActual = nuevoEstado;
        
        OnEstadoCambiado(oldEstado, nuevoEstado);
    }

    public virtual IEnumerable<Enum> GetEstadosAlcanzables()
    {
        return ((EstadoFactura)EstadoActual) switch
        {
            EstadoFactura.Borrador => [EstadoFactura.Validada],
            EstadoFactura.Validada => [EstadoFactura.EnviadaVerifactu, EstadoFactura.Contabilizada, EstadoFactura.Borrador],
            EstadoFactura.EnviadaVerifactu => [EstadoFactura.Contabilizada],
            EstadoFactura.Contabilizada => [],
            _ => []
        };
    }

    protected virtual void OnEstadoCambiado(Enum oldEstado, Enum nuevoEstado)
    {
        // Hook para lógica adicional en subclases
    }
}
