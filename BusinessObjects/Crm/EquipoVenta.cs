using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
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
    private decimal _porcentajeComision;
    private decimal _importeComisionFijo;

    [Size(255)]
    [RuleRequiredField("RuleRequiredField_EquipoVenta_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre del Equipo de Venta es obligatorio")]
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

    [XafDisplayName("% Comisión")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal PorcentajeComision
    {
        get => _porcentajeComision;
        set => SetPropertyValue(nameof(PorcentajeComision), ref _porcentajeComision, value);
    }

    [XafDisplayName("Importe Fijo Comisión")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal ImporteComisionFijo
    {
        get => _importeComisionFijo;
        set => SetPropertyValue(nameof(ImporteComisionFijo), ref _importeComisionFijo, value);
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
    [DataSourceCriteria("EsVendedor = true AND Activo = true")]
    public XPCollection<Contacto> Vendedores => GetCollection<Contacto>();
}
