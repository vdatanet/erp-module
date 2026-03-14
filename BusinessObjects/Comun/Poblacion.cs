using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Comun;

[DefaultClassOptions]
[NavigationItem("Comun")]
public class Poblacion(Session session): EntidadBase(session)
{
    private Provincia _provincia;
    private string _nombre;

    [RuleRequiredField]
    [Association("Provincia-Poblaciones")]
    public Provincia Provincia
    {
        get => _provincia;
        set => SetPropertyValue(nameof(Provincia), ref _provincia, value);
    }

    [RuleRequiredField]
    [RuleUniqueValue]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }
}