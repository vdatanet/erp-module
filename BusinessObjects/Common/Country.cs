using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Common;

[DefaultClassOptions]
[NavigationItem("Common")]
public class Country(Session session) : BaseEntity(session)
{
    private string _name;

    [RuleRequiredField]
    [RuleUniqueValue]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    [Association("Country-States")] public XPCollection<State> States => GetCollection<State>(nameof(States));
}