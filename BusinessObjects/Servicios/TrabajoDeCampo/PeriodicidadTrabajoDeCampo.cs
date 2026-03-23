using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Servicios.TrabajoDeCampo;

[DefaultClassOptions]
[NavigationItem("Servicios")]
[XafDisplayName("Periodicidades")]
public class PeriodicidadTrabajoDeCampo(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private int _intervalo;
    private UnidadIntervalo _unidad;
    private bool _lunes;
    private bool _martes;
    private bool _miercoles;
    private bool _jueves;
    private bool _viernes;
    private bool _sabado;
    private bool _domingo;

    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Intervalo")]
    public int Intervalo
    {
        get => _intervalo;
        set => SetPropertyValue(nameof(Intervalo), ref _intervalo, value);
    }

    [XafDisplayName("Unidad")]
    public UnidadIntervalo Unidad
    {
        get => _unidad;
        set => SetPropertyValue(nameof(Unidad), ref _unidad, value);
    }

    [XafDisplayName("Lunes")]
    public bool Lunes
    {
        get => _lunes;
        set => SetPropertyValue(nameof(Lunes), ref _lunes, value);
    }

    [XafDisplayName("Martes")]
    public bool Martes
    {
        get => _martes;
        set => SetPropertyValue(nameof(Martes), ref _martes, value);
    }

    [XafDisplayName("Miércoles")]
    public bool Miercoles
    {
        get => _miercoles;
        set => SetPropertyValue(nameof(Miercoles), ref _miercoles, value);
    }

    [XafDisplayName("Jueves")]
    public bool Jueves
    {
        get => _jueves;
        set => SetPropertyValue(nameof(Jueves), ref _jueves, value);
    }

    [XafDisplayName("Viernes")]
    public bool Viernes
    {
        get => _viernes;
        set => SetPropertyValue(nameof(Viernes), ref _viernes, value);
    }

    [XafDisplayName("Sábado")]
    public bool Sabado
    {
        get => _sabado;
        set => SetPropertyValue(nameof(Sabado), ref _sabado, value);
    }

    [XafDisplayName("Domingo")]
    public bool Domingo
    {
        get => _domingo;
        set => SetPropertyValue(nameof(Domingo), ref _domingo, value);
    }
}

public enum UnidadIntervalo
{
    Dias,
    Semanas,
    Meses,
    Años
}
