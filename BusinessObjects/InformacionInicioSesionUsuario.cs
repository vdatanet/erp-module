using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects;

[DeferredDeletion(false)]
public class InformacionInicioSesionUsuario : BaseObject, ISecurityUserLoginInfo {
    string _nombreProveedorInicioSesion;
    UsuarioAplicacion _usuario;
    string _claveUsuarioProveedor;
    public InformacionInicioSesionUsuario(Session session) : base(session) { }

    [Indexed("ProviderUserKey", Unique = true)]
    [Appearance("PasswordProvider", Enabled = false, Criteria = "!(IsNewObject(this)) and LoginProviderName == '" + SecurityDefaults.PasswordAuthentication + "'", Context = "DetailView")]
    public string LoginProviderName {
        get { return _nombreProveedorInicioSesion; }
        set { SetPropertyValue(nameof(LoginProviderName), ref _nombreProveedorInicioSesion, value); }
    }

    [Appearance("PasswordProviderUserKey", Enabled = false, Criteria = "!(IsNewObject(this)) and LoginProviderName == '" + SecurityDefaults.PasswordAuthentication + "'", Context = "DetailView")]
    public string ProviderUserKey {
        get { return _claveUsuarioProveedor; }
        set { SetPropertyValue(nameof(ProviderUserKey), ref _claveUsuarioProveedor, value); }
    }

    [Association("Usuario-InformacionInicioSesion")]
    public UsuarioAplicacion Usuario {
        get { return _usuario; }
        set { SetPropertyValue(nameof(Usuario), ref _usuario, value); }
    }

    object ISecurityUserLoginInfo.User => Usuario;
}
