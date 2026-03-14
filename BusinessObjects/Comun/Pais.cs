using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Comun;

[DefaultClassOptions]
[NavigationItem("Comun")]
public class Pais(Session session) : EntidadBase(session)
{
    private string _nombre;

    [RuleRequiredField]
    [RuleUniqueValue]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Association("Country-States")] 
    public XPCollection<Provincia> Provincias => GetCollection<Provincia>(nameof(Provincias));
}