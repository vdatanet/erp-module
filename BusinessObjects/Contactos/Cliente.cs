using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Facturacion;
using erp.Module.BusinessObjects.Ventas;
using VeriFactu.Xml.Factu;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Cliente")]
public class Cliente(Session session) : Tercero(session)
{
    private IDType _tipoIdentificacion;
    
    [NonCloneable]
    public IDType TipoIdentificacion
    {
        get => _tipoIdentificacion;
        set => SetPropertyValue(nameof(TipoIdentificacion), ref _tipoIdentificacion, value);
    }
    
    [Association("Customer-Opportunities")]
    public XPCollection<Oportunidad> Oportunidades => GetCollection<Oportunidad>(nameof(Oportunidades));
    
    [Association("Customer-Invoices")]
    public XPCollection<Factura> Facturas => GetCollection<Factura>(nameof(Facturas));
    
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        TipoIdentificacion = IDType.NIF_IVA;
    }
}