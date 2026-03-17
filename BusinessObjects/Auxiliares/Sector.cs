using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Auxiliares;

[DefaultClassOptions]
[NavigationItem("Auxiliares")]
[ImageName("EnableClustering")]
[XafDisplayName("Sector")]
[DefaultProperty(nameof(Nombre))]
public class Sector(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private string? _notas;

    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    [Size(255)]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Notas")]
    [Size(SizeAttribute.Unlimited)]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }
}