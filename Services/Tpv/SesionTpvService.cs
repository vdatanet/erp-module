using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Security;
using DevExpress.Xpo;
using erp.Module.BusinessObjects;
using erp.Module.BusinessObjects.Tpv;
using erp.Module.Helpers.Contactos;
using Microsoft.Extensions.DependencyInjection;
using TpvBO = erp.Module.BusinessObjects.Tpv.Tpv;

namespace erp.Module.Services.Tpv;

public class SesionTpvService : ISesionTpvService
{
    public SesionTpv AbrirSesion(TpvBO tpv, ApplicationUser usuario, decimal importeApertura)
    {
        using var uow = new UnitOfWork(tpv.Session.DataLayer);
        uow.BeginTransaction();
        try
        {
            var tpvInUow = uow.GetObjectByKey<TpvBO>(tpv.Oid);
            var userInUow = uow.GetObjectByKey<ApplicationUser>(usuario.Oid);
            
            var sesion = new SesionTpv(uow);
            InicializarSesion(sesion, tpvInUow, userInUow, importeApertura);
            
            uow.CommitChanges();
            uow.CommitTransaction();
            
            return tpv.Session.GetObjectByKey<SesionTpv>(sesion.Oid);
        }
        catch
        {
            uow.RollbackTransaction();
            throw;
        }
    }

    public void InicializarSesion(SesionTpv sesion, TpvBO tpv, ApplicationUser usuario, decimal importeApertura)
    {
        if (!PuedeAbrirSesion(tpv, out string? error))
            throw new InvalidOperationException(error);

        var user = GetUsuarioActual(sesion.Session) ?? usuario;
        var fechaLocal = tpv.GetLocalTime();

        sesion.Tpv = tpv;
        sesion.Usuario = usuario;
        sesion.AbiertaPor = user;
        sesion.ImporteApertura = Math.Round(importeApertura, 2);
        sesion.Apertura = fechaLocal;
        sesion.FechaUltimaModificacion = fechaLocal;

        var mov = new MovimientoCajaTpv(sesion.Session);
        mov.Tipo = TipoMovimientoCajaTpv.Apertura;
        mov.Importe = sesion.ImporteApertura;
        mov.SesionTpv = sesion;
        mov.Fecha = sesion.Apertura;
        mov.Usuario = user;

        var evento = new SesionTpvEvento(sesion.Session);
        evento.Sesion = sesion;
        evento.TipoEvento = TipoEventoSesionTpv.Apertura;
        evento.Usuario = user;
        evento.FechaHora = fechaLocal;
        evento.ImporteNuevo = sesion.ImporteApertura;
        evento.EstadoNuevo = "Abierta";
        evento.Descripcion = $"Apertura de sesión";
        if (user != null) evento.Descripcion += $" por {user.UserName}";

        CalcularImporteEsperado(sesion);
    }

    public void CerrarSesion(SesionTpv sesion, decimal? importeCierreManual = null, string? observaciones = null)
    {
        using var uow = new UnitOfWork(sesion.Session.DataLayer);
        uow.BeginTransaction();
        try
        {
            var sesionInUow = uow.GetObjectByKey<SesionTpv>(sesion.Oid);
            if (sesionInUow == null) throw new InvalidOperationException("La sesión no existe.");

            if (!PuedeCerrarSesion(sesionInUow, out string? error))
                throw new InvalidOperationException(error);

            var user = GetUsuarioActual(uow);
            var fechaCierre = sesionInUow.Tpv?.GetLocalTime() ?? InformacionEmpresaHelper.GetLocalTime(uow);

            var estadoAnterior = sesionInUow.Estado.ToString();
            var importeAnterior = sesionInUow.ImporteEsperado;

            sesionInUow.Cierre = fechaCierre;
            sesionInUow.CerradaPor = user;
            sesionInUow.FechaUltimaModificacion = fechaCierre;
            
            CalcularImporteEsperado(sesionInUow);

            if (importeCierreManual.HasValue)
                sesionInUow.ImporteCierre = Math.Round(importeCierreManual.Value, 2);

            sesionInUow.DiferenciaArqueo = Math.Round(sesionInUow.ImporteCierre - sesionInUow.ImporteEsperado, 2);
            
            if (!string.IsNullOrEmpty(observaciones))
            {
                sesionInUow.Observaciones = string.IsNullOrEmpty(sesionInUow.Observaciones) 
                    ? observaciones 
                    : $"{sesionInUow.Observaciones}\n--- CIERRE ---\n{observaciones}";
            }

            var mov = new MovimientoCajaTpv(uow);
            mov.Tipo = TipoMovimientoCajaTpv.Cierre;
            mov.Importe = sesionInUow.ImporteCierre;
            mov.SesionTpv = sesionInUow;
            mov.Fecha = fechaCierre;
            mov.Usuario = user ?? sesionInUow.Usuario;

            var evento = new SesionTpvEvento(uow);
            evento.Sesion = sesionInUow;
            evento.TipoEvento = TipoEventoSesionTpv.Cierre;
            evento.Usuario = user;
            evento.FechaHora = fechaCierre;
            evento.ImporteAnterior = importeAnterior;
            evento.ImporteNuevo = sesionInUow.ImporteCierre;
            evento.EstadoAnterior = estadoAnterior;
            evento.EstadoNuevo = "Cerrada";
            evento.Descripcion = "Cierre de sesión";
            if (user != null) evento.Descripcion += $" por {user.UserName}";
            if (!string.IsNullOrEmpty(observaciones)) evento.Descripcion += $". Obs: {observaciones}";

            uow.CommitChanges();
            uow.CommitTransaction();
            
            sesion.Reload();
        }
        catch
        {
            uow.RollbackTransaction();
            throw;
        }
    }

