using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;

namespace erp.Module.BusinessObjects.Auxiliares;

[DefaultClassOptions]
[NavigationItem("Configuraciones")]
[ImageName("BO_Localization")]
[DefaultProperty(nameof(Nombre))]
public class Nacionalidad(Session session) : EntidadBase(session)
{
    private string? _nombre;

    [Size(255)]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Association("Nacionalidad-Contactos")]
    [XafDisplayName("Contactos")]
    public XPCollection<Contacto> Contactos => GetCollection<Contacto>();
}
