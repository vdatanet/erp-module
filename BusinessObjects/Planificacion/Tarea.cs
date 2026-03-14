using DevExpress.ExpressApp.DC;
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
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(1000)]
    [XafDisplayName("Descripción")]
    public string Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
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
    public DateTime FechaFin
    {
        get => _fechaFin;
        set => SetPropertyValue(nameof(FechaFin), ref _fechaFin, value);
    }

    [XafDisplayName("Propietario")]
    public UsuarioAplicacion Propietario
    {
        get => _propietario;
        set => SetPropertyValue(nameof(Propietario), ref _propietario, value);
    }

    [XafDisplayName("Asignada A")]
    public UsuarioAplicacion AsignadaA
    {
        get => _asignadaA;
        set => SetPropertyValue(nameof(AsignadaA), ref _asignadaA, value);
    }

    [XafDisplayName("Completada Por")]
    public UsuarioAplicacion CompletadaPor
    {
        get => _completadaPor;
        set => SetPropertyValue(nameof(CompletadaPor), ref _completadaPor, value);
    }
    
    [Association("Tarea-Subtareas")]
    [XafDisplayName("Tarea Padre")]
    public Tarea TareaPadre
    {
        get => _tareaPadre;
        set => SetPropertyValue(nameof(TareaPadre), ref _tareaPadre, value);
    }

    [Association("Contacto-Tareas")]
    [XafDisplayName("Contacto")]
    public Contacto Contacto
    {
        get => _contacto;
        set => SetPropertyValue(nameof(Contacto), ref _contacto, value);
    }

    [Association("Producto-Tareas")]
    [XafDisplayName("Producto")]
    public Producto Producto
    {
        get => _producto;
        set => SetPropertyValue(nameof(Producto), ref _producto, value);
    }
    
    [Association("DocumentoVenta-Tareas")]
    [XafDisplayName("Documento Venta")]
    public DocumentoVenta DocumentoVenta
    {
        get => _documentoVenta;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _documentoVenta, value);
    }

    [Size(1000)]
    [XafDisplayName("Notas")]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [DevExpress.Xpo.Aggregated]
    [Association("Tarea-Subtareas")]
    [XafDisplayName("Subtareas")]
    public XPCollection<Tarea> Subtareas => GetCollection<Tarea>(nameof(Subtareas));

    [DevExpress.Xpo.Aggregated]
    [Association("Tarea-Fotos")]
    [XafDisplayName("Imágenes")]
    public XPCollection<Imagen> Imagenes => GetCollection<Imagen>(nameof(Imagenes));
    
    [DevExpress.Xpo.Aggregated]
    [Association("Tarea-Adjuntos")]
    [XafDisplayName("Adjuntos")]
    public XPCollection<Adjunto> Adjuntos => GetCollection<Adjunto>(nameof(Adjuntos));
}