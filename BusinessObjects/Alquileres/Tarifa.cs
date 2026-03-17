using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[DefaultProperty(nameof(Nombre))]
[ImageName("BO_Price")]
[XafDisplayName("Tarifa")]
public class Tarifa(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private string? _observaciones;

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Tipo")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(255)]
    [XafDisplayName("Observaciones")]
    public string Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [Association("Tarifa-Alquileres")]
    [XafDisplayName("Alquileres")]
    public XPCollection<Alquiler> Alquileres => GetCollection<Alquiler>();

    [Association("Tarifa-Detalles")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Detalles")]
    public XPCollection<DetalleTarifa> Detalles => GetCollection<DetalleTarifa>();

    [Association("Tarifa-PreciosDiarios")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Precios Diarios")]
    public XPCollection<PrecioDiario> PreciosDiarios => GetCollection<PrecioDiario>();
}
