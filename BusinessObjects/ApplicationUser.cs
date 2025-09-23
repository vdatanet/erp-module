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

[MapInheritance(MapInheritanceType.ParentTable)]
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