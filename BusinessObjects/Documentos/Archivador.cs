using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Documentos;

[DefaultClassOptions]
[NavigationItem("Documentos")]
[ImageName("BO_Folder")]
[XafDisplayName("Archivador")]
public class Archivador(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private string? _descripcion;

    [Size(100)]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripción")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [Association("Archivador-Documentos")]
    public XPCollection<Adjunto> Documentos => GetCollection<Adjunto>(nameof(Documentos));
}
