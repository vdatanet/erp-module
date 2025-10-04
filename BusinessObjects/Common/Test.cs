using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects.Common;

[DefaultClassOptions]
public class Test(Session session) : BaseObject(session)
{
    private string _name;
    
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }
    
}