    public void ReabrirSesion(SesionTpv sesion, string? motivo = null)
    {
        using var uow = new UnitOfWork(sesion.Session.DataLayer);
        uow.BeginTransaction();
        try
        {
            var sesionInUow = uow.GetObjectByKey<SesionTpv>(sesion.Oid);
            if (sesionInUow == null) throw new InvalidOperationException("La sesión no existe.");

            if (!PuedeReabrirSesion(sesionInUow, out string? error))
                throw new InvalidOperationException(error);

            var fechaLocal = sesionInUow.Tpv?.GetLocalTime() ?? InformacionEmpresaHelper.GetLocalTime(uow);
            var user = GetUsuarioActual(uow);

            var estadoAnterior = sesionInUow.Estado.ToString();
            var importeAnterior = sesionInUow.ImporteCierre;

            sesionInUow.Cierre = null;
            sesionInUow.ReabiertaPor = user;
            sesionInUow.MotivoReapertura = motivo;
            sesionInUow.NumeroReaperturas++;
            sesionInUow.FechaUltimaModificacion = fechaLocal;
            
            sesionInUow.Observaciones += $"\n--- REAPERTURA ---\nFecha: {fechaLocal}\nUsuario: {user?.UserName ?? "Desconocido"}\nMotivo: {motivo ?? "No especificado"}\n------------------";
            
            CalcularImporteEsperado(sesionInUow);

            var evento = new SesionTpvEvento(uow);
            evento.Sesion = sesionInUow;
            evento.TipoEvento = TipoEventoSesionTpv.Reapertura;
            evento.Usuario = user;
            evento.FechaHora = fechaLocal;
            evento.ImporteAnterior = importeAnterior;
            evento.ImporteNuevo = sesionInUow.ImporteEsperado;
            evento.EstadoAnterior = estadoAnterior;
            evento.EstadoNuevo = "Abierta";
            evento.Descripcion = "Reapertura de sesión";
            if (user != null) evento.Descripcion += $" por {user.UserName}";
            if (!string.IsNullOrEmpty(motivo)) evento.Descripcion += $". Motivo: {motivo}";

            uow.CommitChanges();
            uow.CommitTransaction();
            
            sesion.Reload();
        }
        catch
        {
            uow.RollbackTransaction();
            throw;
        }
    }

    public void RegistrarMovimiento(SesionTpv sesion, TipoMovimientoCajaTpv tipo, decimal importe, string? motivo)
    {
        using var uow = new UnitOfWork(sesion.Session.DataLayer);
        uow.BeginTransaction();
        try
        {
            var sesionInUow = uow.GetObjectByKey<SesionTpv>(sesion.Oid);
            if (sesionInUow == null) throw new InvalidOperationException("La sesión no existe.");

            if (sesionInUow.Estado != EstadoSesionTpv.Abierta)
                throw new InvalidOperationException("La sesión no está abierta.");

            if (importe <= 0 && (tipo == TipoMovimientoCajaTpv.Retirada || tipo == TipoMovimientoCajaTpv.Ingreso))
                throw new InvalidOperationException("El importe debe ser mayor que cero.");
            
            if (string.IsNullOrEmpty(motivo) && tipo == TipoMovimientoCajaTpv.Retirada)
                throw new InvalidOperationException("El motivo es obligatorio para retiradas de efectivo.");

            var user = GetUsuarioActual(uow);
            var fechaLocal = sesionInUow.Tpv?.GetLocalTime() ?? InformacionEmpresaHelper.GetLocalTime(uow);
            var importeAnterior = sesionInUow.ImporteEsperado;

            var mov = new MovimientoCajaTpv(uow);
            mov.Tipo = tipo;
            mov.Importe = Math.Round(importe, 2);
            mov.Motivo = motivo;
            mov.SesionTpv = sesionInUow;
            mov.Fecha = fechaLocal;
            mov.Usuario = user;

            CalcularImporteEsperado(sesionInUow);
            sesionInUow.FechaUltimaModificacion = fechaLocal;

            var evento = new SesionTpvEvento(uow);
            evento.Sesion = sesionInUow;
            evento.TipoEvento = tipo switch
            {
                TipoMovimientoCajaTpv.Retirada => TipoEventoSesionTpv.RetiradaEfectivo,
                _ => TipoEventoSesionTpv.MovimientoManual
            };
            evento.Usuario = user;
            evento.FechaHora = fechaLocal;
            evento.ImporteAnterior = importeAnterior;
            evento.ImporteNuevo = sesionInUow.ImporteEsperado;
            evento.Descripcion = $"Movimiento {tipo}: {importe}. Motivo: {motivo}";

            uow.CommitChanges();
            uow.CommitTransaction();
            
            sesion.Reload();
        }
        catch
        {
            uow.RollbackTransaction();
            throw;
        }
    }

