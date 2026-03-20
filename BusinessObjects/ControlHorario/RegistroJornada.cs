using System.Globalization;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.BusinessObjects.ControlHorario;

[DefaultClassOptions]
[NavigationItem("Control Horario")]
[XafDisplayName("Registro de Jornada")]
[ImageName("Time")]
[RuleCriteria("FechaFin >= FechaInicio",
    CustomMessageTemplate = "La fecha de fin debe ser posterior a la fecha de inicio")]
public class RegistroJornada(Session session) : EntidadBase(session)
{
    private string? _motivoModificacion;
    private bool _modificado;
    private TimeSpan _duracion;
    private Empleado? _empleado;
    private DateTime? _fechaFin;
    private DateTime _fechaInicio;
    private double? _latitudFin;
    private double? _latitudInicio;
    private double? _longitudFin;
    private double? _longitudInicio;
    private string? _notas;
    private string? _ubicacionFin;
    private string? _ubicacionInicio;

    [RuleRequiredField]
    [Association("Empleado-RegistrosJornada")]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Empleado")]
    public Empleado? Empleado
    {
        get => _empleado;
        set => SetPropertyValue(nameof(Empleado), ref _empleado, value);
    }

    [RuleRequiredField]
    [ImmediatePostData]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.EditMask), "g")]
    [XafDisplayName("Fecha Inicio")]
    public DateTime FechaInicio
    {
        get => _fechaInicio;
        set
        {
            var oldVal = _fechaInicio;
            if (SetPropertyValue(nameof(FechaInicio), ref _fechaInicio, value))
            {
                if (!IsLoading && !IsSaving && !Session.IsNewObject(this) && oldVal != DateTime.MinValue && (oldVal - value).Duration() > TimeSpan.FromSeconds(1))
                {
                    _modificado = true;
                    OnChanged(nameof(Modificado));
                }
                RecalcularDuracion();
                ActualizarEmpleado();
            }
        }
    }

    [ImmediatePostData]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.EditMask), "g")]
    [XafDisplayName("Fecha Fin")]
    public DateTime? FechaFin
    {
        get => _fechaFin;
        set
        {
            var oldVal = _fechaFin;
            if (SetPropertyValue(nameof(FechaFin), ref _fechaFin, value))
            {
                if (!IsLoading && !IsSaving && !Session.IsNewObject(this) && oldVal != null && value != null && (oldVal.Value - value.Value).Duration() > TimeSpan.FromSeconds(1))
                {
                    _modificado = true;
                    OnChanged(nameof(Modificado));
                }
                RecalcularDuracion();
                ActualizarEmpleado();
            }
        }
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("RowCount", "3")]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [XafDisplayName("Modificado")]
    [ModelDefault("AllowEdit", "False")]
    [ImmediatePostData]
    public bool Modificado
    {
        get => _modificado;
        set => SetPropertyValue(nameof(Modificado), ref _modificado, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("RowCount", "5")]
    [XafDisplayName("Motivo de Modificación")]
    [RuleRequiredField("MotivoModificacionRequired", DefaultContexts.Save,
        TargetCriteria = "Modificado = True",
        CustomMessageTemplate = "Debe indicar el motivo de la modificación")]
    [ImmediatePostData]
    public string? MotivoModificacion
    {
        get => _motivoModificacion;
        set => SetPropertyValue(nameof(MotivoModificacion), ref _motivoModificacion, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Duración")]
    [ModelDefault("DisplayFormat", @"{0:hh\:mm}")]
    [ModelDefault("EditMask", @"hh\:mm")]
    public TimeSpan Duracion
    {
        get => _duracion;
        set => SetPropertyValue(nameof(Duracion), ref _duracion, value);
    }

    [XafDisplayName("Latitud Inicio")]
    [ImmediatePostData]
    public double? LatitudInicio
    {
        get => _latitudInicio;
        set
        {
            if (SetPropertyValue(nameof(LatitudInicio), ref _latitudInicio, value)) ActualizarUbicacionInicio();
        }
    }

    [XafDisplayName("Longitud Inicio")]
    [ImmediatePostData]
    public double? LongitudInicio
    {
        get => _longitudInicio;
        set
        {
            if (SetPropertyValue(nameof(LongitudInicio), ref _longitudInicio, value)) ActualizarUbicacionInicio();
        }
    }

    [XafDisplayName("Latitud Fin")]
    [ImmediatePostData]
    public double? LatitudFin
    {
        get => _latitudFin;
        set
        {
            if (SetPropertyValue(nameof(LatitudFin), ref _latitudFin, value)) ActualizarUbicacionFin();
        }
    }

    [XafDisplayName("Longitud Fin")]
    [ImmediatePostData]
    public double? LongitudFin
    {
        get => _longitudFin;
        set
        {
            if (SetPropertyValue(nameof(LongitudFin), ref _longitudFin, value)) ActualizarUbicacionFin();
        }
    }

    [XafDisplayName("Ubicación Inicio")]
    [ModelDefault("AllowEdit", "False")]
    [Size(SizeAttribute.Unlimited)]
    public string? UbicacionInicio
    {
        get => _ubicacionInicio;
        set
        {
            if (SetPropertyValue(nameof(UbicacionInicio), ref _ubicacionInicio, value)) ActualizarEmpleado();
        }
    }

    [XafDisplayName("Ubicación Fin")]
    [ModelDefault("AllowEdit", "False")]
    [Size(SizeAttribute.Unlimited)]
    public string? UbicacionFin
    {
        get => _ubicacionFin;
        set
        {
            if (SetPropertyValue(nameof(UbicacionFin), ref _ubicacionFin, value)) ActualizarEmpleado();
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
        ActualizarEmpleado();
    }

    private void InitValues()
    {
        try
        {
            var security = Session.ServiceProvider?.GetService<ISecurityStrategyBase>();
            if (security == null || security.UserId == null) return;
            Empleado = Session.FindObject<Empleado>(new BinaryOperator("Usuario.Oid", security.UserId));
        }
        catch
        {
            // Ignorar en contextos sin seguridad o actualización de BD
        }
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        RecalcularDuracion();
    }

    private void RecalcularDuracion()
    {
        Duracion = FechaFin.HasValue && FechaFin.Value >= FechaInicio
            ? FechaFin.Value - FechaInicio
            : TimeSpan.Zero;
    }

    private ApplicationUser? GetCurrentUser()
    {
        try
        {
            var security = Session.ServiceProvider?.GetService<ISecurityStrategyBase>();
            if (security == null || security.UserId == null) return null;
            return Session.GetObjectByKey<ApplicationUser>(security.UserId);
        }
        catch
        {
            return null;
        }
    }

    private void ActualizarUbicacionInicio()
    {
        if (IsLoading || IsSaving || !LatitudInicio.HasValue || !LongitudInicio.HasValue) return;
        UbicacionInicio = string.Format(CultureInfo.InvariantCulture, "{0},{1}", LatitudInicio.Value,
            LongitudInicio.Value);
    }

    private void ActualizarUbicacionFin()
    {
        if (IsLoading || IsSaving || !LatitudFin.HasValue || !LongitudFin.HasValue) return;
        UbicacionFin = string.Format(CultureInfo.InvariantCulture, "{0},{1}", LatitudFin.Value, LongitudFin.Value);
    }

    private void ActualizarEmpleado()
    {
        if (IsLoading || IsSaving || Empleado == null) return;

        if (FechaFin.HasValue)
        {
            Empleado.EstaTrabajando = false;
            Empleado.UbicacionEntradaActual = null;
            Empleado.UltimoRegistroSalida = FechaFin;
        }
        else
        {
            Empleado.EstaTrabajando = true;
            Empleado.UbicacionEntradaActual = UbicacionInicio;
            Empleado.UltimoRegistroEntrada = FechaInicio;
        }
    }
}