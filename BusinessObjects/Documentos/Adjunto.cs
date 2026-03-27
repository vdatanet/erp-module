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
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Documentos;

public enum EstadoDocumento
{
    [XafDisplayName("Borrador")] Borrador,
    [XafDisplayName("Validado")] Validado,
    [XafDisplayName("Archivado")] Archivado,
    [XafDisplayName("Anulado")] Anulado
}

[DefaultClassOptions]
[NavigationItem("Documentos")]
[ImageName("BO_FileAttachment")]
[XafDisplayName("Documento")]
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
    private TipoDocumento? _tipoDocumento;
    private Archivador? _archivador;
    private DateTime? _fechaDocumento;
    private EstadoDocumento _estado;
    private string? _tags;

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

    [Association("TipoDocumento-Documentos")]
    [XafDisplayName("Tipo de Documento")]
    public TipoDocumento? TipoDocumento
    {
        get => _tipoDocumento;
        set => SetPropertyValue(nameof(TipoDocumento), ref _tipoDocumento, value);
    }

    [Association("Archivador-Documentos")]
    [XafDisplayName("Archivador")]
    public Archivador? Archivador
    {
        get => _archivador;
        set => SetPropertyValue(nameof(Archivador), ref _archivador, value);
    }

    [XafDisplayName("Fecha del Documento")]
    public DateTime? FechaDocumento
    {
        get => _fechaDocumento;
        set => SetPropertyValue(nameof(FechaDocumento), ref _fechaDocumento, value);
    }

    [XafDisplayName("Estado")]
    public EstadoDocumento Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Etiquetas (Tags)")]
    public string? Tags
    {
        get => _tags;
        set => SetPropertyValue(nameof(Tags), ref _tags, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadoDocumento.Borrador;
        FechaDocumento = InformacionEmpresaHelper.GetLocalTime(Session).Date;
    }
}