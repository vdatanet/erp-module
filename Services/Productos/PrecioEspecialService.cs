using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Productos;
using erp.Module.Helpers.Contactos;

namespace erp.Module.Services.Productos;

public static class PrecioEspecialService
{
    public static PrecioEspecial? GetPrecioEspecialActivo(Producto producto, Tercero tercero, ContextoPrecio contexto, DateTime fecha)
    {
        if (producto == null || tercero == null) return null;

        var session = producto.Session;
        
        // Criterio base: Producto, Tercero y Contexto compatible
        var criteria = CriteriaOperator.Parse(
            "Producto = ? AND Tercero = ? AND (Contexto = ? OR Contexto = ?)",
            producto, tercero, contexto, ContextoPrecio.Ambos);

        // Criterio de vigencia
        var vigenciaCriteria = CriteriaOperator.Parse(
            "(VigenteDesde IS NULL OR VigenteDesde <= ?) AND (VigenteHasta IS NULL OR VigenteHasta >= ?)",
            fecha, fecha);

        var finalCriteria = GroupOperator.And(criteria, vigenciaCriteria);

        var precios = new XPCollection<PrecioEspecial>(session, finalCriteria);
        
        if (precios.Count == 0) return null;
        if (precios.Count == 1) return precios[0];

        // Si hay varios, priorizar por:
        // 1. El que tenga fechas de vigencia más restrictivas (opcional, pero complejo)
        // 2. El más reciente (ModificadoEl o CreadoEl)
        return precios
            .OrderByDescending(p => p.ModificadoEl ?? p.CreadoEl)
            .ThenByDescending(p => p.Oid) // Desempate final
            .FirstOrDefault();
    }
}
