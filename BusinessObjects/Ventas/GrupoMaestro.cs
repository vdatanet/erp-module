using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[NavigationItem("Ventas")]
[ImageName("BO_Folder")]
[XafDisplayName("Grupo Maestro")]
public class GrupoMaestro(Session session) : EntidadBase(session)
{
    private string? _codigo;
    private string? _nombre;
    private string? _descripcion;
    private GrupoMaestro? _padre;
    private int _orden;
    private bool _activo = true;

    [XafDisplayName("Código")]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

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

    [Association("GrupoMaestro-Hijos")]
    [XafDisplayName("Padre")]
    public GrupoMaestro? Padre
    {
        get => _padre;
        set => SetPropertyValue(nameof(Padre), ref _padre, value);
    }

    [Association("GrupoMaestro-Hijos")]
    [XafDisplayName("Subgrupos")]
    public XPCollection<GrupoMaestro> Hijos => GetCollection<GrupoMaestro>(nameof(Hijos));

    [XafDisplayName("Orden")]
    public int Orden
    {
        get => _orden;
        set => SetPropertyValue(nameof(Orden), ref _orden, value);
    }

    [XafDisplayName("Activo")]
    public bool Activo
    {
        get => _activo;
        set => SetPropertyValue(nameof(Activo), ref _activo, value);
    }
}
