using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Comun;
using erp.Module.BusinessObjects.Productos;

using erp.Module.BusinessObjects.Contactos;

namespace erp.Module.BusinessObjects.Planificacion;

[DefaultClassOptions]
[NavigationItem("Planificacion")]
[ImageName("BO_Tarea")]
public class Tarea(Session session) : EntidadBase(session)
{
    private string _nombre;
    private string _descripcion;
    //private string _status;
    //private string _priority;
    //private string _type;
    private DateTime _fechaVencimiento;
    private DateTime _fechaInicio;
    private DateTime _fechaFin;
    private UsuarioAplicacion _propietario;
    private UsuarioAplicacion _asignadaA;
    private UsuarioAplicacion _completadaPor;
    private Tarea _tareaPadre;
    private Contacto _contacto;
    private Producto _producto;
    private DocumentoVenta _documentoVenta;
    private string _notas;

    [Size(255)]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(1000)]
    public string Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    public DateTime FechaVencimiento
    {
        get => _fechaVencimiento;
        set => SetPropertyValue(nameof(FechaVencimiento), ref _fechaVencimiento, value);
    }

    public DateTime FechaInicio
    {
        get => _fechaInicio;
        set => SetPropertyValue(nameof(FechaInicio), ref _fechaInicio, value);
    }

    public DateTime FechaFin
    {
        get => _fechaFin;
        set => SetPropertyValue(nameof(FechaFin), ref _fechaFin, value);
    }

    public UsuarioAplicacion Propietario
    {
        get => _propietario;
        set => SetPropertyValue(nameof(Propietario), ref _propietario, value);
    }

    public UsuarioAplicacion AsignadaA
    {
        get => _asignadaA;
        set => SetPropertyValue(nameof(AsignadaA), ref _asignadaA, value);
    }

    public UsuarioAplicacion CompletadaPor
    {
        get => _completadaPor;
        set => SetPropertyValue(nameof(CompletadaPor), ref _completadaPor, value);
    }
    
    [Association("Tarea-Subtareas")]
    public Tarea TareaPadre
    {
        get => _tareaPadre;
        set => SetPropertyValue(nameof(TareaPadre), ref _tareaPadre, value);
    }

    [Association("Contacto-Tareas")]
    public Contacto Contacto
    {
        get => _contacto;
        set => SetPropertyValue(nameof(Contacto), ref _contacto, value);
    }

    [Association("Producto-Tareas")]
    public Producto Producto
    {
        get => _producto;
        set => SetPropertyValue(nameof(Producto), ref _producto, value);
    }
    
    [Association("DocumentoVenta-Tareas")]
    public DocumentoVenta DocumentoVenta
    {
        get => _documentoVenta;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _documentoVenta, value);
    }

    [Size(1000)]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [Aggregated]
    [Association("Tarea-Subtareas")]
    public XPCollection<Tarea> Subtareas => GetCollection<Tarea>(nameof(Subtareas));

    [Aggregated]
    [Association("Tarea-Fotos")]
    public XPCollection<Imagen> Imagenes => GetCollection<Imagen>(nameof(Imagenes));
    
    [Aggregated]
    [Association("Tarea-Adjuntos")]
    public XPCollection<Adjunto> Adjuntos => GetCollection<Adjunto>(nameof(Adjuntos));
}