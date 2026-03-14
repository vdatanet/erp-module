using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Common;

[DefaultClassOptions]
[NavigationItem("Common")]
public class City(Session session): BaseEntity(session)
{
    private State _provincia;
    private string _nombre;

    [RuleRequiredField]
    [Association("State-Cities")]
    public State Provincia
    {
        get => _provincia;
        set => SetPropertyValue(nameof(Provincia), ref _provincia, value);
    }
    
    [RuleRequiredField]
    [RuleUniqueValue]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }
}