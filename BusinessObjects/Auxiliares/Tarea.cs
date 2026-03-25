using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Compras;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Productos;
using erp.Module.Helpers.Contactos;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.BusinessObjects.Auxiliares;

public enum EstadoTarea
{
    [XafDisplayName("Pendiente")] [ImageName("State_Task_NotStarted")]
    Pendiente,

    [XafDisplayName("En Progreso")] [ImageName("State_Task_InProgress")]
    EnProgreso,

    [XafDisplayName("Completada")] [ImageName("State_Task_Completed")]
    Completada,

    [XafDisplayName("Cancelada")] [ImageName("State_Task_Deferred")]
    Cancelada
}

public enum PrioridadTarea
{
    [XafDisplayName("Baja")] [ImageName("State_Priority_Low")]
    Baja,

    [XafDisplayName("Media")] [ImageName("State_Priority_Normal")]
    Media,

    [XafDisplayName("Alta")] [ImageName("State_Priority_High")]
    Alta
}

[DefaultClassOptions]
[NavigationItem("Auxiliares")]
[XafDisplayName("Tarea")]
[XafDefaultProperty(nameof(Nombre))]
[ImageName("BO_Task")]
public class Tarea(Session session) : EntidadBase(session)
{
    private Empleado? _asignadaA;
    private Empleado? _completadaPor;
    private Contacto? _contacto;
    private DocumentoCompra? _documentoCompra;
    private DocumentoVenta? _documentoVenta;
    private EstadoTarea _estado;
    private DateTime _fechaFin;
    private DateTime _fechaInicio;
    private DateTime _fechaVencimiento;
    private string? _nombre;
    private string? _notas;
    private Oportunidad? _oportunidad;
    private int _porcentajeCompletado;
    private PrioridadTarea _prioridad;
    private Producto? _producto;
    private Empleado? _propietario;
    private Tarea? _tareaPadre;

    [Size(255)]
    [RuleRequiredField("RuleRequiredField_Tarea_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre de la Tarea es obligatorio")]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Estado")]
    public EstadoTarea Estado
    {
        get => _estado;
        set
        {
            if (SetPropertyValue(nameof(Estado), ref _estado, value))
                if (!IsLoading && !IsSaving)
                {
                    if (value == EstadoTarea.Completada)
                    {
                        PorcentajeCompletado = 100;
                        FechaFin = InformacionEmpresaHelper.GetLocalTime(Session);
                        CompletadaPor = GetCurrentEmpleado();
                    }
                    else if (value == EstadoTarea.Pendiente && PorcentajeCompletado == 100)
                    {
                        PorcentajeCompletado = 0;
                    }
                }
        }
    }

    [XafDisplayName("Prioridad")]
    public PrioridadTarea Prioridad
    {
        get => _prioridad;
        set => SetPropertyValue(nameof(Prioridad), ref _prioridad, value);
    }

    [XafDisplayName("% Completado")]
    public int PorcentajeCompletado
    {
        get => _porcentajeCompletado;
        set
        {
            if (SetPropertyValue(nameof(PorcentajeCompletado), ref _porcentajeCompletado, value))
                if (!IsLoading && !IsSaving)
                {
                    if (value == 100)
                        Estado = EstadoTarea.Completada;
                    else if (value > 0 && Estado == EstadoTarea.Pendiente) Estado = EstadoTarea.EnProgreso;
                }
        }
    }

    [XafDisplayName("Fecha Vencimiento")]
    public DateTime FechaVencimiento
    {
        get => _fechaVencimiento;
        set => SetPropertyValue(nameof(FechaVencimiento), ref _fechaVencimiento, value);
    }

    [XafDisplayName("Fecha Inicio")]
    public DateTime FechaInicio
    {
        get => _fechaInicio;
        set => SetPropertyValue(nameof(FechaInicio), ref _fechaInicio, value);
    }

    [XafDisplayName("Fecha Fin")]
    [ModelDefault("AllowEdit", "False")]
    public DateTime FechaFin
    {
        get => _fechaFin;
        set => SetPropertyValue(nameof(FechaFin), ref _fechaFin, value);
    }

