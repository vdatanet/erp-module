using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.TimeTracking;
using Country = erp.Module.BusinessObjects.Common.Country;
using State = erp.Module.BusinessObjects.Common.State;

namespace erp.Module.BusinessObjects;

[NavigationItem("Contacts")]
[ImageName("BO_Employee")]
[XafDisplayName("Employee")]
[MapInheritance(MapInheritanceType.ParentTable)]
[DefaultProperty(nameof(Name))]
public class ApplicationUser(Session session)
    : PermissionPolicyUser(session), ISecurityUserWithLoginInfo, ISecurityUserLockout
{
    private int _accessFailedCount;
    private DateTime _lockoutEnd;

    [Browsable(false)]
    public int AccessFailedCount
    {
        get => _accessFailedCount;
        set => SetPropertyValue(nameof(AccessFailedCount), ref _accessFailedCount, value);
    }

    [Browsable(false)]
    public DateTime LockoutEnd
    {
        get => _lockoutEnd;
        set => SetPropertyValue(nameof(LockoutEnd), ref _lockoutEnd, value);
    }

    [Browsable(false)]
    [NonCloneable]
    [DevExpress.Xpo.Aggregated]
    [Association("User-LoginInfo")]
    public XPCollection<ApplicationUserLoginInfo> LoginInfo =>
        GetCollection<ApplicationUserLoginInfo>(nameof(LoginInfo));

    private string _name;
    private string _address;
    private Country _country;
    private State _state;
    private City _city;
    private string _phone;
    private string _email;
    private string _website;
    private MediaDataObject _picture;
    private string _notes;

    [Size(255)]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    [Size(255)]
    public string Address
    {
        get => _address;
        set => SetPropertyValue(nameof(Address), ref _address, value);
    }

    public Country Country
    {
        get => _country;
        set => SetPropertyValue(nameof(Country), ref _country, value);
    }

    [DataSourceProperty("Country.States")]
    public State State
    {
        get => _state;
        set => SetPropertyValue(nameof(State), ref _state, value);
    }

    [DataSourceProperty("State.Cities")]
    public City City
    {
        get => _city;
        set => SetPropertyValue(nameof(City), ref _city, value);
    }

    public string Phone
    {
        get => _phone;
        set => SetPropertyValue(nameof(Phone), ref _phone, value);
    }

    public string Email
    {
        get => _email;
        set => SetPropertyValue(nameof(Email), ref _email, value);
    }

    public string Website
    {
        get => _website;
        set => SetPropertyValue(nameof(Website), ref _website, value);
    }

    public MediaDataObject Picture
    {
        get => _picture;
        set => SetPropertyValue(nameof(Picture), ref _picture, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("RowCount", "4")]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }

    [Association("ApplicationUser-TimesheetEntries")]
    [XafDisplayName("Timesheet Entries")]
    public XPCollection<TimesheetEntry> TimesheetEntries => GetCollection<TimesheetEntry>(nameof(TimesheetEntries));

    [Association("ApplicationUser-DailyTimesheets")]
    [XafDisplayName("Daily Timesheets")]
    public XPCollection<DailyTimesheet> DailyTimesheets => GetCollection<DailyTimesheet>(nameof(DailyTimesheets));

    private WorkdayRule _workdayRule;
    private bool _isWorking;
    private DateTime? _lastClockIn;
    private DateTime? _lastClockOut;

    [Association("WorkdayRule-ApplicationUsers")]
    [XafDisplayName("Workday Rule")]
    public WorkdayRule WorkdayRule
    {
        get => _workdayRule;
        set => SetPropertyValue(nameof(WorkdayRule), ref _workdayRule, value);
    }

    [XafDisplayName("Is Working")]
    public bool IsWorking
    {
        get => _isWorking;
        set => SetPropertyValue(nameof(IsWorking), ref _isWorking, value);
    }

    [XafDisplayName("Last Clock In")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    public DateTime? LastClockIn
    {
        get => _lastClockIn;
        set => SetPropertyValue(nameof(LastClockIn), ref _lastClockIn, value);
    }

    [XafDisplayName("Last Clock Out")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    public DateTime? LastClockOut
    {
        get => _lastClockOut;
        set => SetPropertyValue(nameof(LastClockOut), ref _lastClockOut, value);
    }

    IEnumerable<ISecurityUserLoginInfo> IOAuthSecurityUser.UserLogins => LoginInfo.OfType<ISecurityUserLoginInfo>();

    ISecurityUserLoginInfo ISecurityUserWithLoginInfo.CreateUserLoginInfo(string loginProviderName,
        string providerUserKey)
    {
        var result = new ApplicationUserLoginInfo(Session);
        result.LoginProviderName = loginProviderName;
        result.ProviderUserKey = providerUserKey;
        result.User = this;
        return result;
    }
}