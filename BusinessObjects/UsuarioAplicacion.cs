using System.ComponentModel;
using System.Text;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects;

[MapInheritance(MapInheritanceType.ParentTable)]
[DefaultProperty("UserName")]
public class UsuarioAplicacion : PermissionPolicyUser, ISecurityUserWithLoginInfo, ISecurityUserLockout {
    int _contadorAccesosFallidos;
    DateTime _finBloqueo;
    private Contactos.Empleado _empleado;

    public UsuarioAplicacion(Session session) : base(session) { }

    [XafDisplayName("Empleado")]
    public Contactos.Empleado Empleado {
        get => _empleado;
        set {
            if (_empleado == value) return;
            Contactos.Empleado anterior = _empleado;
            _empleado = value;
            if (IsLoading) return;
            if (anterior != null && anterior.Usuario == this)
                anterior.Usuario = null;
            if (_empleado != null)
                _empleado.Usuario = this;
            OnChanged(nameof(Empleado), anterior, _empleado);
        }
    }

    [Browsable(false)]
    public int AccessFailedCount {
        get { return _contadorAccesosFallidos; }
        set { SetPropertyValue(nameof(AccessFailedCount), ref _contadorAccesosFallidos, value); }
    }

    [Browsable(false)]
    public DateTime LockoutEnd {
        get { return _finBloqueo; }
        set { SetPropertyValue(nameof(LockoutEnd), ref _finBloqueo, value); }
    }

    [Browsable(false)]
    [NonCloneable]
    [DevExpress.Xpo.Aggregated, Association("Usuario-InformacionInicioSesion")]
    public XPCollection<InformacionInicioSesionUsuario> InformacionInicioSesion {
        get { return GetCollection<InformacionInicioSesionUsuario>(nameof(InformacionInicioSesion)); }
    }

    IEnumerable<ISecurityUserLoginInfo> IOAuthSecurityUser.UserLogins => InformacionInicioSesion.OfType<ISecurityUserLoginInfo>();

    ISecurityUserLoginInfo ISecurityUserWithLoginInfo.CreateUserLoginInfo(string loginProviderName, string providerUserKey) {
        InformacionInicioSesionUsuario result = new InformacionInicioSesionUsuario(Session);
        result.LoginProviderName = loginProviderName;
        result.ProviderUserKey = providerUserKey;
        result.Usuario = this;
        return result;
    }
}
