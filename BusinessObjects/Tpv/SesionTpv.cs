using System.ComponentModel;
using System.Linq;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
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

    [XafDisplayName("Importe Cierre")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [Appearance("ImporteCierreVisibleWhenClosed", Visibility = ViewItemVisibility.Hide,
        Criteria = "Estado = 'Abierta'")]
    public decimal ImporteCierre
    {
        get => _importeCierre;
        set => SetPropertyValue(nameof(ImporteCierre), ref _importeCierre, value);
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

    [Browsable(false)]
    public bool EstaAbierta => Estado == EstadoSesionTpv.Abierta;

    [Browsable(false)]
    public bool EstaCerrada => Estado == EstadoSesionTpv.Cerrada;

    public void AbrirSesion(Tpv tpv, ApplicationUser usuario, decimal importeApertura = 0)
    {
        if (tpv == null) throw new ArgumentNullException(nameof(tpv));
        if (usuario == null) throw new ArgumentNullException(nameof(usuario));

        // Evitar abrir si ya está abierta (si es una nueva instancia, el estado será Abierta por defecto)
        // Pero este método sirve para inicializar una sesión recién creada.
        Tpv = tpv;
        Usuario = usuario;
        ImporteApertura = importeApertura;
        Apertura = Tpv.GetLocalTime();
        Estado = EstadoSesionTpv.Abierta;
    }

    /*[Action(Caption = "Cerrar Sesión", TargetObjectsCriteria = "Estado = 'Abierta'",
        ConfirmationMessage = "¿Desea cerrar la sesión?", ImageName = "Action_Close")]*/
    public void CerrarSesion(decimal? importeCierreManual = null)
    {
        ValidarCierre();

        Estado = EstadoSesionTpv.Cerrada;
        Cierre = Tpv?.GetLocalTime() ?? InformacionEmpresaHelper.GetLocalTime(Session);
        
        if (importeCierreManual.HasValue)
        {
            ImporteCierre = importeCierreManual.Value;
        }
        else if (ImporteCierre == 0)
        {
            ImporteCierre = CalcularImporteCierre();
        }
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

    public decimal CalcularImporteCierre()
    {
        // El importe esperado es el de apertura más la suma de todas las facturas simplificadas
        return ImporteApertura + FacturasSimplificadas.Sum(f => f.ImporteTotal);
    }

    public void ReabrirSesion()
    {
        if (!EstaCerrada)
            throw new InvalidOperationException("Solo se pueden reabrir sesiones cerradas.");

        // Comprobar si el negocio permite la reapertura (por ejemplo, solo el mismo día)
        // Por ahora lo permitimos sin restricciones adicionales según el requerimiento.
        
        Estado = EstadoSesionTpv.Abierta;
        Cierre = null;
        // Se podría dejar trazabilidad en Observaciones si fuera necesario.
        Observaciones += $"\nSesión reabierta el {Tpv?.GetLocalTime() ?? InformacionEmpresaHelper.GetLocalTime(Session)}";
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Apertura = Tpv?.GetLocalTime() ?? InformacionEmpresaHelper.GetLocalTime(Session);
        Estado = EstadoSesionTpv.Abierta;
    }
}