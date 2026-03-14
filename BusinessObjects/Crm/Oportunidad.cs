using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Ventas;

namespace erp.Module.BusinessObjects.Crm;

[DefaultClassOptions]
[NavigationItem("Crm")]
[ImageName("BO_Oportunidad")]
public class Oportunidad(Session session) : Contacto(session)
{
    private string _descripcion;
    private Cliente _cliente;
    private Campana _campana;
    private Medio _medio;
    private Origen _fuente;

    [Size(1000)]
    public string Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [Association("Cliente-Oportunidades")]
    public Cliente Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }
    
    public Campana Campana
    {
        get => _campana;
        set => SetPropertyValue(nameof(Campana), ref _campana, value);
    }
    
    public Medio Medio
    {
        get => _medio;
        set => SetPropertyValue(nameof(Medio), ref _medio, value);
    }
    
    public Origen Fuente
    {
        get => _fuente;
        set => SetPropertyValue(nameof(Fuente), ref _fuente, value);
    }
    
    [Association("Oportunidad-PedidoVentas")]
    public XPCollection<PedidoVenta> PedidoVentas => GetCollection<PedidoVenta>(nameof(PedidoVentas));
}