using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Tesoreria;

[NonPersistent]
[Appearance("BlockLiquidatedEffect", AppearanceItemType = "ViewItem", TargetItems = "Importe,FechaVencimiento,Factura,FacturaCompra",
    Criteria = "Estado = 'Cobrado' or Estado = 'Pagado'", Context = "Any", Enabled = false)]
public abstract class EfectoBase(Session session) : EntidadBase(session)
{
    private decimal _importe;
    private DateTime _fechaVencimiento;
    private EstadoEfecto _estado;
    private string? _observaciones;
    private DateTime? _fechaLiquidacion;

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Importe")]
    public decimal Importe
    {
        get => _importe;
        set => SetPropertyValue(nameof(Importe), ref _importe, value);
    }

    [RuleRequiredField]
    [XafDisplayName("Fecha Vencimiento")]
    public DateTime FechaVencimiento
    {
        get => _fechaVencimiento;
        set => SetPropertyValue(nameof(FechaVencimiento), ref _fechaVencimiento, value);
    }

    [XafDisplayName("Estado")]
    public EstadoEfecto Estado
    {
        get => _estado;
        set
        {
            if (!SetPropertyValue(nameof(Estado), ref _estado, value)) return;
            if (IsLoading || IsSaving) return;
            if (value is EstadoEfecto.Cobrado or EstadoEfecto.Pagado)
            {
                if (!FechaLiquidacion.HasValue)
                    FechaLiquidacion = DateTime.Today;
            }
            else if (value == EstadoEfecto.Pendiente)
            {
                FechaLiquidacion = null;
            }
        }
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [XafDisplayName("Fecha Liquidación")]
    public DateTime? FechaLiquidacion
    {
        get => _fechaLiquidacion;
        set
        {
            if (!SetPropertyValue(nameof(FechaLiquidacion), ref _fechaLiquidacion, value)) return;
            if (IsLoading || IsSaving) return;
            if (value.HasValue)
            {
                if (this is EfectoCobro)
                    Estado = EstadoEfecto.Cobrado;
                else if (this is EfectoPago)
                    Estado = EstadoEfecto.Pagado;
            }
            else
            {
                Estado = EstadoEfecto.Pendiente;
            }
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadoEfecto.Pendiente;
        FechaVencimiento = DateTime.Today;
    }
}
