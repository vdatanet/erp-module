using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Logistica;

[DefaultClassOptions]
[NavigationItem("Logística")]
[XafDisplayName("Transportista")]
[Persistent("Transportista")]
[DefaultProperty(nameof(Nombre))]
public class Transportista(Session session) : EntidadBase(session)
{
    private string? _codigo;
    private string? _nombre;
    private bool _estaActivo;
    private string? _telefono;
    private string? _email;
    private string? _observaciones;
    private string? _nifCif;
    private string? _personaContacto;
    private string? _direccion;
    private string? _plazoEntregaHabitual;

    [RuleUniqueValue]
    [RuleRequiredField("RuleRequiredField_Transportista_Codigo", DefaultContexts.Save)]
    [XafDisplayName("Código")]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [RuleRequiredField("RuleRequiredField_Transportista_Nombre", DefaultContexts.Save)]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Activo")]
    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    [XafDisplayName("Teléfono")]
    public string? Telefono
    {
        get => _telefono;
        set => SetPropertyValue(nameof(Telefono), ref _telefono, value);
    }

    [XafDisplayName("Email")]
    public string? Email
    {
        get => _email;
        set => SetPropertyValue(nameof(Email), ref _email, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [XafDisplayName("NIF/CIF")]
    public string? NifCif
    {
        get => _nifCif;
        set => SetPropertyValue(nameof(NifCif), ref _nifCif, value);
    }

    [XafDisplayName("Persona de contacto")]
    public string? PersonaContacto
    {
        get => _personaContacto;
        set => SetPropertyValue(nameof(PersonaContacto), ref _personaContacto, value);
    }

    [XafDisplayName("Dirección")]
    public string? Direccion
    {
        get => _direccion;
        set => SetPropertyValue(nameof(Direccion), ref _direccion, value);
    }

    [XafDisplayName("Plazo de entrega habitual")]
    public string? PlazoEntregaHabitual
    {
        get => _plazoEntregaHabitual;
        set => SetPropertyValue(nameof(PlazoEntregaHabitual), ref _plazoEntregaHabitual, value);
    }

    [Association("Transportista-MetodosEntrega")]
    [XafDisplayName("Métodos de Entrega")]
    public XPCollection<MetodoEntrega> MetodosEntrega => GetCollection<MetodoEntrega>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        EstaActivo = true;
    }
}
