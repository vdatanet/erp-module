using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Invoicing;
using erp.Module.BusinessObjects.Sales;
using VeriFactu.Xml.Factu;

namespace erp.Module.BusinessObjects.Contacts;

[DefaultClassOptions]
[NavigationItem("Contacts")]
[ImageName("BO_Customer")]
public class Customer(Session session) : Partner(session)
{
    private IDType _tipoIdentificacion;
    
    [NonCloneable]
    public IDType TipoIdentificacion
    {
        get => _tipoIdentificacion;
        set => SetPropertyValue(nameof(TipoIdentificacion), ref _tipoIdentificacion, value);
    }
    
    [Association("Customer-Opportunities")]
    public XPCollection<Opportunity> Oportunidades => GetCollection<Opportunity>();
    
    //[Association("Customer-SalesOrders")]
    //public XPCollection<SalesOrder> SalesOrders => GetCollection<SalesOrder>(nameof(SalesOrders));

    [Association("Customer-Invoices")]
    public XPCollection<Invoice> Facturas => GetCollection<Invoice>(nameof(Facturas));
    
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