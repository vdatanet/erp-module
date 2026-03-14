using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Base.Sales;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Contacts;
using erp.Module.BusinessObjects.Products;

namespace erp.Module.BusinessObjects.Planning;

[DefaultClassOptions]
[NavigationItem("Planning")]
[ImageName("BO_Task")]
public class Task(Session session) : BaseEntity(session)
{
    private string _nombre;
    private string _descripcion;
    //private string _status;
    //private string _priority;
    //private string _type;
    private DateTime _fechaVencimiento;
    private DateTime _fechaInicio;
    private DateTime _fechaFin;
    private ApplicationUser _propietario;
    private ApplicationUser _asignadaA;
    private ApplicationUser _completadaPor;
    private Task _tareaPadre;
    private Contact _contacto;
    private Product _producto;
    private SalesDocument _documentoVenta;
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

    public ApplicationUser Propietario
    {
        get => _propietario;
        set => SetPropertyValue(nameof(Propietario), ref _propietario, value);
    }

    public ApplicationUser AsignadaA
    {
        get => _asignadaA;
        set => SetPropertyValue(nameof(AsignadaA), ref _asignadaA, value);
    }

    public ApplicationUser CompletadaPor
    {
        get => _completadaPor;
        set => SetPropertyValue(nameof(CompletadaPor), ref _completadaPor, value);
    }
    
    [Association("Task-Subtasks")]
    public Task TareaPadre
    {
        get => _tareaPadre;
        set => SetPropertyValue(nameof(TareaPadre), ref _tareaPadre, value);
    }

    [Association("Contact-Tasks")]
    public Contact Contacto
    {
        get => _contacto;
        set => SetPropertyValue(nameof(Contacto), ref _contacto, value);
    }

    [Association("Product-Tasks")]
    public Product Producto
    {
        get => _producto;
        set => SetPropertyValue(nameof(Producto), ref _producto, value);
    }
    
    [Association("SalesDocument-Tasks")]
    public SalesDocument DocumentoVenta
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
    [Association("Task-Subtasks")] 
    public XPCollection<Task> Subtareas => GetCollection<Task>(nameof(Subtareas));

    [Aggregated]
    [Association("Task-Pictures")]
    public XPCollection<Picture> Pictures => GetCollection<Picture>(nameof(Pictures));
    
    [Aggregated]
    [Association("Task-Attachments")]
    public XPCollection<Attachment> Attachments => GetCollection<Attachment>(nameof(Attachments));
}