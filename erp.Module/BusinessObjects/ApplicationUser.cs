using System.ComponentModel;
using System.Text;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.TimeTracking;

namespace erp.Module.BusinessObjects
{
    [NavigationItem("Contacts")]
    [ImageName("BO_Employee")]
    [XafDisplayName("Employee")]
    [MapInheritance(MapInheritanceType.ParentTable)]
    [DefaultProperty(nameof(UserName))]
    public class ApplicationUser : PermissionPolicyUser, ISecurityUserWithLoginInfo, ISecurityUserLockout
    {
        int accessFailedCount;
        DateTime lockoutEnd;

        public ApplicationUser(Session session) : base(session) { }

        [Browsable(false)]
        public int AccessFailedCount
        {
            get { return accessFailedCount; }
            set { SetPropertyValue(nameof(AccessFailedCount), ref accessFailedCount, value); }
        }

        [Browsable(false)]
        public DateTime LockoutEnd
        {
            get { return lockoutEnd; }
            set { SetPropertyValue(nameof(LockoutEnd), ref lockoutEnd, value); }
        }

        [Browsable(false)]
        [NonCloneable]
        [DevExpress.Xpo.Aggregated, Association("User-LoginInfo")]
        public XPCollection<ApplicationUserLoginInfo> LoginInfo
        {
            get { return GetCollection<ApplicationUserLoginInfo>(nameof(LoginInfo)); }
        }
        
        [Association("ApplicationUser-TimesheetEntries")]
        [XafDisplayName("Timesheet Entries")]
        public XPCollection<TimesheetEntry> TimesheetEntries => GetCollection<TimesheetEntry>(nameof(TimesheetEntries));

        IEnumerable<ISecurityUserLoginInfo> IOAuthSecurityUser.UserLogins => LoginInfo.OfType<ISecurityUserLoginInfo>();

        ISecurityUserLoginInfo ISecurityUserWithLoginInfo.CreateUserLoginInfo(string loginProviderName, string providerUserKey)
        {
            ApplicationUserLoginInfo result = new ApplicationUserLoginInfo(Session);
            result.LoginProviderName = loginProviderName;
            result.ProviderUserKey = providerUserKey;
            result.User = this;
            return result;
        }
    }
}
