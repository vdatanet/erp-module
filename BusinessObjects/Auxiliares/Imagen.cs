using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Productos;
using System.ComponentModel;

namespace erp.Module.BusinessObjects.Auxiliares;

[DefaultClassOptions]
[NavigationItem("Auxiliares")]
[ImageName("Images")]
[XafDisplayName("Imagen")]
[DefaultProperty(nameof(Nombre))]
public class Imagen(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private MediaDataObject? _mediaDataObject;
    private string? _notas;
    private Contacto? _contact;
    private Oportunidad? _opportunity;
    private Producto? _product;
    private DocumentoVenta? _salesDocument;
    private Tarea? _task;

    [Size(255)]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [XafDisplayName("Imagen")]
    public MediaDataObject? MediaDataObject
    {
        get => _mediaDataObject;
        set => SetPropertyValue(nameof(MediaDataObject), ref _mediaDataObject, value);
    }

    [Association("Contacto-Fotos")]
    [XafDisplayName("Contacto")]
    public Contacto? Contacto
    {
        get => _contact;
        set => SetPropertyValue(nameof(Contacto), ref _contact, value);
    }

    [Association("Producto-Fotos")]
    [XafDisplayName("Producto")]
    public Producto? Producto
    {
        get => _product;
        set => SetPropertyValue(nameof(Producto), ref _product, value);
    }

    [Association("DocumentoVenta-Fotos")]
    [XafDisplayName("Documento Venta")]
    public DocumentoVenta? DocumentoVenta
    {
        get => _salesDocument;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _salesDocument, value);
    }

    [Association("Tarea-Fotos")]
    [XafDisplayName("Tarea")]
    public Tarea? Tarea
    {
        get => _task;
        set => SetPropertyValue(nameof(Tarea), ref _task, value);
    }

    [Association("Oportunidad-Fotos")]
    [XafDisplayName("Oportunidad")]
    public Oportunidad? Oportunidad
    {
        get => _opportunity;
        set => SetPropertyValue(nameof(Oportunidad), ref _opportunity, value);
    }
}