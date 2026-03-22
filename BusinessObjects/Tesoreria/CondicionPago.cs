using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Tesoreria;

[DefaultClassOptions]
[NavigationItem("Tesorería")]
[ImageName("EmployeeQuickWelcome")]
[XafDisplayName("Condiciones de Pago")]
[DefaultProperty(nameof(Nombre))]
public class CondicionPago(Session session) : EntidadBase(session)
{
    private int _diasEntrePlazos;
    private MedioPago? _medioPago;
    private string? _nombre;
    private string? _notas;
    private int _numeroPlazos;
    private int _plazoPrimerPago;

    [RuleRequiredField("RuleRequiredField_CondicionPago_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre de la Condición de Pago es obligatorio")]
    [Size(255)]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Medio de Pago")]
    public MedioPago? MedioPago
    {
        get => _medioPago;
        set => SetPropertyValue(nameof(MedioPago), ref _medioPago, value);
    }

    [XafDisplayName("Plazo Primer Pago (Días)")]
    public int PlazoPrimerPago
    {
        get => _plazoPrimerPago;
        set => SetPropertyValue(nameof(PlazoPrimerPago), ref _plazoPrimerPago, value);
    }

    [XafDisplayName("Días Entre Plazos")]
    public int DiasEntrePlazos
    {
        get => _diasEntrePlazos;
        set => SetPropertyValue(nameof(DiasEntrePlazos), ref _diasEntrePlazos, value);
    }

    [XafDisplayName("Número de Plazos")]
    public int NumeroPlazos
    {
        get => _numeroPlazos;
        set => SetPropertyValue(nameof(NumeroPlazos), ref _numeroPlazos, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        PlazoPrimerPago = 0;
        DiasEntrePlazos = 0;
        NumeroPlazos = 1;
    }
}