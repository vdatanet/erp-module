using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("BO_Localization")]
[DefaultProperty(nameof(Nom))]
public class Nacionalitat(Session session) : EntidadBase(session)
{
    private string _nom;

    [Size(255)]
    public string Nom
    {
        get => _nom;
        set => SetPropertyValue(nameof(Nom), ref _nom, value);
    }

    [Association("Nacionalitat-Viatgers")]
    public XPCollection<Viatger> Viatgers => GetCollection<Viatger>();
}
