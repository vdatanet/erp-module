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
    private string? _nombre;
    private Pais? _pais;

    [RuleRequiredField("RuleRequiredField_Provincia_Pais", DefaultContexts.Save, CustomMessageTemplate = "El País de la Provincia es obligatorio")]
    [Association("Pais-Provincias")]
    [XafDisplayName("País")]
    public Pais? Pais
    {
        get => _pais;
        set => SetPropertyValue(nameof(Pais), ref _pais, value);
    }

    [RuleRequiredField("RuleRequiredField_Provincia_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre de la Provincia es obligatorio")]
    [RuleUniqueValue]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    public static Provincia? FindByName(Session session, string name, Pais? pais = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        var criteria = (DevExpress.Data.Filtering.CriteriaOperator)new DevExpress.Data.Filtering.BinaryOperator(nameof(Nombre), name.Trim());
        if (pais != null)
        {
            criteria = DevExpress.Data.Filtering.CriteriaOperator.And(criteria, new DevExpress.Data.Filtering.BinaryOperator(nameof(Pais), pais));
        }
        return session.FindObject<Provincia>(criteria);
    }

    [Association("Provincia-Poblaciones")]
    [XafDisplayName("Poblaciones")]
    [VisibleInDetailView(false)]
    public XPCollection<Poblacion> Poblaciones => GetCollection<Poblacion>();
}