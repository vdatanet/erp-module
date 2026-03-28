using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Tesoreria;

[DefaultClassOptions]
[NavigationItem("Tesorería")]
[ImageName("Business_Cash")]
[XafDisplayName("Medio de Pago")]
[DefaultProperty(nameof(Nombre))]
public class MedioPago(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private bool _esEfectivo;
    private string? _notas;
    private CuentaContable? _cuentaContableCobros;
    private CuentaContable? _cuentaContablePagos;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        CuentaContableCobros ??= companyInfo.CuentaCobrosPorDefecto;
        CuentaContablePagos ??= companyInfo.CuentaPagosPorDefecto;
    }

    [RuleRequiredField("RuleRequiredField_MedioPago_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre del Medio de Pago es obligatorio")]
    [XafDisplayName("Nombre")]
    [Size(255)]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Es Efectivo")]
    public bool EsEfectivo
    {
        get => _esEfectivo;
        set => SetPropertyValue(nameof(EsEfectivo), ref _esEfectivo, value);
    }

    [XafDisplayName("Notas")]
    [Size(SizeAttribute.Unlimited)]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [XafDisplayName("Cuenta Contable de Cobros")]
    public CuentaContable? CuentaContableCobros
    {
        get => _cuentaContableCobros;
        set => SetPropertyValue(nameof(CuentaContableCobros), ref _cuentaContableCobros, value);
    }

    [XafDisplayName("Cuenta Contable de Pagos")]
    public CuentaContable? CuentaContablePagos
    {
        get => _cuentaContablePagos;
        set => SetPropertyValue(nameof(CuentaContablePagos), ref _cuentaContablePagos, value);
    }
}