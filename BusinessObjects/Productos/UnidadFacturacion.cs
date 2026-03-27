using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Productos;

[DefaultClassOptions]
[NavigationItem("Productos")]
[XafDisplayName("Unidad de Facturación")]
[ImageName("BO_List")]
[DefaultProperty(nameof(Nombre))]
public class UnidadFacturacion(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private string? _abreviatura;

    [RuleRequiredField("RuleRequiredField_UnidadFacturacion_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre de la Unidad es obligatorio")]
    [RuleUniqueValue]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Abreviatura")]
    public string? Abreviatura
    {
        get => _abreviatura;
        set => SetPropertyValue(nameof(Abreviatura), ref _abreviatura, value);
    }

    public static UnidadFacturacion? FindByName(Session session, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        return session.FindObject<UnidadFacturacion>(new DevExpress.Data.Filtering.BinaryOperator(nameof(Nombre), name.Trim()));
    }
}
