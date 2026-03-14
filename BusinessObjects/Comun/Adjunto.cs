using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Productos;
using erp.Module.BusinessObjects.Planificacion;

namespace erp.Module.BusinessObjects.Comun;

[ImageName("BO_FileAttachment")]
[FileAttachment(nameof(FileData))]
public class Adjunto(Session session) : EntidadBase(session)
{
    private Contacto _contact;
    private Producto _product;
    private DocumentoVenta _salesDocument;
    private Tarea _task;
    private FileData _fileData;
    private string _description;

    [Association("Contacto-Adjuntos")]
    public Contacto Contacto
    {
        get => _contact;
        set => SetPropertyValue(nameof(Contacto), ref _contact, value);
    }

    [Association("Producto-Adjuntos")]
    public Producto Producto
    {
        get => _product;
        set => SetPropertyValue(nameof(Producto), ref _product, value);
    }
    
    [Association("DocumentoVenta-Adjuntos")]
    public DocumentoVenta DocumentoVenta
    {
        get => _salesDocument;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _salesDocument, value);
    }

    [Association("Tarea-Adjuntos")]
    public Tarea Tarea
    {
        get => _task;
        set => SetPropertyValue(nameof(Tarea), ref _task, value);
    }

    public FileData FileData
    {
        get => _fileData;
        set => SetPropertyValue(nameof(FileData), ref _fileData, value);
    }

    [Size(1000)]
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }
}