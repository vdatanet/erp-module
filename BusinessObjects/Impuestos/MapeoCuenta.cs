using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contabilidad;

namespace erp.Module.BusinessObjects.Impuestos;

[DefaultClassOptions]
[NavigationItem("Impuestos")]
[ImageName("BO_List")]
public class MapeoCuenta(Session session) : EntidadBase(session)
{
    private CuentaContable? _cuentaDestino;
    private CuentaContable? _cuentaOrigen;
    private PosicionFiscal? _posicionFiscal;

    [Association("PosicionFiscal-MapeosCuenta")]
    [XafDisplayName("Posición Fiscal")]
    public PosicionFiscal? PosicionFiscal
    {
        get => _posicionFiscal;
        set => SetPropertyValue(nameof(PosicionFiscal), ref _posicionFiscal, value);
    }

    [RuleRequiredField("RuleRequiredField_MapeoCuenta_CuentaOrigen", DefaultContexts.Save, CustomMessageTemplate = "La Cuenta Contable Origen del Mapeo es obligatoria")]
    [XafDisplayName("Cuenta Contable Origen")]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public CuentaContable? CuentaOrigen
    {
        get => _cuentaOrigen;
        set => SetPropertyValue(nameof(CuentaOrigen), ref _cuentaOrigen, value);
    }

    [RuleRequiredField("RuleRequiredField_MapeoCuenta_CuentaDestino", DefaultContexts.Save, CustomMessageTemplate = "La Cuenta Contable Destino del Mapeo es obligatoria")]
    [XafDisplayName("Cuenta Contable Destino")]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public CuentaContable? CuentaDestino
    {
        get => _cuentaDestino;
        set => SetPropertyValue(nameof(CuentaDestino), ref _cuentaDestino, value);
    }
}