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
public class Proveedor(Session session) : Tercero(session)
{
    private CondicionPago? _condicionPago;

    [XafDisplayName("Condiciones de Pago")]
    public CondicionPago? CondicionPago
    {
        get => _condicionPago;
        set => SetPropertyValue(nameof(CondicionPago), ref _condicionPago, value);
    }
    [Association("Proveedor-DocumentosCompra")]
    [XafDisplayName("Documentos de Compra")]
    [VisibleInDetailView(false)]
    public XPCollection<DocumentoCompra> DocumentosCompra => GetCollection<DocumentoCompra>();

    [XafDisplayName("Presupuestos")]
    public XPCollection<PresupuestoCompra> Presupuestos => new(Session, CriteriaOperator.Parse("Proveedor = ?", this));

    [XafDisplayName("Pedidos")]
    public XPCollection<PedidoCompra> Pedidos => new(Session, CriteriaOperator.Parse("Proveedor = ?", this));

    [XafDisplayName("Albaranes")]
    public XPCollection<AlbaranCompra> Albaranes => new(Session, CriteriaOperator.Parse("Proveedor = ?", this));

    [XafDisplayName("Facturas")]
    public XPCollection<FacturaCompra> Facturas => new(Session, CriteriaOperator.Parse("Proveedor = ?", this));

    public override string GetPrefijoCodigo()
    {
        return "P";
    }
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        CuentaContable = companyInfo.CuentaProveedoresPorDefecto;
        if (CuentaContable != null && (!CuentaContable.EstaActiva || !CuentaContable.EsAsentable))
        {
            CuentaContable = null;
        }
        _condicionPago = companyInfo.CondicionPagoPorDefecto;
    }
}