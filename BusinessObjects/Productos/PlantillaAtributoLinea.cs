using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Productos;

[XafDisplayName("Línea de Plantilla")]
public class PlantillaAtributoLinea(Session session) : EntidadBase(session)
{
    private PlantillaAtributo _plantilla = null!;
    private Atributo _atributo = null!;
    private int _orden;
    private bool _esObligatorioOverride;
    private string? _valorPorDefecto;

    [Association("PlantillaAtributo-Lineas")]
    [XafDisplayName("Plantilla")]
    [RuleRequiredField]
    public PlantillaAtributo Plantilla
    {
        get => _plantilla;
        set => SetPropertyValue(nameof(Plantilla), ref _plantilla, value);
    }

    [XafDisplayName("Atributo")]
    [RuleRequiredField]
    public Atributo Atributo
    {
        get => _atributo;
        set => SetPropertyValue(nameof(Atributo), ref _atributo, value);
    }

    [XafDisplayName("Orden")]
    public int Orden
    {
        get => _orden;
        set => SetPropertyValue(nameof(Orden), ref _orden, value);
    }

    [XafDisplayName("Obligatorio (Sobrescribir)")]
    public bool EsObligatorioOverride
    {
        get => _esObligatorioOverride;
        set => SetPropertyValue(nameof(EsObligatorioOverride), ref _esObligatorioOverride, value);
    }

    [XafDisplayName("Valor por defecto")]
    public string? ValorPorDefecto
    {
        get => _valorPorDefecto;
        set => SetPropertyValue(nameof(ValorPorDefecto), ref _valorPorDefecto, value);
    }
}
