using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
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
public class Documento(Session session) : EntidadBase(session)
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
    private DateTime? _fechaDocumento;
    private EstadoDocumento _estado;
    private string? _tags;

    [Association("Contacto-Documentos")]
    [XafDisplayName("Contacto")]
    public Contacto? Contacto
    {
        get => _contact;
        set
        {
            if (SetPropertyValue(nameof(Contacto), ref _contact, value))
            {
                if (!IsLoading && !IsSaving && value != null)
                {
                    ActualizarTipoDocumento(nameof(Contacto));
                }
            }
        }
    }

    [Association("Producto-Documentos")]
    [XafDisplayName("Producto")]
    public Producto? Producto
    {
        get => _product;
        set
        {
            if (SetPropertyValue(nameof(Producto), ref _product, value))
            {
                if (!IsLoading && !IsSaving && value != null)
                {
                    ActualizarTipoDocumento(nameof(Producto));
                }
            }
        }
    }

    [Association("DocumentoVenta-Documentos")]
    [XafDisplayName("Documento Venta")]
    public DocumentoVenta? DocumentoVenta
    {
        get => _salesDocument;
        set
        {
            if (SetPropertyValue(nameof(DocumentoVenta), ref _salesDocument, value))
            {
                if (!IsLoading && !IsSaving && value != null)
                {
                    ActualizarTipoDocumento(nameof(DocumentoVenta));
                }
            }
        }
    }

    [Association("DocumentoCompra-Documentos")]
    [XafDisplayName("Documento Compra")]
    public DocumentoCompra? DocumentoCompra
    {
        get => _purchaseDocument;
        set
        {
            if (SetPropertyValue(nameof(DocumentoCompra), ref _purchaseDocument, value))
            {
                if (!IsLoading && !IsSaving && value != null)
                {
                    ActualizarTipoDocumento(nameof(DocumentoCompra));
                }
            }
        }
    }

    [Association("Tarea-Documentos")]
    [XafDisplayName("Tarea")]
    public Tarea? Tarea
    {
        get => _task;
        set
        {
            if (SetPropertyValue(nameof(Tarea), ref _task, value))
            {
                if (!IsLoading && !IsSaving && value != null)
                {
                    ActualizarTipoDocumento(nameof(Tarea));
                }
            }
        }
    }

    [Association("Oportunidad-Documentos")]
    [XafDisplayName("Oportunidad")]
    public Oportunidad? Oportunidad
    {
        get => _opportunity;
        set
        {
            if (SetPropertyValue(nameof(Oportunidad), ref _opportunity, value))
            {
                if (!IsLoading && !IsSaving && value != null)
                {
                    ActualizarTipoDocumento(nameof(Oportunidad));
                }
            }
        }
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

    [DevExpress.Xpo.Aggregated]
    [Association("Documento-Tareas")]
    [XafDisplayName("Tareas")]
    public XPCollection<Tarea> Tareas => GetCollection<Tarea>();

    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("Documento-Etiquetas")]
    [XafDisplayName("Etiquetas")]
    public XPCollection<EtiquetaDocumento> Etiquetas => GetCollection<EtiquetaDocumento>();

    private void ActualizarTipoDocumento(string propertyName)
    {
        if (TipoDocumento != null && TipoDocumento.Nombre != "General") return;

        string tipoNombre = propertyName switch
        {
            nameof(DocumentoVenta) => "Factura de Venta",
            nameof(DocumentoCompra) => "Factura de Compra",
            nameof(Producto) => "Producto",
            nameof(Contacto) => "Contacto",
            nameof(Oportunidad) => "Oportunidad",
            nameof(Tarea) => "Tarea",
            _ => "General"
        };

        if (tipoNombre != "General")
        {
            TipoDocumento = Session.FindObject<TipoDocumento>(new DevExpress.Data.Filtering.BinaryOperator(nameof(BusinessObjects.Documentos.TipoDocumento.Nombre), tipoNombre));
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadoDocumento.Borrador;
        FechaDocumento = InformacionEmpresaHelper.GetLocalTime(Session).Date;
    }
}
