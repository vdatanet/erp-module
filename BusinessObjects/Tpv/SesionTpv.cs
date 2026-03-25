using System.ComponentModel;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Security;
using Microsoft.Extensions.DependencyInjection;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Tpv;

public enum EstadoSesionTpv
{
    Abierta,
    Cerrada
}

[DefaultClassOptions]
[NavigationItem("Tpv")]
[XafDisplayName("Sesión TPV")]
[Persistent("SesionTpv")]
[DefaultProperty(nameof(Apertura))]
public class SesionTpv(Session session) : EntidadBase(session)
{
    private DateTime _apertura;
    private DateTime? _cierre;
    private EstadoSesionTpv _estado;
    private decimal _importeApertura;
    private decimal _importeCierre;
    private decimal _importeEsperado;
    private decimal _diferenciaArqueo;
    private string? _observaciones;
    private Tpv? _tpv;
    private ApplicationUser? _usuario;

    [XafDisplayName("TPV")]
    [RuleRequiredField("RuleRequiredField_SesionTpv_Tpv", DefaultContexts.Save, CustomMessageTemplate = "El TPV de la Sesión es obligatorio")]
    [Association("Tpv-Sesiones")]
    public Tpv? Tpv
    {
        get => _tpv;
        set => SetPropertyValue(nameof(Tpv), ref _tpv, value);
    }

    [XafDisplayName("Usuario")]
    [RuleRequiredField("RuleRequiredField_SesionTpv_Usuario", DefaultContexts.Save, CustomMessageTemplate = "El Usuario de la Sesión es obligatorio")]
    public ApplicationUser? Usuario
    {
        get => _usuario;
        set => SetPropertyValue(nameof(Usuario), ref _usuario, value);
    }

    [XafDisplayName("Apertura")]
    [RuleRequiredField("RuleRequiredField_SesionTpv_Apertura", DefaultContexts.Save, CustomMessageTemplate = "La Fecha de Apertura de la Sesión es obligatoria")]
    public DateTime Apertura
    {
        get => _apertura;
        set => SetPropertyValue(nameof(Apertura), ref _apertura, value);
    }

    [XafDisplayName("Cierre")]
    [Appearance("CierreVisibleWhenClosed", Visibility = ViewItemVisibility.Hide, Criteria = "Estado = 'Abierta'")]
    public DateTime? Cierre
    {
        get => _cierre;
        set => SetPropertyValue(nameof(Cierre), ref _cierre, value);
    }