    public decimal CalcularVentasTotales(SesionTpv sesion)
    {
        return Math.Round(sesion.FacturasSimplificadas.Sum(f => f.ImporteTotal), 2);
    }

    public decimal CalcularMovimientosTotales(SesionTpv sesion)
    {
        return Math.Round(sesion.Movimientos.Sum(m => m.Tipo switch
        {
            TipoMovimientoCajaTpv.Apertura => m.Importe,
            TipoMovimientoCajaTpv.Ingreso => m.Importe,
            TipoMovimientoCajaTpv.Ajuste => m.Importe, 
            TipoMovimientoCajaTpv.Retirada => -m.Importe,
            _ => 0
        }), 2);
    }

    public decimal CalcularImporteEsperado(SesionTpv sesion)
    {
        var totalVentas = CalcularVentasTotales(sesion);
        var totalMovimientos = CalcularMovimientosTotales(sesion);
        
        sesion.ImporteEsperado = Math.Round(totalVentas + totalMovimientos, 2);
        return sesion.ImporteEsperado;
    }

    public bool PuedeAbrirSesion(TpvBO tpv, out string? error)
    {
        error = null;
        if (tpv == null) { error = "El TPV es obligatorio."; return false; }
        if (!tpv.Activo) { error = "No se puede abrir una sesión en un TPV inactivo."; return false; }

        // Refuerzo de unicidad: comprobar en DB
        var sesionAbierta = tpv.Session.FindObject<SesionTpv>(CriteriaOperator.Parse("Tpv.Oid = ? AND Estado = ?", tpv.Oid, EstadoSesionTpv.Abierta));
        if (sesionAbierta != null)
        {
            error = "Ya existe una sesión abierta para este TPV.";
            return false;
        }

        return true;
    }

    public bool PuedeCerrarSesion(SesionTpv sesion, out string? error)
    {
        error = null;
        if (sesion == null) { error = "La sesión es obligatoria."; return false; }
        if (sesion.Estado != EstadoSesionTpv.Abierta) { error = "La sesión no está abierta."; return false; }
        if (sesion.Tpv == null) { error = "La sesión no tiene un TPV asociado."; return false; }
        return true;
    }

    public bool PuedeReabrirSesion(SesionTpv sesion, out string? error)
    {
        error = null;
        if (sesion == null) { error = "La sesión es obligatoria."; return false; }
        if (sesion.Estado != EstadoSesionTpv.Cerrada) { error = "Solo se pueden reabrir sesiones cerradas."; return false; }
        
        if (sesion.Tpv == null) { error = "La sesión no tiene un TPV asociado."; return false; }
        
        // Refuerzo de unicidad para reapertura
        var sesionAbierta = sesion.Session.FindObject<SesionTpv>(CriteriaOperator.Parse("Tpv.Oid = ? AND Estado = ?", sesion.Tpv.Oid, EstadoSesionTpv.Abierta));
        if (sesionAbierta != null)
        {
            error = "No se puede reabrir la sesión porque ya existe otra sesión abierta para este TPV.";
            return false;
        }

        return true;
    }

    public ApplicationUser? InvokeGetUsuarioActual(Session session) => GetUsuarioActual(session);

    private ApplicationUser? GetUsuarioActual(Session session)
    {
        var security = session.ServiceProvider?.GetService<ISecurityStrategyBase>();
        if (security?.UserId != null)
        {
            return session.GetObjectByKey<ApplicationUser>(security.UserId);
        }
        return null;
    }
}
