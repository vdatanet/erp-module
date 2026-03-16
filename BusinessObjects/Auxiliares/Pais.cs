using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Comun;

namespace erp.Module.BusinessObjects.Auxiliares;

[DefaultClassOptions]
[NavigationItem("Auxiliares")]
public class Pais(Session session) : EntidadBase(session)
{
    private string _nombre;

    [RuleRequiredField]
    [RuleUniqueValue]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Association("Pais-Provincias")]
    [XafDisplayName("Provincias")]
    [VisibleInDetailView(false)]
    public XPCollection<Provincia> Provincias => GetCollection<Provincia>();
}