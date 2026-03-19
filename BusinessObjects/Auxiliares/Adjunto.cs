using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Compras;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Productos;

namespace erp.Module.BusinessObjects.Auxiliares;

[DefaultClassOptions]
[NavigationItem("Auxiliares")]
[ImageName("BO_FileAttachment")]
[XafDisplayName("Adjunto")]
[DefaultProperty(nameof(Nombre))]
[FileAttachment(nameof(FileData))]
public class Adjunto(Session session) : EntidadBase(session)
{
    private Contacto? _contact;
    private FileData? _fileData;
    private string? _nombre;
    private string? _notas;
    private Oportunidad? _opportunity;
    private Producto? _product;
    private DocumentoCompra? _purchaseDocument;
    private DocumentoVenta? _salesDocument;
    private Tarea? _task;

    [Association("Contacto-Adjuntos")]
    [XafDisplayName("Contacto")]
    public Contacto? Contacto
    {
        get => _contact;
        set => SetPropertyValue(nameof(Contacto), ref _contact, value);
    }

    [Association("Producto-Adjuntos")]
    [XafDisplayName("Producto")]
    public Producto? Producto
    {
        get => _product;
        set => SetPropertyValue(nameof(Producto), ref _product, value);
    }

    [Association("DocumentoVenta-Adjuntos")]
    [XafDisplayName("Documento Venta")]
    public DocumentoVenta? DocumentoVenta
    {
        get => _salesDocument;
        set => SetPropertyValue(nameof(DocumentoVenta), ref _salesDocument, value);
    }

    [Association("DocumentoCompra-Adjuntos")]
    [XafDisplayName("Documento Compra")]
    public DocumentoCompra? DocumentoCompra
    {
        get => _purchaseDocument;
        set => SetPropertyValue(nameof(DocumentoCompra), ref _purchaseDocument, value);
    }

    [Association("Tarea-Adjuntos")]
    [XafDisplayName("Tarea")]
    public Tarea? Tarea
    {
        get => _task;
        set => SetPropertyValue(nameof(Tarea), ref _task, value);
    }

    [Association("Oportunidad-Adjuntos")]
    [XafDisplayName("Oportunidad")]
    public Oportunidad? Oportunidad
    {
        get => _opportunity;
        set => SetPropertyValue(nameof(Oportunidad), ref _opportunity, value);
    }

    [XafDisplayName("Archivo")]
    public FileData? FileData
    {
        get => _fileData;
        set => SetPropertyValue(nameof(FileData), ref _fileData, value);
    }

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
}