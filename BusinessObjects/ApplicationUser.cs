using System.ComponentModel;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects;

[MapInheritance(MapInheritanceType.ParentTable)]
[DefaultProperty(nameof(UserName))]
public sealed class ApplicationUser : PermissionPolicyUser, ISecurityUserWithLoginInfo, ISecurityUserLockout
{
    private int accessFailedCount;
    private DateTime lockoutEnd;

    public ApplicationUser(Session session) : base(session)
    {
    }

    [Browsable(false)]
    [NonCloneable]
    [Aggregated]
    [Association("User-LoginInfo")]
    public XPCollection<ApplicationUserLoginInfo> LoginInfo => GetCollection<ApplicationUserLoginInfo>();

    [Browsable(false)]
    public int AccessFailedCount
    {
        get => accessFailedCount;
        set => SetPropertyValue(nameof(AccessFailedCount), ref accessFailedCount, value);
    }

    [Browsable(false)]
    public DateTime LockoutEnd
    {
        get => lockoutEnd;
        set => SetPropertyValue(nameof(LockoutEnd), ref lockoutEnd, value);
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