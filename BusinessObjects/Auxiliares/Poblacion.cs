using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Auxiliares;

[DefaultClassOptions]
[NavigationItem("Auxiliares")]
[XafDisplayName("Población")]
[DefaultProperty(nameof(Nombre))]
[ImageName("MapIt")]
public class Poblacion(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private Provincia? _provincia;

    [RuleRequiredField("RuleRequiredField_Poblacion_Provincia", DefaultContexts.Save, CustomMessageTemplate = "La Provincia de la Población es obligatoria")]
    [Association("Provincia-Poblaciones")]
    [XafDisplayName("Provincia")]
    public Provincia? Provincia
    {
        get => _provincia;
        set => SetPropertyValue(nameof(Provincia), ref _provincia, value);
    }

    [RuleRequiredField("RuleRequiredField_Poblacion_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre de la Población es obligatorio")]
    [RuleUniqueValue]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }
}