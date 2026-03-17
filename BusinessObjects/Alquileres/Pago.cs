using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Facturacion;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("Business_Money")]
[XafDisplayName("Pago")]
public class Pago(Session session) : EntidadBase(session)
{
    private DateTime _fechaPago;
    private decimal _importe;
    private MedioPago? _medio;
    private Reserva? _reserva;
    private Factura? _factura;
    private string? _notas;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaPago = DateTime.Now.Date;
    }

    [XafDisplayName("Fecha de pago")]
    public DateTime FechaPago
    {
        get => _fechaPago;
        set => SetPropertyValue(nameof(FechaPago), ref _fechaPago, value);
    }

    [XafDisplayName("Importe")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal Importe
    {
        get => _importe;
        set
        {
            bool modified = SetPropertyValue(nameof(Importe), ref _importe, value);
            if (!IsLoading && !IsSaving && Reserva != null && modified)
            {
                Reserva.SumarPagos(true);
            }
        }
    }

    [XafDisplayName("Medio de pago")]
    public MedioPago? Medio
    {
        get => _medio;
        set => SetPropertyValue(nameof(Medio), ref _medio, value);
    }

    [Association("Reserva-Pagos")]
    [XafDisplayName("Reserva")]
    public Reserva? Reserva
    {
        get => _reserva;
        set
        {
            Reserva? oldReserva = _reserva;
            bool modified = SetPropertyValue(nameof(Reserva), ref _reserva, value);
            if (!IsLoading && !IsSaving && modified)
            {
                oldReserva?.SumarPagos(true);
                Reserva?.SumarPagos(true);
            }
        }
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [XafDisplayName("Factura")]
    [Association("Factura-Pago")]
    public Factura? Factura
    {
        get => _factura;
        set => SetPropertyValue(nameof(Factura), ref _factura, value);
    }
}