    [XafDisplayName("Importe Apertura")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal ImporteApertura
    {
        get => _importeApertura;
        set => SetPropertyValue(nameof(ImporteApertura), ref _importeApertura, value);
    }

    [XafDisplayName("Importe Contado")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [Appearance("ImporteCierreVisibleWhenClosed", Visibility = ViewItemVisibility.Hide,
        Criteria = "Estado = 'Abierta'")]
    public decimal ImporteCierre
    {
        get => _importeCierre;
        set => SetPropertyValue(nameof(ImporteCierre), ref _importeCierre, value);
    }

    [XafDisplayName("Importe Esperado")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImporteEsperado
    {
        get => _importeEsperado;
        set => SetPropertyValue(nameof(ImporteEsperado), ref _importeEsperado, value);
    }

    [XafDisplayName("Diferencia Arqueo")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal DiferenciaArqueo
    {
        get => _diferenciaArqueo;
        set => SetPropertyValue(nameof(DiferenciaArqueo), ref _diferenciaArqueo, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [XafDisplayName("Estado")]
    [ModelDefault("AllowEdit", "False")]
    public EstadoSesionTpv Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [Association("SesionTpv-FacturasSimplificadas")]
    [XafDisplayName("Facturas Simplificadas")]
    public XPCollection<FacturaSimplificada> FacturasSimplificadas => GetCollection<FacturaSimplificada>();

    [Association("SesionTpv-Movimientos")]
    [XafDisplayName("Movimientos de Caja")]
    public XPCollection<MovimientoCajaTpv> Movimientos => GetCollection<MovimientoCajaTpv>();

    [Browsable(false)]
    public bool EstaAbierta => Estado == EstadoSesionTpv.Abierta;

    [Browsable(false)]
    public bool EstaCerrada => Estado == EstadoSesionTpv.Cerrada;

    [Browsable(false)]
    [RuleFromBoolProperty("RuleFromBoolProperty_SesionTpv_UnaSolaSesionAbierta", DefaultContexts.Save, "Ya existe una sesión abierta para este TPV.", UsedProperties = "Tpv, Estado")]
    public bool UnaSolaSesionAbierta
    {
        get
        {
            if (Tpv == null || Estado != EstadoSesionTpv.Abierta) return true;
            var sesionAbierta = Session.FindObject<SesionTpv>(CriteriaOperator.Parse("Tpv.Oid = ? AND Estado = ? AND Oid != ?", Tpv.Oid, EstadoSesionTpv.Abierta, Oid));
            return sesionAbierta == null;
        }
    }

    public void AbrirSesion(Tpv tpv, ApplicationUser usuario, decimal importeApertura = 0)
    {
        if (tpv == null) throw new ArgumentNullException(nameof(tpv));
        if (usuario == null) throw new ArgumentNullException(nameof(usuario));

        if (tpv.SesionAbierta)
            throw new InvalidOperationException("Ya existe una sesión abierta para este TPV.");

        // Evitar abrir si ya está abierta (si es una nueva instancia, el estado será Abierta por defecto)
        // Pero este método sirve para inicializar una sesión recién creada.
        Tpv = tpv;
        Usuario = usuario;
        ImporteApertura = importeApertura;
        Apertura = Tpv.GetLocalTime();
        Estado = EstadoSesionTpv.Abierta;

        var mov = new MovimientoCajaTpv(Session);
        mov.Tipo = TipoMovimientoCajaTpv.Apertura;
        mov.Importe = importeApertura;
        mov.SesionTpv = this;
        mov.Fecha = Apertura;
        mov.Usuario = usuario;
        mov.Save();
        
        CalcularImporteEsperado();
    }

    public void CerrarSesionAction()
    {
        CerrarSesion();
        Save();
    }

    public void CerrarSesion(decimal? importeCierreManual = null)
    {
        ValidarCierre();

        Estado = EstadoSesionTpv.Cerrada;
        Cierre = Tpv?.GetLocalTime() ?? InformacionEmpresaHelper.GetLocalTime(Session);
        
        CalcularImporteEsperado();

        if (importeCierreManual.HasValue)
        {
            ImporteCierre = importeCierreManual.Value;
        }

        DiferenciaArqueo = ImporteCierre - ImporteEsperado;

        var mov = new MovimientoCajaTpv(Session);
        mov.Tipo = TipoMovimientoCajaTpv.Cierre;
        mov.Importe = ImporteCierre;
        mov.SesionTpv = this;
        mov.Fecha = Cierre.Value;
        mov.Usuario = Usuario;
        mov.Save();
    }

    public void RegistrarMovimiento(TipoMovimientoCajaTpv tipo, decimal importe, string? motivo)
    {
        if (!EstaAbierta)
            throw new InvalidOperationException("La sesión no está abierta.");

        if (importe <= 0 && (tipo == TipoMovimientoCajaTpv.Retirada || tipo == TipoMovimientoCajaTpv.Ingreso))
            throw new InvalidOperationException("El importe debe ser mayor que cero.");
        
        if (string.IsNullOrEmpty(motivo) && tipo == TipoMovimientoCajaTpv.Retirada)
            throw new InvalidOperationException("El motivo es obligatorio para retiradas de efectivo.");

        var mov = new MovimientoCajaTpv(Session);
        mov.Tipo = tipo;
        mov.Importe = importe;
        mov.Motivo = motivo;
        mov.SesionTpv = this;
        mov.Fecha = Tpv?.GetLocalTime() ?? InformacionEmpresaHelper.GetLocalTime(Session);
        var userId = Session.ServiceProvider?.GetService<ISecurityStrategyBase>()?.UserId;
        mov.Usuario = userId != null ? Session.GetObjectByKey<ApplicationUser>(userId) : null;
        mov.Save();

        CalcularImporteEsperado();
    }

    public void RetirarEfectivoAction(decimal importe, string motivo)
    {
        RegistrarMovimiento(TipoMovimientoCajaTpv.Retirada, importe, motivo);
        Save();
    }

    public void ValidarCierre()
    {
        if (!EstaAbierta)
            throw new InvalidOperationException("La sesión no está abierta.");

        if (Tpv == null)
            throw new InvalidOperationException("La sesión debe tener un TPV asociado.");

        if (Usuario == null)
            throw new InvalidOperationException("La sesión debe tener un Usuario asociado.");

        // Reglas adicionales de negocio podrían ir aquí
    }

    public bool PuedeCerrarSesion()
    {
        return EstaAbierta && Tpv != null && Usuario != null;
    }

    public decimal CalcularImporteEsperado()
    {
        // Importe esperado = Importe de apertura + total de ventas - total retirado + otros ingresos/ajustes
        // Las ventas se suman si están en la colección FacturasSimplificadas vinculada a esta sesión
        var totalVentas = FacturasSimplificadas.Sum(f => f.ImporteTotal);
        
        // Sumamos ingresos, aperturas, cierres (aunque cierre no debería afectar antes de cerrarse, lo incluimos por si acaso)
        // Restamos retiradas
        // Ajustes pueden ser positivos o negativos, pero aquí el modelo los tiene como decimal positivos, 
        // asumiremos que se registra el signo en el importe o tratamos el tipo.
        
        var totalMovimientos = Movimientos.Sum(m => m.Tipo switch
        {
            TipoMovimientoCajaTpv.Apertura => m.Importe,
            TipoMovimientoCajaTpv.Ingreso => m.Importe,
            TipoMovimientoCajaTpv.Ajuste => m.Importe, // Podría ser negativo
            TipoMovimientoCajaTpv.Retirada => -m.Importe,
            _ => 0
        });

        ImporteEsperado = totalVentas + totalMovimientos;
        return ImporteEsperado;
    }

    public void ReabrirSesion()
    {
        if (!EstaCerrada)
            throw new InvalidOperationException("Solo se pueden reabrir sesiones cerradas.");

        if (Tpv?.SesionAbierta ?? false)
            throw new InvalidOperationException("No se puede reabrir la sesión porque ya existe otra sesión abierta para este TPV.");

        // Comprobar si el negocio permite la reapertura (por ejemplo, solo el mismo día)
        // Por ahora lo permitimos sin restricciones adicionales según el requerimiento.
        
        Estado = EstadoSesionTpv.Abierta;
        Cierre = null;
        // Se podría dejar trazabilidad en Observaciones si fuera necesario.
        Observaciones += $"\nSesión reabierta el {Tpv?.GetLocalTime() ?? InformacionEmpresaHelper.GetLocalTime(Session)}";
    }

    public void ReabrirSesionAction()
    {
        ReabrirSesion();
        Save();
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Apertura = Tpv?.GetLocalTime() ?? InformacionEmpresaHelper.GetLocalTime(Session);
        Estado = EstadoSesionTpv.Abierta;
    }
}