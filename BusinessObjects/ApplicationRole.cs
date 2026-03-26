using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects;

[MapInheritance(MapInheritanceType.ParentTable)]
[XafDisplayName("Rol")]
public class ApplicationRole : PermissionPolicyRole
{
    public ApplicationRole(Session session) : base(session)
    {
    }

    [Association("RolesAutorizados-Tpvs")]
    [XafDisplayName("TPVs Autorizados")]
    public XPCollection<Tpv.Tpv> TpvsAutorizados => GetCollection<Tpv.Tpv>();
}
