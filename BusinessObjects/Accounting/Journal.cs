using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Helpers.Contacts;

namespace erp.Module.BusinessObjects.Accounting;

[DefaultClassOptions]
[NavigationItem("Accounting")]
//[ImageName("CustomerContactDirectory")]
[DefaultProperty(nameof(Name))]
public class Journal(Session session) : BaseEntity(session)
{
    private string _name;
    private string _notes;
    private bool _isActive;

    [RuleRequiredField]
    [RuleUniqueValue]
    [Size(255)]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetPropertyValue(nameof(IsActive), ref _isActive, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        IsActive = true;
        var companyInfo = CompanyInfoHelper.GetCompanyInfo(Session);
        if (companyInfo == null) return;
    }
}