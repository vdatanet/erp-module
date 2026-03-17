using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("BO_List")]
[XafDisplayName("Extra")]
public class Extra(Session session) : EntidadBase(session)
{
    private string _nombre;
    private decimal _precioDiario;
    private string _descripcion;

    [Size(255)]
    [RuleRequiredField]
    [RuleUniqueValue]
    [XafDisplayName("Nom")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Preu Diari")]
    public decimal PreuDiari
    {
        get => _precioDiario;
        set => SetPropertyValue(nameof(PreuDiari), ref _precioDiario, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripció")]
    public string Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }
}
