using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Ventas;

using erp.Module.Factories;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Customer")]
public class Cliente(Session session) : Tercero(session)
{
    private CondicionesPago _condicionesPago;

    [XafDisplayName("Condiciones de Pago")]
    public CondicionesPago CondicionesPago
    {
        get => _condicionesPago;
        set => SetPropertyValue(nameof(CondicionesPago), ref _condicionesPago, value);
    }

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

    public override string GetPrefijoCodigo()
    {
        return "C";
    }

    private void InitValues()
    {
    }
}