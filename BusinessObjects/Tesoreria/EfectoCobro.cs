using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Alquileres;
using erp.Module.BusinessObjects.Ventas;

namespace erp.Module.BusinessObjects.Tesoreria;

[DefaultClassOptions]
[NavigationItem("Tesorería")]
[XafDisplayName("Efecto de Cobro")]
[ImageName("BO_Invoice")]
public class EfectoCobro(Session session) : EfectoBase(session)
{
    private FacturaVenta? _factura;
    private Reserva? _reserva;

    [Association("FacturaVenta-EfectosCobro")]
    [XafDisplayName("Factura")]
    public FacturaVenta? Factura
    {
        get => _factura;
        set => SetPropertyValue(nameof(Factura), ref _factura, value);
    }

    [Association("Reserva-EfectosCobro")]
    [XafDisplayName("Reserva")]
    public Reserva? Reserva
    {
        get => _reserva;
        set
        {
            var oldReserva = _reserva;
            var modified = SetPropertyValue(nameof(Reserva), ref _reserva, value);
            if (IsLoading || IsSaving || !modified) return;
            oldReserva?.SumarPagos(true);
            Reserva?.SumarPagos(true);
        }
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (IsLoading || IsSaving) return;

        if (propertyName == nameof(Importe) && Reserva != null)
        {
            Reserva.SumarPagos(true);
        }

        if (propertyName is nameof(Estado) or nameof(Importe))
        {
            Factura?.ActualizarEstadoCobro();
        }
    }
}
