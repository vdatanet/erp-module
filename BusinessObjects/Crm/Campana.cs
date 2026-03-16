using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Crm;

[DefaultClassOptions]
[NavigationItem("Crm")]
[XafDisplayName("Campaña")]
[XafDefaultProperty(nameof(Nombre))]
[ImageName("Shopping_Wallet")]
public class Campana(Session session) : EntidadBase(session)
{
    private string _nombre;
    private string _notas;

    [Size(255)]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }
}