using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.ControlHorario;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Employee")]
public class Empleado(Session session) : Contacto(session)
{
    private bool _estaTrabajando;
    private string _ubicacionEntradaActual;
    private DateTime? _ultimoRegistroEntrada;
    private DateTime? _ultimoRegistroSalida;

    private ApplicationUser _usuario;

    [XafDisplayName("Usuario de Aplicación")]
    public ApplicationUser Usuario
    {
        get => _usuario;
        set => SetPropertyValue(nameof(Usuario), ref _usuario, value);
    }

    [XafDisplayName("¿Está trabajando?")]
    [ModelDefault("AllowEdit", "False")]
    public bool EstaTrabajando
    {
        get => _estaTrabajando;
        set => SetPropertyValue(nameof(EstaTrabajando), ref _estaTrabajando, value);
    }

    [XafDisplayName("Ubicación de entrada actual")]
    [ModelDefault("AllowEdit", "False")]
    [Size(SizeAttribute.Unlimited)]
    public string UbicacionEntradaActual
    {
        get => _ubicacionEntradaActual;
        set => SetPropertyValue(nameof(UbicacionEntradaActual), ref _ubicacionEntradaActual, value);
    }

    [XafDisplayName("Último Registro Entrada")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [ModelDefault("AllowEdit", "False")]
    public DateTime? UltimoRegistroEntrada
    {
        get => _ultimoRegistroEntrada;
        set => SetPropertyValue(nameof(UltimoRegistroEntrada), ref _ultimoRegistroEntrada, value);
    }

    [XafDisplayName("Último Registro Salida")]
    [ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "G")]
    [ModelDefault("AllowEdit", "False")]
    public DateTime? UltimoRegistroSalida
    {
        get => _ultimoRegistroSalida;
        set => SetPropertyValue(nameof(UltimoRegistroSalida), ref _ultimoRegistroSalida, value);
    }

    public override string GetPrefijoCodigo()
    {
        return "E";
    }

    [Association("Empleado-RegistrosJornada")]
    [XafDisplayName("Registros de Tiempo")]
    public XPCollection<RegistroJornada> RegistrosJornada => GetCollection<RegistroJornada>();

    [Association("Empleado-TareasPropias")]
    [XafDisplayName("Tareas Propias")]
    public XPCollection<Tarea> TareasPropias => GetCollection<Tarea>();

    [Association("Empleado-TareasAsignadas")]
    [XafDisplayName("Tareas Asignadas")]
    public XPCollection<Tarea> TareasAsignadas => GetCollection<Tarea>();
}