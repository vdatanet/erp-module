using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Auxiliares;

[DefaultClassOptions]
[NavigationItem("Auxiliares")]
[XafDisplayName("Provincia")]
[XafDefaultProperty(nameof(Nombre))]
[ImageName("Travel_Map")]
public class Provincia(Session session) : EntidadBase(session)
{
    private string _nombre;
    private Pais _pais;

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
    [VisibleInDetailView(false)]
    public XPCollection<Poblacion> Poblaciones => GetCollection<Poblacion>();
}