using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.TimeTracking;

namespace erp.Module.BusinessObjects.Projects;

[DefaultClassOptions]
[NavigationItem("Projects")]
[ImageName("BO_Project")]
[XafDisplayName("Project")]
public class Project(Session session) : BaseEntity(session)
{
    private string _code;
    private string _name;
    private string _description;
    private bool _isActive = true;

    [Size(64)]
    [RuleRequiredField]
    [RuleUniqueValue]
    [XafDisplayName("Code")]
    public string Code
    {
        get => _code;
        set => SetPropertyValue(nameof(Code), ref _code, value);
    }

    [Size(256)]
    [RuleRequiredField]
    [XafDisplayName("Name")]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("RowCount", "4")]
    [XafDisplayName("Description")]
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }

    [XafDisplayName("Active")]
    public bool IsActive
    {
        get => _isActive;
        set => SetPropertyValue(nameof(IsActive), ref _isActive, value);
    }

    [Association("Project-Activities")]
    [XafDisplayName("Activities")]
    public XPCollection<ProjectActivity> Activities => GetCollection<ProjectActivity>(nameof(Activities));

    [Association("Project-TimesheetEntries")]
    [XafDisplayName("Partes de tiempo")]
    public XPCollection<TimesheetEntry> TimesheetEntries => GetCollection<TimesheetEntry>(nameof(TimesheetEntries));
}