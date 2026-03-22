using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Auxiliares;

[DefaultClassOptions]
[NavigationItem("Auxiliares")]
[XafDisplayName("País")]
[ImageName("Business_World")]
[DefaultProperty(nameof(Nombre))]
public class Pais(Session session) : EntidadBase(session)
{
    private string? _nombre;

    [RuleRequiredField("RuleRequiredField_Pais_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre del País es obligatorio")]
    [RuleUniqueValue]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    public static Pais? FindByName(Session session, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return session.FindObject<Pais>(new DevExpress.Data.Filtering.BinaryOperator(nameof(Nombre), name.Trim()));
    }

    [Association("Pais-Provincias")]
    [XafDisplayName("Provincias")]
    [VisibleInDetailView(false)]
    public XPCollection<Provincia> Provincias => GetCollection<Provincia>();
}