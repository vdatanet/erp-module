using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects.Comun;

[DefaultClassOptions]
[NavigationItem("Configuraciones")]
public class Secuencia(Session session) : BaseObject(session)
{
    private string _nombre;
    private string _prefijo;
    private int _valorActual;
    private int _relleno;

    [Indexed(Unique = true)]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    public string Prefijo
    {
        get => _prefijo;
        set => SetPropertyValue(nameof(Prefijo), ref _prefijo, value);
    }

    public int ValorActual
    {
        get => _valorActual;
        set => SetPropertyValue(nameof(ValorActual), ref _valorActual, value);
    }

    public int Relleno
    {
        get => _relleno;
        set => SetPropertyValue(nameof(Relleno), ref _relleno, value);
    }
}