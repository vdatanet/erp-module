using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("BO_Category")]
[DefaultProperty(nameof(Nombre))]
public class TipoAlquiler(Session session) : EntidadBase(session)
{
    private string _nombre;
    private string _descripcion;

    [Size(255)]
    [RuleRequiredField]
    [RuleUniqueValue]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripción")]
    public string Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [Association("TipoAlquiler-Alquileres")]
    [XafDisplayName("Alquileres")]
    public XPCollection<Alquiler> Alquileres => GetCollection<Alquiler>();
}
