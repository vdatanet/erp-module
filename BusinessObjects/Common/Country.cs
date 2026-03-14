using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Common;

[DefaultClassOptions]
[NavigationItem("Common")]
public class Country(Session session) : BaseEntity(session)
{
    private string _nombre;

    [RuleRequiredField]
    [RuleUniqueValue]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Association("Country-States")] public XPCollection<State> States => GetCollection<State>(nameof(States));
}