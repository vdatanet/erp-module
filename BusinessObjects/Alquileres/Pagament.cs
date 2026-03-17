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
[XafDisplayName("Pagament")]
public class Pagament(Session session) : EntidadBase(session)
{
    private DateTime _dataPagament;
    private decimal _import;
    private Mitjans _mitja;
    private Reserva _reserva;
    private Factura _factura;
    private string _observacions;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        DataPagament = DateTime.Now.Date;
        Mitja = Mitjans.Transferencia;
    }

    [XafDisplayName("Data de pagament")]
    public DateTime DataPagament
    {
        get => _dataPagament;
        set => SetPropertyValue(nameof(DataPagament), ref _dataPagament, value);
    }

    [XafDisplayName("Import")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal Import
    {
        get => _import;
        set
        {
            bool modified = SetPropertyValue(nameof(Import), ref _import, value);
            if (!IsLoading && !IsSaving && Reserva != null && modified)
            {
                Reserva.SumarPagaments(true);
            }
        }
    }

    [XafDisplayName("Mitjà de pagament")]
    public Mitjans Mitja
    {
        get => _mitja;
        set => SetPropertyValue(nameof(Mitja), ref _mitja, value);
    }

    [Association("Reserva-Pagaments")]
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
                oldReserva?.SumarPagaments(true);
                Reserva?.SumarPagaments(true);
            }
        }
    }

    [Size(255)]
    [XafDisplayName("Observacions")]
    public string Observacions
    {
        get => _observacions;
        set => SetPropertyValue(nameof(Observacions), ref _observacions, value);
    }

    [XafDisplayName("Factura")]
    public Factura Factura
    {
        get => _factura;
        set => SetPropertyValue(nameof(Factura), ref _factura, value);
    }

    public enum Mitjans
    {
        [XafDisplayName("Transferència bancària")]
        Transferencia,
        [XafDisplayName("Efectiu")]
        Efectiu,
        [XafDisplayName("Targeta de crèdit")]
        Targeta
    }
}
