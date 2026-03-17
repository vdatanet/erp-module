using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
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
    private Medios _medio;
    private Reserva _reserva;
    private Factura _factura;
    private string _observaciones;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaPago = DateTime.Now.Date;
        Medio = Medios.Transferencia;
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
    public Medios Medio
    {
        get => _medio;
        set => SetPropertyValue(nameof(Medio), ref _medio, value);
    }

    [Association("Reserva-Pagos")]
    [XafDisplayName("Reserva")]
    public Reserva Reserva
    {
        get => _reserva;
        set
        {
            Reserva oldReserva = _reserva;
            bool modified = SetPropertyValue(nameof(Reserva), ref _reserva, value);
            if (!IsLoading && !IsSaving && modified)
            {
                oldReserva?.SumarPagos(true);
                Reserva?.SumarPagos(true);
            }
        }
    }

    [Size(255)]
    [XafDisplayName("Observaciones")]
    public string Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [XafDisplayName("Factura")]
    public Factura Factura
    {
        get => _factura;
        set => SetPropertyValue(nameof(Factura), ref _factura, value);
    }

    public enum Medios
    {
        [XafDisplayName("Transferencia bancaria")]
        Transferencia,
        [XafDisplayName("Efectivo")]
        Efectiu,
        [XafDisplayName("Tarjeta de crédito")]
        Targeta
    }
}
