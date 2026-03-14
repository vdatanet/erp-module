using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.ControlHorario;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Empleado")]
public class Empleado(Session session) : Contacto(session)
{
    private ReglaJornada _reglaJornadaLaboral;
    private bool _estaTrabajando;
    private DateTime? _ultimoRegistroEntrada;
    private DateTime? _ultimoRegistroSalida;

    [Association("ReglaJornada-Empleados")]
    [XafDisplayName("Regla Jornada Laboral")]
    public ReglaJornada ReglaJornadaLaboral
    {
        get => _reglaJornadaLaboral;
        set => SetPropertyValue(nameof(ReglaJornadaLaboral), ref _reglaJornadaLaboral, value);
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
    
    [Association("Empleado-EntradasParte")]
    [XafDisplayName("Registros de Tiempo")]
    public XPCollection<EntradaParte> RegistrosTiempo => GetCollection<EntradaParte>(nameof(RegistrosTiempo));

    [Association("Empleado-ParteDiarios")]
    [XafDisplayName("Partes Diarios")]
    public XPCollection<ParteDiario> PartesDiarios => GetCollection<ParteDiario>(nameof(PartesDiarios));
}