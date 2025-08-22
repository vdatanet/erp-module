using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Crm;

public class Source(Session session) : BaseEntity(session)
{
    private string _name;
    private string _description;

    [Size(255)]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    [Size(1000)]
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }
}