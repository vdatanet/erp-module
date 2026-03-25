using erp.Module.BusinessObjects.Tpv;
using erp.Module.BusinessObjects;
using TpvBO = erp.Module.BusinessObjects.Tpv.Tpv;

namespace erp.Module.Services.Tpv;

public interface ISesionTpvService
{
    SesionTpv AbrirSesion(TpvBO tpv, ApplicationUser usuario, decimal importeApertura);
    void InicializarSesion(SesionTpv sesion, TpvBO tpv, ApplicationUser usuario, decimal importeApertura);
    void CerrarSesion(SesionTpv sesion, decimal? importeCierreManual = null, string? observaciones = null);
    void ReabrirSesion(SesionTpv sesion, string? motivo = null);
    void RegistrarMovimiento(SesionTpv sesion, TipoMovimientoCajaTpv tipo, decimal importe, string? motivo);
    
    decimal CalcularVentasTotales(SesionTpv sesion);
    decimal CalcularMovimientosTotales(SesionTpv sesion);
    decimal CalcularImporteEsperado(SesionTpv sesion);
    
    bool PuedeAbrirSesion(TpvBO tpv, out string? error);
    bool PuedeCerrarSesion(SesionTpv sesion, out string? error);
    bool PuedeReabrirSesion(SesionTpv sesion, out string? error);
}
