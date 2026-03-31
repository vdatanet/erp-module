using System.Reflection;
using DevExpress.ExpressApp.DC;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Helpers.Contactos;

namespace erp.Module.Services.Ventas.StateMachines;

public abstract class FacturaStateMachineBase(FacturaBase documento) : IFacturaStateMachine
{
    protected readonly FacturaBase Factura = documento ?? throw new ArgumentNullException(nameof(documento));

    protected virtual Dictionary<EstadoFactura, List<EstadoFactura>> Transiciones => new()
    {
        { EstadoFactura.Borrador, [EstadoFactura.Validada] },
        { EstadoFactura.Validada, [EstadoFactura.Emitida, EstadoFactura.Borrador] },
        { EstadoFactura.Emitida, [EstadoFactura.Enviada, EstadoFactura.Contabilizada] },
        { EstadoFactura.Enviada, [EstadoFactura.Contabilizada] },
        { EstadoFactura.VeriFactuNoNecesario, [EstadoFactura.Contabilizada] },
        { EstadoFactura.Contabilizada, [] }
    };

    private static readonly Dictionary<EstadoFactura, string> DisplayNamesCache = new();

    public EstadoFactura EstadoActual
    {
        get => Factura.EstadoFactura;
        protected set => Factura.EstadoFactura = value;
    }

    public virtual bool PuedeCambiarA(EstadoFactura nuevoEstado)
    {
        if (Equals(EstadoActual, nuevoEstado)) return false;

        return GetEstadosAlcanzables().Contains(nuevoEstado);
    }

    public virtual void CambiarA(EstadoFactura nuevoEstado)
    {
        if (!Enum.IsDefined(typeof(EstadoFactura), nuevoEstado))
        {
            throw new ArgumentException($"El estado '{nuevoEstado}' no es un valor válido de EstadoFactura.", nameof(nuevoEstado));
        }

        if (!PuedeCambiarA(nuevoEstado))
        {
            var facturaInfo = !string.IsNullOrEmpty(Factura.NumeroFiscal) 
                ? $"Número Fiscal: {Factura.NumeroFiscal}" 
                : $"Oid: {Factura.Oid}";
            
            var estadosAlcanzables = string.Join(", ", GetEstadosAlcanzables());
            var mensaje = $"No se puede cambiar el estado de '{EstadoActual}' a '{nuevoEstado}' para la factura ({facturaInfo}). " +
                         $"Estados permitidos desde '{EstadoActual}': [{estadosAlcanzables}]";
            
            throw new InvalidOperationException(mensaje);
        }

        var oldEstado = EstadoActual;
        EstadoActual = nuevoEstado;

        OnEstadoCambiado(oldEstado, nuevoEstado);
    }

    public virtual IEnumerable<EstadoFactura> GetEstadosAlcanzables()
    {
        if (EstadoActual == EstadoFactura.Emitida)
        {
            var infoEmpresa = Helpers.Contactos.InformacionEmpresaHelper.GetInformacionEmpresa(Factura.Session);
            var activarVeriFactu = infoEmpresa?.ActivarVeriFactu ?? false;

            if (activarVeriFactu)
            {
                // Si VeriFactu está activo, debe pasar por Enviada
                return new List<EstadoFactura> { EstadoFactura.Enviada, EstadoFactura.Contabilizada };
            }
            else
            {
                // Si no está activo, puede saltar a VeriFactuNoNecesario o directamente a Contabilizada
                return new List<EstadoFactura> { EstadoFactura.VeriFactuNoNecesario, EstadoFactura.Contabilizada };
            }
        }

        if (EstadoActual == EstadoFactura.VeriFactuNoNecesario)
        {
            return new List<EstadoFactura> { EstadoFactura.Contabilizada };
        }

        return Transiciones.TryGetValue(EstadoActual, out var estados) ? estados : [];
    }

    protected virtual void OnEstadoCambiado(EstadoFactura oldEstado, EstadoFactura nuevoEstado)
    {
        // Hook para lógica adicional en subclases
    }

    public override string ToString()
    {
        if (DisplayNamesCache.TryGetValue(EstadoActual, out var displayName))
        {
            return displayName;
        }

        var type = typeof(EstadoFactura);
        var name = Enum.GetName(type, EstadoActual);
        if (name != null)
        {
            var field = type.GetField(name);
            if (field != null)
            {
                var attr = field.GetCustomAttribute<XafDisplayNameAttribute>();
                if (attr != null)
                {
                    DisplayNamesCache[EstadoActual] = attr.DisplayName;
                    return attr.DisplayName;
                }
            }
        }

        var fallbackName = EstadoActual.ToString();
        DisplayNamesCache[EstadoActual] = fallbackName;
        return fallbackName;
    }
}
