using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("BO_List")]
[XafDisplayName("Detalle de Tarifa")]
public class DetalleTarifa(Session session) : EntidadBase(session)
{
    private Tarifa? _tarifa;
    private DateTime _fechaInicio;
    private DateTime _fechaFin;
    private decimal _precio;
    private string? _observaciones;
    private int _temporada;

    [Association("Tarifa-Detalles")]
    [XafDisplayName("Tarifa")]
    public Tarifa Tarifa
    {
        get => _tarifa;
        set => SetPropertyValue(nameof(Tarifa), ref _tarifa, value);
    }

    [XafDisplayName("Desde")]
    [RuleRequiredField]
    public DateTime Desde
    {
        get => _fechaInicio;
        set
        {
            bool modified = SetPropertyValue(nameof(Desde), ref _fechaInicio, value);
            if (modified && !IsLoading)
                Temporada = _fechaInicio.Year;
        }
    }

    [XafDisplayName("Hasta")]
    [RuleRequiredField]
    public DateTime Hasta
    {
        get => _fechaFin;
        set => SetPropertyValue(nameof(Hasta), ref _fechaFin, value);
    }

    [XafDisplayName("Precio")]
    public decimal Precio
    {
        get => _precio;
        set => SetPropertyValue(nameof(Precio), ref _precio, value);
    }

    [Size(255)]
    [XafDisplayName("Observaciones")]
    public string Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Temporada")]
    public int Temporada
    {
        get => _temporada;
        set => SetPropertyValue(nameof(Temporada), ref _temporada, value);
    }
}
