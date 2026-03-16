using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Ventas;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Customer")]
public class Cliente(Session session) : Tercero(session)
{
    [Association("Cliente-Oportunidades")]
    [XafDisplayName("Oportunidades")]
    public XPCollection<Oportunidad> Oportunidades => GetCollection<Oportunidad>();

    [Association("Cliente-DocumentosVenta")]
    [XafDisplayName("Documentos de Venta")]
    public XPCollection<DocumentoVenta> DocumentosVenta => GetCollection<DocumentoVenta>();
    
    [XafDisplayName("Presupuestos")] public XPCollection<Presupuesto> Presupuestos => GetCollection<Presupuesto>();

    [XafDisplayName("Pedidos")] public XPCollection<Pedido> Pedidos => GetCollection<Pedido>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
    }
}