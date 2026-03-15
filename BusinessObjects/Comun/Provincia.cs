using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Comun;

[DefaultClassOptions]
[NavigationItem("Auxiliares")]
public class Provincia(Session session) : EntidadBase(session)
{
    private Pais _pais;
    private string _nombre;

    [RuleRequiredField]
    [Association("Pais-Provincias")]
    [XafDisplayName("País")]
    public Pais Pais
    {
        get => _pais;
        set => SetPropertyValue(nameof(Pais), ref _pais, value);
    }

    [RuleRequiredField]
    [RuleUniqueValue]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }
    
    [Association("Provincia-Poblaciones")]
    [XafDisplayName("Poblaciones")]
    public XPCollection<Poblacion> Poblaciones => GetCollection<Poblacion>(nameof(Poblaciones));
}