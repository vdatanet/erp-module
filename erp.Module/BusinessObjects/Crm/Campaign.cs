using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Crm;

public class Campaign(Session session): BaseEntity(session)
{
    private string _name;
    private string _description;
    private DateTime _createdDate;  
    private string _notes;
    private bool _isActive;
    private bool _isAvailableInSales;
    private bool _isAvailableInPurchases;

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
    
    
}