using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects.Configuraciones;

[DefaultClassOptions]
[NavigationItem("Configuraciones")]
[XafDisplayName("Secuencia")]
[DefaultProperty(nameof(Nombre))]
[ImageName("Number")]
public class Secuencia(Session session) : BaseObject(session)
{
    private string? _nombre;
    private string? _prefijo;
    private int _relleno;
    private int _valorActual;

    [Indexed(Unique = true)]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    public string? Prefijo
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