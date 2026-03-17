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
    private string? _notas;

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [Association("Tarifa-RecursosAlquilables")]
    [XafDisplayName("Recursos Alquilables")]
    public XPCollection<RecursoAlquilable> RecursosAlquilables => GetCollection<RecursoAlquilable>();

    [Association("Tarifa-Detalles")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Detalles")]
    public XPCollection<DetalleTarifa> Detalles => GetCollection<DetalleTarifa>();

    [Association("Tarifa-PreciosDiarios")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Precios Diarios")]
    public XPCollection<PrecioDiario> PreciosDiarios => GetCollection<PrecioDiario>();
}