    [XafDisplayName("Propietario")]
    [Association("Empleado-TareasPropias")]
    public Empleado? Propietario
    {
        get => _propietario;
        set => SetPropertyValue(nameof(Propietario), ref _propietario, value);
    }

    [XafDisplayName("Asignada A")]
    [Association("Empleado-TareasAsignadas")]
    public Empleado? AsignadaA
    {
        get => _asignadaA;
        set => SetPropertyValue(nameof(AsignadaA), ref _asignadaA, value);
    }

    [XafDisplayName("Completada Por")]
    [ModelDefault("AllowEdit", "False")]
    public Empleado? CompletadaPor
    {
        get => _completadaPor;
        set => SetPropertyValue(nameof(CompletadaPor), ref _completadaPor, value);
    }

    [Association("Tarea-Subtareas")]
    [XafDisplayName("Tarea Padre")]
    public Tarea? TareaPadre
    {
        get => _tareaPadre;
        set => SetPropertyValue(nameof(TareaPadre), ref _tareaPadre, value);
    }

    [Association("Contacto-Tareas")]
    [XafDisplayName("Contacto")]
    public Contacto? Contacto
    {
        get => _contacto;
        set => SetPropertyValue(nameof(Contacto), ref _contacto, value);
    }

    [Association("Producto-Tareas")]
    [XafDisplayName("Producto")]
    public Producto? Producto
    {
        get => _producto;
        set => SetPropertyValue(nameof(Producto), ref _producto, value);
    }

    [Association("DocumentoVenta-Tareas")]
    [XafDisplayName("Documento Venta")]
    public DocumentoVenta? DocumentoVenta
    {
        get => _documentoVenta;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _documentoVenta, value);
    }

    [Association("DocumentoCompra-Tareas")]
    [XafDisplayName("Documento Compra")]
    public DocumentoCompra? DocumentoCompra
    {
        get => _documentoCompra;
        set => SetPropertyValue(nameof(DocumentoCompra), ref _documentoCompra, value);
    }

    [Association("Oportunidad-Tareas")]
    [XafDisplayName("Oportunidad")]
    public Oportunidad? Oportunidad
    {
        get => _oportunidad;
        set => SetPropertyValue(nameof(Oportunidad), ref _oportunidad, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [DevExpress.Xpo.Aggregated]
    [Association("Tarea-Subtareas")]
    [XafDisplayName("Subtareas")]
    public XPCollection<Tarea> Subtareas => GetCollection<Tarea>();

    [DevExpress.Xpo.Aggregated]
    [Association("Tarea-Fotos")]
    [XafDisplayName("Imágenes")]
    public XPCollection<Imagen> Imagenes => GetCollection<Imagen>();

    [DevExpress.Xpo.Aggregated]
    [Association("Tarea-Adjuntos")]
    [XafDisplayName("Adjuntos")]
    public XPCollection<Adjunto> Adjuntos => GetCollection<Adjunto>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadoTarea.Pendiente;
        Prioridad = PrioridadTarea.Media;
        Propietario = GetCurrentEmpleado();
    }

    protected override void OnSaving()
    {
        // Validaciones de fechas básicas
        if (FechaInicio != default && FechaVencimiento != default && FechaInicio > FechaVencimiento)
            throw new UserFriendlyException("La Fecha de inicio no puede ser posterior a la Fecha de vencimiento.");

        if (FechaInicio != default && FechaFin != default && FechaInicio > FechaFin)
            throw new UserFriendlyException("La Fecha de inicio no puede ser posterior a la Fecha de fin.");

        if (Propietario == null)
            Propietario = GetCurrentEmpleado();

        base.OnSaving();
    }

    private Empleado? GetCurrentEmpleado()
    {
        try
        {
            var security = Session.ServiceProvider?.GetService<ISecurityStrategyBase>();
            if (security == null || security.UserId == null) return null;
            return Session.FindObject<Empleado>(new BinaryOperator("Usuario.Oid", security.UserId));
        }
        catch
        {
            return null;
        }
    }
}