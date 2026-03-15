using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.ControlHorario;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Employee")]
public class Empleado(Session session) : Contacto(session)
{
    private bool _estaTrabajando;
    private DateTime? _ultimoRegistroEntrada;
    private DateTime? _ultimoRegistroSalida;
    
    private UsuarioAplicacion _usuario;
    
    [XafDisplayName("Usuario de Aplicación")]
    public UsuarioAplicacion Usuario
    {
        get => _usuario;
        set
        {
            if (_usuario == value) return;
            UsuarioAplicacion anterior = _usuario;
            _usuario = value;
            if (IsLoading) return;
            if (anterior != null && anterior.Empleado == this)
                anterior.Empleado = null;
            if (_usuario != null)
                _usuario.Empleado = this;
            OnChanged(nameof(Usuario), anterior, _usuario);
        }
    }

    [XafDisplayName("¿Está trabajando?")]
    public bool EstaTrabajando
    {
        get => _estaTrabajando;
        set => SetPropertyValue(nameof(EstaTrabajando), ref _estaTrabajando, value);
    }

    [XafDisplayName("Último Registro Entrada")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    public DateTime? UltimoRegistroEntrada
    {
        get => _ultimoRegistroEntrada;
        set => SetPropertyValue(nameof(UltimoRegistroEntrada), ref _ultimoRegistroEntrada, value);
    }

    [XafDisplayName("Último Registro Salida")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    public DateTime? UltimoRegistroSalida
    {
        get => _ultimoRegistroSalida;
        set => SetPropertyValue(nameof(UltimoRegistroSalida), ref _ultimoRegistroSalida, value);
    }
    
    [Association("Empleado-RegistrosJornada")]
    [XafDisplayName("Registros de Tiempo")]
    public XPCollection<RegistroJornada> RegistrosJornada => GetCollection<RegistroJornada>(nameof(RegistrosJornada));
}