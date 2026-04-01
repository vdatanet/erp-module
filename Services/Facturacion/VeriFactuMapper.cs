using erp.Module.BusinessObjects.Base.Facturacion;
using Microsoft.Extensions.Logging;
using LibTipoFactura = VeriFactu.Xml.Factu.Alta.TipoFactura;
using LibTipoRectificativa = VeriFactu.Xml.Factu.Alta.TipoRectificativa;
using LibImpuesto = VeriFactu.Xml.Factu.Impuesto;
using LibCalificacionOperacion = VeriFactu.Xml.Factu.Alta.CalificacionOperacion;
using LibClaveRegimen = VeriFactu.Xml.Factu.Alta.ClaveRegimen;
using LibCausaExencion = VeriFactu.Xml.Factu.Alta.CausaExencion;

namespace erp.Module.Services.Facturacion;

public static class VeriFactuMapper
{
    public static LibTipoFactura MapTipoFactura(TipoFacturaAmigable type, ILogger? logger = null)
    {
        var mappedValue = type switch
        {
            TipoFacturaAmigable.F1 => 0,
            TipoFacturaAmigable.F2 => 1,
            TipoFacturaAmigable.F3 => 2,
            TipoFacturaAmigable.F4 => 0, // Fallback a F1
            TipoFacturaAmigable.R1 => 3,
            TipoFacturaAmigable.R2 => 4,
            TipoFacturaAmigable.R3 => 5,
            TipoFacturaAmigable.R4 => 6,
            TipoFacturaAmigable.R5 => 7,
            _ => 0
        };

        var mapped = (LibTipoFactura)mappedValue;
        // logger?.LogInformation("Mapeando TipoFactura: {Internal} -> {External}", type, mapped);
        return mapped;
    }

    public static LibTipoRectificativa MapTipoRectificativa(TipoRectificativaAmigable? type, ILogger? logger = null)
    {
        var mappedValue = type switch
        {
            TipoRectificativaAmigable.S => 1,
            TipoRectificativaAmigable.I => 2,
            _ => 0
        };

        var mapped = (LibTipoRectificativa)mappedValue;
        // logger?.LogInformation("Mapeando TipoRectificativa: {Internal} -> {External}", type?.ToString() ?? "null", mapped);
        return mapped;
    }

    public static LibImpuesto MapImpuesto(Impuesto tax, ILogger? logger = null)
    {
        var mappedValue = tax switch
        {
            Impuesto.IVA => 0,
            Impuesto.IPSI => 1,
            Impuesto.IGIC => 2,
            _ => 0
        };

        var mapped = (LibImpuesto)mappedValue;
        // logger?.LogInformation("Mapeando Impuesto: {Internal} -> {External}", tax, mapped);
        return mapped;
    }

    public static LibCalificacionOperacion MapCalificacion(CalificacionOperacion type, ILogger? logger = null)
    {
        var mappedValue = type switch
        {
            CalificacionOperacion.S1 => 0,
            CalificacionOperacion.S3 => 1,
            CalificacionOperacion.S2 => 0, // Fallback temporal
            _ => 0
        };

        var mapped = (LibCalificacionOperacion)mappedValue;
        // logger?.LogInformation("Mapeando Calificacion: {Internal} -> {External}", type, mapped);
        return mapped;
    }

    public static LibClaveRegimen MapRegimen(ClaveRegimen scheme, ILogger? logger = null)
    {
        var mappedValue = scheme switch
        {
            ClaveRegimen.General => 1,
            ClaveRegimen.Exportacion => 2,
            ClaveRegimen.BienesUsados => 3,
            ClaveRegimen.AgenciasViajes => 5,
            ClaveRegimen.AgriculturaGanaderiaPesca => 19,
            ClaveRegimen.RecargoEquivalencia => 18,
            ClaveRegimen.CriterioCaja => 7,
            ClaveRegimen.GrupoEntidades => 6,
            ClaveRegimen.CobroTerceros => 10,
            ClaveRegimen.IGIC_IPSI => 8,
            ClaveRegimen.AgenciasViajesMediadoras => 9,
            ClaveRegimen.ArrendamientosLocales => 11,
            _ => 1
        };

        var mapped = (LibClaveRegimen)mappedValue;
        // logger?.LogInformation("Mapeando Regimen: {Internal} -> {External}", scheme, mapped);
        return mapped;
    }

    public static LibCausaExencion MapCausaExencion(CausaExencion exception, ILogger? logger = null)
    {
        var mappedValue = exception switch
        {
            CausaExencion.E1 => 0,
            CausaExencion.E2 => 1,
            CausaExencion.E3 => 2,
            CausaExencion.E4 => 3,
            CausaExencion.E5 => 4,
            CausaExencion.E6 => 5,
            _ => 0
        };

        var mapped = (LibCausaExencion)mappedValue;
        // logger?.LogInformation("Mapeando CausaExencion: {Internal} -> {External}", exception, mapped);
        return mapped;
    }
}
