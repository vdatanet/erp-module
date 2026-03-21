using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Crm;

namespace erp.Module.BusinessObjects.Crm;

[DefaultClassOptions]
[NavigationItem("Crm")]
[XafDisplayName("Equipo de Venta")]
[XafDefaultProperty(nameof(Nombre))]
[ImageName("BO_Department")]
public class EquipoVenta(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private ApplicationUser? _responsable;

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Responsable")]
    public ApplicationUser? Responsable
    {
        get => _responsable;
        set => SetPropertyValue(nameof(Responsable), ref _responsable, value);
    }

    [Association("EquipoVenta-Leads")]
    [XafDisplayName("Leads")]
    public XPCollection<Lead> Leads => GetCollection<Lead>(nameof(Leads));

    [Association("EquipoVenta-Oportunidades")]
    [XafDisplayName("Oportunidades")]
    public XPCollection<Oportunidad> Oportunidades => GetCollection<Oportunidad>();

    [Association("EquipoVenta-DocumentosVenta")]
    [XafDisplayName("Documentos de Venta")]
    public XPCollection<DocumentoVenta> DocumentosVenta => GetCollection<DocumentoVenta>();

    [Association("EquipoVenta-Vendedores")]
    [XafDisplayName("Vendedores")]
    public XPCollection<Contacto> Vendedores => GetCollection<Contacto>();
}
