using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Productos;

[DefaultClassOptions]
[NavigationItem("Productos")]
[XafDisplayName("Plantilla de Atributos")]
[DefaultProperty(nameof(Nombre))]
public class PlantillaAtributo(Session session) : EntidadBase(session)
{
    private string _nombre = string.Empty;
    private bool _estaActivo;

    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Activo")]
    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    [Association("PlantillaAtributo-Lineas"), DevExpress.Xpo.Aggregated]
    [XafDisplayName("Atributos")]
    public XPCollection<PlantillaAtributoLinea> Lineas => GetCollection<PlantillaAtributoLinea>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        EstaActivo = true;
    }
}
