using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Ventas;

namespace erp.Module.BusinessObjects.Crm;

[DefaultClassOptions]
[NavigationItem("Crm")]
[ImageName("BO_Lead")]
public class Oportunidad(Session session) : EntidadBase(session)
{
    private string _descripcion;
    private Cliente _cliente;
    private Campana _campana;
    private Medio _medio;
    private Origen _fuente;

    [Size(1000)]
    [XafDisplayName("Descripción")]
    public string Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [Association("Cliente-Oportunidades")]
    [XafDisplayName("Cliente")]
    public Cliente Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [XafDisplayName("Campaña")]
    public Campana Campana
    {
        get => _campana;
        set => SetPropertyValue(nameof(Campana), ref _campana, value);
    }

    [XafDisplayName("Medio")]
    public Medio Medio
    {
        get => _medio;
        set => SetPropertyValue(nameof(Medio), ref _medio, value);
    }

    [XafDisplayName("Fuente")]
    public Origen Fuente
    {
        get => _fuente;
        set => SetPropertyValue(nameof(Fuente), ref _fuente, value);
    }

    [Association("Oportunidad-Presupuestos")]
    [XafDisplayName("Presupuestos")]
    public XPCollection<Presupuesto> Presupuestos => GetCollection<Presupuesto>();
}