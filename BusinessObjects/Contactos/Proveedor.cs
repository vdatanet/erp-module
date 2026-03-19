using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Compras;
using erp.Module.BusinessObjects.Compras;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Vendor")]
public class Proveedor(Session session) : Tercero(session)
{
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
}