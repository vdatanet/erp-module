using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Logistica;

[DefaultClassOptions]
[NavigationItem("Logística")]
[XafDisplayName("Método de Entrega")]
[Persistent("MetodoEntrega")]
[DefaultProperty(nameof(Nombre))]
public class MetodoEntrega(Session session) : EntidadBase(session)
{
    private string? _codigo;
    private string? _nombre;
    private bool _estaActivo;
    private string? _descripcion;
    private bool _requiereTransportista;
    private bool _permiteSeguimiento;
    private string? _tiempoEstimado;
    private decimal _costeFijo;
    private decimal _costeVariable;
    private int _ordenPrioridad;
    private Transportista? _transportistaPorDefecto;

    [RuleUniqueValue]
    [RuleRequiredField("RuleRequiredField_MetodoEntrega_Codigo", DefaultContexts.Save)]
    [XafDisplayName("Código")]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [RuleRequiredField("RuleRequiredField_MetodoEntrega_Nombre", DefaultContexts.Save)]
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

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripción")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [XafDisplayName("Requiere transportista")]
    public bool RequiereTransportista
    {
        get => _requiereTransportista;
        set => SetPropertyValue(nameof(RequiereTransportista), ref _requiereTransportista, value);
    }

    [XafDisplayName("Permite seguimiento")]
    public bool PermiteSeguimiento
    {
        get => _permiteSeguimiento;
        set => SetPropertyValue(nameof(PermiteSeguimiento), ref _permiteSeguimiento, value);
    }

    [XafDisplayName("Tiempo estimado")]
    public string? TiempoEstimado
    {
        get => _tiempoEstimado;
        set => SetPropertyValue(nameof(TiempoEstimado), ref _tiempoEstimado, value);
    }

    [XafDisplayName("Coste fijo")]
    public decimal CosteFijo
    {
        get => _costeFijo;
        set => SetPropertyValue(nameof(CosteFijo), ref _costeFijo, value);
    }

    [XafDisplayName("Coste variable")]
    public decimal CosteVariable
    {
        get => _costeVariable;
        set => SetPropertyValue(nameof(CosteVariable), ref _costeVariable, value);
    }

    [XafDisplayName("Orden de prioridad")]
    public int OrdenPrioridad
    {
        get => _ordenPrioridad;
        set => SetPropertyValue(nameof(OrdenPrioridad), ref _ordenPrioridad, value);
    }

    [XafDisplayName("Transportista por defecto")]
    [Association("Transportista-MetodosEntrega")]
    public Transportista? TransportistaPorDefecto
    {
        get => _transportistaPorDefecto;
        set => SetPropertyValue(nameof(TransportistaPorDefecto), ref _transportistaPorDefecto, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        EstaActivo = true;
    }
}
