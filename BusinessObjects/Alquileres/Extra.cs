using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("BO_List")]
[XafDisplayName("Extra")]
public class Extra(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private string? _notas;
    private decimal _precioDiario;

    [Size(255)]
    [RuleRequiredField("RuleRequiredField_Extra_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre del Extra es obligatorio")]
    [RuleUniqueValue]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Precio Diario")]
    public decimal PrecioDiario
    {
        get => _precioDiario;
        set => SetPropertyValue(nameof(PrecioDiario), ref _precioDiario, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }
}