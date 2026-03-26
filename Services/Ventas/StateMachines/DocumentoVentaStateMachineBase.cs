using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Helpers.Contactos;

namespace erp.Module.Services.Ventas.StateMachines;

public abstract class DocumentoVentaStateMachineBase(DocumentoVenta documento) : IDocumentoVentaStateMachine
{
    protected readonly DocumentoVenta Documento = documento ?? throw new ArgumentNullException(nameof(documento));

    public EstadoDocumentoVenta EstadoActual => Documento.Estado;

    public virtual bool PuedeCambiarA(EstadoDocumentoVenta nuevoEstado)
    {
        if (EstadoActual == nuevoEstado) return false;
        if (EstadoActual == EstadoDocumentoVenta.Anulado) return false;
        if (EstadoActual == EstadoDocumentoVenta.Bloqueado && nuevoEstado != EstadoDocumentoVenta.Anulado) return false;

        return GetEstadosAlcanzables().Contains(nuevoEstado);
    }

    public virtual void CambiarA(EstadoDocumentoVenta nuevoEstado)
    {
        if (!PuedeCambiarA(nuevoEstado))
            throw new InvalidOperationException($"No se puede cambiar el estado de {EstadoActual} a {nuevoEstado} para este documento.");

        var oldEstado = EstadoActual;
        Documento.Estado = nuevoEstado;
        
        ActualizarFechasControl(oldEstado, nuevoEstado);
        OnEstadoCambiado(oldEstado, nuevoEstado);
    }

    public abstract IEnumerable<EstadoDocumentoVenta> GetEstadosAlcanzables();

    protected virtual void ActualizarFechasControl(EstadoDocumentoVenta oldEstado, EstadoDocumentoVenta nuevoEstado)
    {
        var localTime = InformacionEmpresaHelper.GetLocalTime(Documento.Session);

        switch (nuevoEstado)
        {
            case EstadoDocumentoVenta.Confirmado:
                Documento.FechaConfirmacion = localTime;
                break;
            case EstadoDocumentoVenta.Emitido:
                Documento.FechaEmision = localTime;
                break;
            case EstadoDocumentoVenta.Impreso:
                Documento.FechaImpresion = localTime;
                break;
            case EstadoDocumentoVenta.Anulado:
                Documento.FechaAnulacion = localTime;
                break;
        }
    }

    protected virtual void OnEstadoCambiado(EstadoDocumentoVenta oldEstado, EstadoDocumentoVenta nuevoEstado)
    {
        // Hook para lógica adicional en subclases
    }
}
