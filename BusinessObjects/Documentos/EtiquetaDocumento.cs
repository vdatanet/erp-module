using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Documentos;

[DefaultClassOptions]
[NavigationItem("Documentos")]
[ImageName("BO_Label")]
[XafDisplayName("Etiqueta de Documento")]
public class EtiquetaDocumento(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private string? _color;

    [Size(100)]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(50)]
    [XafDisplayName("Color")]
    public string? Color
    {
        get => _color;
        set => SetPropertyValue(nameof(Color), ref _color, value);
    }

    [Association("Documento-Etiquetas")]
    [XafDisplayName("Documentos")]
    public XPCollection<Documento> Documentos => GetCollection<Documento>(nameof(Documentos));
}
