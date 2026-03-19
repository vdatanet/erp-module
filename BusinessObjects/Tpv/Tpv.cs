using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Tpv;

[DefaultClassOptions]
[NavigationItem("Tpv")]
[XafDisplayName("TPV")]
[Persistent("Tpv")]
public class Tpv(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private string? _codigo;
    private string? _seriePorDefecto;
    private bool _activo;

    [Size(100)]
    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(20)]
    [RuleRequiredField]
    [RuleUniqueValue]
    [XafDisplayName("Código")]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [Size(10)]
    [XafDisplayName("Serie por defecto")]
    public string? SeriePorDefecto
    {
        get => _seriePorDefecto;
        set => SetPropertyValue(nameof(SeriePorDefecto), ref _seriePorDefecto, value);
    }

    [XafDisplayName("Activo")]
    public bool Activo
    {
        get => _activo;
        set => SetPropertyValue(nameof(Activo), ref _activo, value);
    }

    [Association("Tpv-FacturasSimplificadas")]
    [XafDisplayName("Facturas Simplificadas")]
    public XPCollection<FacturaSimplificada> FacturasSimplificadas => GetCollection<FacturaSimplificada>(nameof(FacturasSimplificadas));

    [Association("Tpv-Sesiones")]
    [XafDisplayName("Sesiones")]
    public XPCollection<SesionTpv> Sesiones => GetCollection<SesionTpv>(nameof(Sesiones));

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Activo = true;
    }
}
