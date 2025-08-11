using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Common;

[DefaultClassOptions]
[NavigationItem("Common")]
public class State(Session session) : BaseEntity(session)
{
    private Country _country;
    private string _name;

    [RuleRequiredField]
    [Association("Country-States")]
    public Country Country
    {
        get => _country;
        set => SetPropertyValue(nameof(Country), ref _country, value);
    }

    [RuleRequiredField]
    [RuleUniqueValue]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }
    
    [Association("State-Cities")] 
    public XPCollection<City> Cities => GetCollection<City>(nameof(Cities));
}