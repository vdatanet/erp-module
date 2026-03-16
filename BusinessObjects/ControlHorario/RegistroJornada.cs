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
    private TimeSpan _duracion;
    private Empleado _empleado;
    private DateTime? _fechaFin;
    private DateTime _fechaInicio;
    private double? _latitudFin;
    private double? _latitudInicio;
    private double? _longitudFin;
    private double? _longitudInicio;
    private string _notas;
    private string _ubicacionFin;
    private string _ubicacionInicio;

    [RuleRequiredField]
    [Association("Empleado-RegistrosJornada")]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Empleado")]
    public Empleado Empleado
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
            if (SetPropertyValue(nameof(FechaInicio), ref _fechaInicio, value))
            {
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
            if (SetPropertyValue(nameof(FechaFin), ref _fechaFin, value))
            {
                RecalcularDuracion();
                ActualizarEmpleado();
            }
        }
    }
    
    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("RowCount", "3")]
    [XafDisplayName("Notas")]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Duración")]
    [ModelDefault("DisplayFormat", "{0:hh\\:mm}")]
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
    public string UbicacionInicio
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
    public string UbicacionFin
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
        var userId = Session.ServiceProvider.GetRequiredService<ISecurityStrategyBase>().UserId;
        if (userId != null)
        {
            Empleado = Session.FindObject<Empleado>(new BinaryOperator("Usuario.Oid", userId));
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

    private ApplicationUser GetCurrentUser()
    {
        return Session.GetObjectByKey<ApplicationUser>(
            Session.ServiceProvider.GetRequiredService<ISecurityStrategyBase>().UserId);
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