using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("BO_Localization")]
[DefaultProperty(nameof(Nombre))]
public class Nacionalidad(Session session) : EntidadBase(session)
{
    private string _nombre;

    [Size(255)]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Association("Nacionalidad-Viajeros")]
    [XafDisplayName("Viajeros")]
    public XPCollection<Viajero> Viajeros => GetCollection<Viajero>();
}
