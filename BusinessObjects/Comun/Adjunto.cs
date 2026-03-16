using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Planificacion;
using erp.Module.BusinessObjects.Productos;

namespace erp.Module.BusinessObjects.Comun;

[ImageName("BO_FileAttachment")]
[FileAttachment(nameof(FileData))]
public class Adjunto(Session session) : EntidadBase(session)
{
    private Contacto _contact;
    private string _description;
    private FileData _fileData;
    private Oportunidad _opportunity;
    private Producto _product;
    private DocumentoVenta _salesDocument;
    private Tarea _task;

    [Association("Contacto-Adjuntos")]
    [XafDisplayName("Contacto")]
    public Contacto Contacto
    {
        get => _contact;
        set => SetPropertyValue(nameof(Contacto), ref _contact, value);
    }

    [Association("Producto-Adjuntos")]
    [XafDisplayName("Producto")]
    public Producto Producto
    {
        get => _product;
        set => SetPropertyValue(nameof(Producto), ref _product, value);
    }

    [Association("DocumentoVenta-Adjuntos")]
    [XafDisplayName("Documento Venta")]
    public DocumentoVenta DocumentoVenta
    {
        get => _salesDocument;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _salesDocument, value);
    }

    [Association("Tarea-Adjuntos")]
    [XafDisplayName("Tarea")]
    public Tarea Tarea
    {
        get => _task;
        set => SetPropertyValue(nameof(Tarea), ref _task, value);
    }

    [Association("Oportunidad-Adjuntos")]
    [XafDisplayName("Oportunidad")]
    public Oportunidad Oportunidad
    {
        get => _opportunity;
        set => SetPropertyValue(nameof(Oportunidad), ref _opportunity, value);
    }

    [XafDisplayName("Archivo")]
    public FileData FileData
    {
        get => _fileData;
        set => SetPropertyValue(nameof(FileData), ref _fileData, value);
    }

    [Size(1000)]
    [XafDisplayName("Descripción")]
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }
}