using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects;

[DeferredDeletion(false)]
[Persistent("PermissionPolicyUserLoginInfo")]
public class ApplicationUserLoginInfo : BaseObject, ISecurityUserLoginInfo
{
    private string loginProviderName;
    private string providerUserKey;
    private ApplicationUser user;

    public ApplicationUserLoginInfo(Session session) : base(session)
    {
    }

    [Association("User-LoginInfo")]
    public ApplicationUser User
    {
        get => user;
        set => SetPropertyValue(nameof(User), ref user, value);
    }

    [Indexed("ProviderUserKey", Unique = true)]
    [Appearance("PasswordProvider", Enabled = false,
        Criteria = "!(IsNewObject(this)) and LoginProviderName == '" + SecurityDefaults.PasswordAuthentication + "'",
        Context = "DetailView")]
    public string LoginProviderName
    {
        get => loginProviderName;
        set => SetPropertyValue(nameof(LoginProviderName), ref loginProviderName, value);
    }

    [Appearance("PasswordProviderUserKey", Enabled = false,
        Criteria = "!(IsNewObject(this)) and LoginProviderName == '" + SecurityDefaults.PasswordAuthentication + "'",
        Context = "DetailView")]
    public string ProviderUserKey
    {
        get => providerUserKey;
        set => SetPropertyValue(nameof(ProviderUserKey), ref providerUserKey, value);
    }

    object ISecurityUserLoginInfo.User => User;
}