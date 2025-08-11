using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Common;

[DefaultClassOptions]
[NavigationItem("Common")]
public class City(Session session): BaseEntity(session)
{
    private State _state;
    private string _name;

    [RuleRequiredField]
    [Association("State-Cities")]
    public State State
    {
        get => _state;
        set => SetPropertyValue(nameof(State), ref _state, value);
    }
    
    [RuleRequiredField]
    [RuleUniqueValue]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }
}