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

namespace erp.Module.BusinessObjects.Auxiliares;

[ImageName("Images")]
public class Imagen(Session session) : EntidadBase(session)
{
    private Contacto _contact;
    private MediaDataObject _mediaDataObject;
    private string _notes;
    private Oportunidad _opportunity;
    private Producto _product;
    private DocumentoVenta _salesDocument;
    private Tarea _task;

    [Association("Contacto-Fotos")]
    [XafDisplayName("Contacto")]
    public Contacto Contacto
    {
        get => _contact;
        set => SetPropertyValue(nameof(Contacto), ref _contact, value);
    }

    [Association("Producto-Fotos")]
    [XafDisplayName("Producto")]
    public Producto Producto
    {
        get => _product;
        set => SetPropertyValue(nameof(Producto), ref _product, value);
    }

    [Association("DocumentoVenta-Fotos")]
    [XafDisplayName("Documento Venta")]
    public DocumentoVenta DocumentoVenta
    {
        get => _salesDocument;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _salesDocument, value);
    }

    [Association("Tarea-Fotos")]
    [XafDisplayName("Tarea")]
    public Tarea Tarea
    {
        get => _task;
        set => SetPropertyValue(nameof(Tarea), ref _task, value);
    }

    [Association("Oportunidad-Fotos")]
    [XafDisplayName("Oportunidad")]
    public Oportunidad Oportunidad
    {
        get => _opportunity;
        set => SetPropertyValue(nameof(Oportunidad), ref _opportunity, value);
    }

    [XafDisplayName("Imagen")]
    public MediaDataObject MediaDataObject
    {
        get => _mediaDataObject;
        set => SetPropertyValue(nameof(MediaDataObject), ref _mediaDataObject, value);
    }

    [Size(1000)]
    [XafDisplayName("Notas")]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }
}