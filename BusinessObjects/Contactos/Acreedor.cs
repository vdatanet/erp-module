using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Compras;
using erp.Module.BusinessObjects.Compras;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Vendor")]
public class Acreedor(Session session) : Tercero(session), IPuedeParticiparEnCompras
{
    private CuentaContable? _cuentaPago;
    private Diario? _diarioCompras;
    private PosicionFiscal? _posicionFiscal;

    [XafDisplayName("Cuenta Contable de Pago")]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public CuentaContable? CuentaPago
    {
        get => _cuentaPago;
        set => SetPropertyValue(nameof(CuentaPago), ref _cuentaPago, value);
    }

    [XafDisplayName("Diario de Compras")]
    [DataSourceCriteria("EstaActivo = True")]
    public Diario? DiarioCompras
    {
        get => _diarioCompras;
        set => SetPropertyValue(nameof(DiarioCompras), ref _diarioCompras, value);
    }


    [XafDisplayName("Posición Fiscal")]
    public PosicionFiscal? PosicionFiscal
    {
        get => _posicionFiscal;
        set => SetPropertyValue(nameof(PosicionFiscal), ref _posicionFiscal, value);
    }

    [XafDisplayName("Facturas")]
    public XPCollection<FacturaCompra> Facturas => new(Session, CriteriaOperator.Parse("Proveedor.Oid = ?", Oid));

    public override string GetPrefijoCodigo()
    {
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return "A";
        return companyInfo.PrefijoAcreedores ?? "A";
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        // InitValues se llama en el base.AfterConstruction()
    }

    protected override void InitValues()
    {
        base.InitValues();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;

        _cuentaPago ??= companyInfo.CuentaPagosPorDefecto ?? CuentaContable;
        _diarioCompras ??= companyInfo.DiarioComprasPorDefecto;
        _posicionFiscal ??= companyInfo.PosicionFiscalPorDefecto;
    }
}
