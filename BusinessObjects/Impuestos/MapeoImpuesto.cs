using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Impuestos;

[DefaultClassOptions]
[NavigationItem("Impuestos")]
[ImageName("BO_List")]
public class MapeoImpuesto(Session session) : EntidadBase(session)
{
    private PosicionFiscal? _posicionFiscal;
    private TipoImpuesto? _impuestoOrigen;

    [Association("PosicionFiscal-Mapeos")]
    [XafDisplayName("Posición Fiscal")]
    public PosicionFiscal? PosicionFiscal
    {
        get => _posicionFiscal;
        set => SetPropertyValue(nameof(PosicionFiscal), ref _posicionFiscal, value);
    }

    [RuleRequiredField]
    [XafDisplayName("Impuesto Origen")]
    public TipoImpuesto? ImpuestoOrigen
    {
        get => _impuestoOrigen;
        set => SetPropertyValue(nameof(ImpuestoOrigen), ref _impuestoOrigen, value);
    }

    [XafDisplayName("Impuestos Destino")]
    [Association("MapeoImpuesto-ImpuestosDestino")]
    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    public XPCollection<TipoImpuesto> ImpuestosDestino => GetCollection<TipoImpuesto>(nameof(ImpuestosDestino));
}
