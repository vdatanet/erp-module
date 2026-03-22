using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Compras;
using erp.Module.BusinessObjects.Compras;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.Helpers.Contactos;
using erp.Module.BusinessObjects.Auxiliares;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Vendor")]
public class Acreedor(Session session) : Tercero(session), IPuedeParticiparEnCompras
{
    private CondicionPago? _condicionPago;

    [XafDisplayName("Condiciones de Pago")]
    public CondicionPago? CondicionPago
    {
        get => _condicionPago;
        set => SetPropertyValue(nameof(CondicionPago), ref _condicionPago, value);
    }

    [XafDisplayName("Facturas")]
    public XPCollection<FacturaCompra> Facturas => new(Session, CriteriaOperator.Parse("Proveedor.Oid = ?", Oid));

    public override string GetPrefijoCodigo()
    {
        return "A";
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
        _condicionPago = companyInfo.CondicionPagoPorDefecto;
    }
}
