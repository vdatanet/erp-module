using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Productos;

[DefaultClassOptions]
// [NavigationItem("Configuración")]
[XafDisplayName("Opción de Atributo")]
[DefaultProperty(nameof(Valor))]
public class AtributoOpcion(Session session) : EntidadBase(session)
{
    private Atributo _atributo = null!;
    private string _valor = string.Empty;
    private int _orden;

    [Association("Atributo-Opciones")]
    [XafDisplayName("Atributo")]
    [RuleRequiredField]
    public Atributo Atributo
    {
        get => _atributo;
        set => SetPropertyValue(nameof(Atributo), ref _atributo, value);
    }

    [RuleRequiredField]
    [XafDisplayName("Valor")]
    public string Valor
    {
        get => _valor;
        set => SetPropertyValue(nameof(Valor), ref _valor, value);
    }

    [XafDisplayName("Orden")]
    public int Orden
    {
        get => _orden;
        set => SetPropertyValue(nameof(Orden), ref _orden, value);
    }
}
