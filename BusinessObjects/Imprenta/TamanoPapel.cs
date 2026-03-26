using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Imprenta;

[DefaultClassOptions]
[NavigationItem("Imprenta")]
[XafDisplayName("Tamaño de Papel")]
public class TamanoPapel : EntidadBase
{
    private decimal _alto;

    private decimal _ancho;

    private string? _descripcion;

    private string? _observaciones;

    public TamanoPapel(Session session) : base(session)
    {
    }

    [ImmediatePostData]
    [RuleRange(0.01, 10000, CustomMessageTemplate = "El ancho debe ser mayor que 0")]
    public decimal Ancho
    {
        get => _ancho;
        set => SetPropertyValue(nameof(Ancho), ref _ancho, value);
    }

    [ImmediatePostData]
    [RuleRange(0.01, 10000, CustomMessageTemplate = "el alto debe ser mayor que 0")]
    public decimal Alto
    {
        get => _alto;
        set => SetPropertyValue(nameof(Alto), ref _alto, value);
    }

    [Size(255)]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [ModelDefault("PropertyEditorType", "DevExpress.ExpressApp.Win.Editors.MemoEditStringPropertyEditor")]
    [Size(4000)]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }
}