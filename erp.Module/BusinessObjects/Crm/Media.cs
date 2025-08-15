using System;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Crm;

public class Media(Session session) : BaseEntity(session)
{
    private string _name;
    private string _description;
    private DateTime _createdDate;

    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }

    public DateTime CreatedDate
    {
        get => _createdDate;
        set => SetPropertyValue(nameof(CreatedDate), ref _createdDate, value);
    }

}
