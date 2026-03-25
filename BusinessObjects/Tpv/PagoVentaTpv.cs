using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.Helpers.Contactos;
using System.ComponentModel;

namespace erp.Module.BusinessObjects.Tpv;

[XafDisplayName("Pago de Venta TPV")]
[Persistent("PagoVentaTpv")]
public class PagoVentaTpv(Session session) : EntidadBase(session)
{
    private VentaTpv? _ventaTpv;
    private MedioPago? _medioPago;
    private decimal _importe;
    private DateTime _fechaHora;
    private string? _referenciaExterna;

    [XafDisplayName("Venta TPV")]
    [Association("VentaTpv-Pagos")]
    public VentaTpv? VentaTpv
    {
        get => _ventaTpv;
        set => SetPropertyValue(nameof(VentaTpv), ref _ventaTpv, value);
    }

    [XafDisplayName("Medio de Pago")]
    [RuleRequiredField]
    public MedioPago? MedioPago
    {
        get => _medioPago;
        set => SetPropertyValue(nameof(MedioPago), ref _medioPago, value);
    }

    [XafDisplayName("Importe")]
    public decimal Importe
    {
        get => _importe;
        set 
        {
            if (SetPropertyValue(nameof(Importe), ref _importe, value) && !IsLoading)
                VentaTpv?.ActualizarTotalPagado();
        }
    }

    [XafDisplayName("Fecha/Hora")]
    public DateTime FechaHora
    {
        get => _fechaHora;
        set => SetPropertyValue(nameof(FechaHora), ref _fechaHora, value);
    }

    [XafDisplayName("Referencia Externa")]
    [Size(255)]
    public string? ReferenciaExterna
    {
        get => _referenciaExterna;
        set => SetPropertyValue(nameof(ReferenciaExterna), ref _referenciaExterna, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaHora = InformacionEmpresaHelper.GetLocalTime(Session);
    }
}
