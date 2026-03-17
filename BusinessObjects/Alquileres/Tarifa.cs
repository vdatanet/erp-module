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
    private string _nombre;
    private string _observacions;

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Tipus")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(255)]
    [XafDisplayName("Observacions")]
    public string Observacions
    {
        get => _observacions;
        set => SetPropertyValue(nameof(Observacions), ref _observacions, value);
    }

    [Association("Tarifa-Alquileres")]
    [XafDisplayName("Lloguers")]
    public XPCollection<Alquiler> Alquileres => GetCollection<Alquiler>();

    [Association("Tarifa-Detalls")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Detalls")]
    public XPCollection<DetallTarifa> Detalls => GetCollection<DetallTarifa>();

    [Association("Tarifa-PreusDiaris")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Preus Diaris")]
    public XPCollection<PreuDiari> PreusDiaris => GetCollection<PreuDiari>();
}
