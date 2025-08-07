using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects.Common;

[DefaultClassOptions]
public class Sequence(Session session) : BaseObject(session)
{
    private string _name;
    private string _prefix;
    private int _currentValue;
    private int _padding;

    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    public string Prefix
    {
        get => _prefix;
        set => SetPropertyValue(nameof(Prefix), ref _prefix, value);
    }
    
    public int CurrentValue
    {
        get => _currentValue;
        set => SetPropertyValue(nameof(CurrentValue), ref _currentValue, value);
    }

    public int Padding
    {
        get => _padding;
        set => SetPropertyValue(nameof(Padding), ref _padding, value);
    }
}