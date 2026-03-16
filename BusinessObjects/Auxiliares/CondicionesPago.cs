using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Auxiliares;

[DefaultClassOptions]
[NavigationItem("Configuraciones")]
[ImageName("BO_List")]
[DefaultProperty(nameof(Nombre))]
public class CondicionesPago(Session session) : EntidadBase(session)
{
    private string _nombre;
    private MedioPago _medioPago;
    private int _plazoPrimerPago;
    private int _diasEntrePlazos;
    private int _numeroPlazos;

    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("Medio de Pago")]
    public MedioPago MedioPago
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

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        PlazoPrimerPago = 0;
        DiasEntrePlazos = 0;
        NumeroPlazos = 1;
    }
}
