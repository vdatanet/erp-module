using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Tpv;

[XafDisplayName("Rol de TPV")]
[Persistent("TpvRol")]
public class TpvRol(Session session) : EntidadBase(session)
{
    private Tpv? _tpv;
    private Guid _rolOid;

    [Association("Tpv-Roles")]
    [XafDisplayName("TPV")]
    public Tpv? Tpv
    {
        get => _tpv;
        set => SetPropertyValue(nameof(Tpv), ref _tpv, value);
    }

    [XafDisplayName("Rol")]
    [DataSourceProperty("RolesDisponibles")]
    [ImmediatePostData]
    [NotMapped]
    public ApplicationRole? Rol
    {
        get => Session.GetObjectByKey<ApplicationRole>(RolOid);
        set => RolOid = value?.Oid ?? Guid.Empty;
    }

    [Browsable(false)]
    public IList<ApplicationRole> RolesDisponibles => Session.GetObjects(Session.GetClassInfo<ApplicationRole>(), null, null, 0, false, true).Cast<ApplicationRole>().ToList();

    [XafDisplayName("Rol OID")]
    [ModelDefault("AllowEdit", "False")]
    public Guid RolOid
    {
        get => _rolOid;
        set => SetPropertyValue(nameof(RolOid), ref _rolOid, value);
    }
}
