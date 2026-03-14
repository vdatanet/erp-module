using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Common;

[DefaultClassOptions]
[NavigationItem("Common")]
public class State(Session session) : BaseEntity(session)
{
    private Country _pais;
    private string _nombre;

    [RuleRequiredField]
    [Association("Country-States")]
    public Country Pais
    {
        get => _pais;
        set => SetPropertyValue(nameof(Pais), ref _pais, value);
    }

    [RuleRequiredField]
    [RuleUniqueValue]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }
    
    [Association("State-Cities")] 
    public XPCollection<City> Cities => GetCollection<City>(nameof(Cities));
}