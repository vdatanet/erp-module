using System.ComponentModel;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.Factories;
using erp.Module.Helpers.Comun;
using erp.Module.Services;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[XafDisplayName("Reserva")]
[ImageName("BO_Scheduler")]
[DefaultProperty(nameof(Secuencia))]
public class Reserva(Session session) : EventoBase(session), IReservaCalculable
{
    private RecursoAlquilable? _recursoAlquilable;
    private decimal _totalPagado;
    private string? _notas;
    private Cliente? _cliente;
    private int _numero;
    private int _temporada;
    private string? _secuencia;
    private DateTime _fechaReserva;
    private DateTime _validaHasta;
    private double _dias;
    private bool _alojamiento;
    private bool _parking;
    private bool _ac;
    private int _personasSabanas;
    private int _personasSujetas;
    private int _personasExentas;
    private decimal _importeAlojamiento;
    private decimal _importeParking;
    private decimal _importeAc;
    private decimal _importeSabanas;
    private decimal _importeTasaTuristica;
    private decimal _importeOtrosExtras;
    private decimal _importeDescuento;
    private decimal _perDescuento;
    private decimal _subtotal;
    private decimal _total;
    private decimal _totalTasaTuristicaIncluida;
    private decimal _importePendiente;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaReserva = DateTime.Now.Date;
        ValidaHasta = FechaReserva.AddDays(7);
        Alojamiento = true;
        Parking = false;
        Ac = false;
    }

    protected override void OnSaving()
    {
        if (!IsLoading && !IsSaving && Numero == 0 && StartOn != DateTime.MinValue)
        {
            Temporada = StartOn.Year;
            if (Temporada != 0)
            {
                Numero = SequenceFactory.GetNextSequence(Session, $"{GetType().FullName}-{Temporada}", out var formattedSequence, $"{Temporada}", 4);
                Secuencia = formattedSequence;
            }
        }
        base.OnSaving();
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (!IsLoading && (propertyName == nameof(StartOn) || propertyName == nameof(EndOn)))
        {
            Dias = (EndOn - StartOn).TotalDays;
            Temporada = StartOn.Year;
            Calcular();
        }
    }

    [Association("RecursoAlquilable-Reservas")]
    [XafDisplayName("Recurso Alquilable")]
    [ImmediatePostData]
    public RecursoAlquilable? RecursoAlquilable
    {
        get => _recursoAlquilable;
        set
        {
            bool modified = SetPropertyValue(nameof(RecursoAlquilable), ref _recursoAlquilable, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [Association("Cliente-Reservas")]
    [XafDisplayName("Cliente")]
    [ImmediatePostData]
    public Cliente? Cliente
    {
        get => _cliente;
        set
        {
            bool modified = SetPropertyValue(nameof(Cliente), ref _cliente, value);
            if (modified && !IsLoading && Cliente != null)
            {
                Subject = Cliente.Nombre;
            }
        }
    }

    [XafDisplayName("Número")]
    [ModelDefault("AllowEdit", "False")]
    public int Numero
    {
        get => _numero;
        set => SetPropertyValue(nameof(Numero), ref _numero, value);
    }

    [XafDisplayName("Temporada")]
    [ModelDefault("AllowEdit", "False")]
    public int Temporada
    {
        get => _temporada;
        set => SetPropertyValue(nameof(Temporada), ref _temporada, value);
    }

    [XafDisplayName("Secuencia")]
    [ModelDefault("AllowEdit", "False")]
    [Size(255)]
    public string? Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    [XafDisplayName("Fecha Reserva")]
    public DateTime FechaReserva
    {
        get => _fechaReserva;
        set => SetPropertyValue(nameof(FechaReserva), ref _fechaReserva, value);
    }

    [XafDisplayName("Válida hasta")]
    public DateTime ValidaHasta
    {
        get => _validaHasta;
        set => SetPropertyValue(nameof(ValidaHasta), ref _validaHasta, value);
    }

    [XafDisplayName("Días")]
    [ModelDefault("AllowEdit", "False")]
    public double Dias
    {
        get => _dias;
        set => SetPropertyValue(nameof(Dias), ref _dias, value);
    }

    [XafDisplayName("Alojamiento")]
    [ImmediatePostData]
    public bool Alojamiento
    {
        get => _alojamiento;
        set
        {
            bool modified = SetPropertyValue(nameof(Alojamiento), ref _alojamiento, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [XafDisplayName("Parking")]
    [ImmediatePostData]
    public bool Parking
    {
        get => _parking;
        set
        {
            bool modified = SetPropertyValue(nameof(Parking), ref _parking, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [XafDisplayName("Aire acondicionado")]
    [ImmediatePostData]
    public bool Ac
    {
        get => _ac;
        set
        {
            bool modified = SetPropertyValue(nameof(Ac), ref _ac, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [XafDisplayName("Personas sábanas")]
    [ImmediatePostData]
    public int PersonasSabanas
    {
        get => _personasSabanas;
        set
        {
            bool modified = SetPropertyValue(nameof(PersonasSabanas), ref _personasSabanas, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [XafDisplayName("Personas sujetas")]
    [ImmediatePostData]
    public int PersonasSujetas
    {
        get => _personasSujetas;
        set
        {
            bool modified = SetPropertyValue(nameof(PersonasSujetas), ref _personasSujetas, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [XafDisplayName("Personas exentas")]
    public int PersonasExentas
    {
        get => _personasExentas;
        set => SetPropertyValue(nameof(PersonasExentas), ref _personasExentas, value);
    }

    [XafDisplayName("Importe alojamiento")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImporteAlojamiento
    {
        get => _importeAlojamiento;
        set => SetPropertyValue(nameof(ImporteAlojamiento), ref _importeAlojamiento, value);
    }

    [XafDisplayName("Importe parking")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImporteParking
    {
        get => _importeParking;
        set => SetPropertyValue(nameof(ImporteParking), ref _importeParking, value);
    }

    [XafDisplayName("Importe aire acondicionado")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImporteAc
    {
        get => _importeAc;
        set => SetPropertyValue(nameof(ImporteAc), ref _importeAc, value);
    }

    [XafDisplayName("Importe sábanas")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImporteSabanas
    {
        get => _importeSabanas;
        set => SetPropertyValue(nameof(ImporteSabanas), ref _importeSabanas, value);
    }

    [XafDisplayName("Importe tasa turística")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImporteTasaTuristica
    {
        get => _importeTasaTuristica;
        set => SetPropertyValue(nameof(ImporteTasaTuristica), ref _importeTasaTuristica, value);
    }

    [XafDisplayName("Importe otros extras")]
    [ImmediatePostData]
    public decimal ImporteOtrosExtras
    {
        get => _importeOtrosExtras;
        set
        {
            bool modified = SetPropertyValue(nameof(ImporteOtrosExtras), ref _importeOtrosExtras, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [XafDisplayName("Importe descuento")]
    [ImmediatePostData]
    public decimal ImporteDescuento
    {
        get => _importeDescuento;
        set
        {
            bool modified = SetPropertyValue(nameof(ImporteDescuento), ref _importeDescuento, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [XafDisplayName("Descuento (%)")]
    [ImmediatePostData]
    [ModelDefault("EditMask", "N2")]
    [ModelDefault("DisplayFormat", "{0:N2}%")]
    public decimal PerDescuento
    {
        get => _perDescuento;
        set
        {
            bool modified = SetPropertyValue(nameof(PerDescuento), ref _perDescuento, value);
            if (modified && !IsLoading)
            {
                Session.ServiceProvider.GetRequiredService<IReservaService>().CalcularDescuento(this);
                ImportePendiente = Total - TotalPagado;
            }
        }
    }

    [XafDisplayName("Importe subtotal")]
    [ModelDefault("AllowEdit", "False")]
    public decimal Subtotal
    {
        get => _subtotal;
        set => SetPropertyValue(nameof(Subtotal), ref _subtotal, value);
    }

    [XafDisplayName("Importe total")]
    [ModelDefault("AllowEdit", "False")]
    public decimal Total
    {
        get => _total;
        set => SetPropertyValue(nameof(Total), ref _total, value);
    }

    [XafDisplayName("Total tasa turística incluida")]
    [ModelDefault("AllowEdit", "False")]
    public decimal TotalTasaTuristicaIncluida
    {
        get => _totalTasaTuristicaIncluida;
        set => SetPropertyValue(nameof(TotalTasaTuristicaIncluida), ref _totalTasaTuristicaIncluida, value);
    }

    [XafDisplayName("Importe pendiente")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImportePendiente
    {
        get => _importePendiente;
        set => SetPropertyValue(nameof(ImportePendiente), ref _importePendiente, value);
    }

    [XafDisplayName("Total pagado")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal TotalPagado
    {
        get => _totalPagado;
        set => SetPropertyValue(nameof(TotalPagado), ref _totalPagado, value);
    }

    [Association("Reserva-Pagos")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Pagos")]
    public XPCollection<Pago> Pagos => GetCollection<Pago>();

    [Association("Reserva-Viajeros")]
    [XafDisplayName("Viajeros")]
    public XPCollection<Viajero> Viajeros => GetCollection<Viajero>();

    [XafDisplayName("Notas")]
    [Size(SizeAttribute.Unlimited)]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    public void SumarPagos(bool update)
    {
        if (IsLoading || IsSaving) return;
        decimal totalPagado = 0;
        foreach (var pago in Pagos)
        {
            totalPagado += pago.Importe;
        }

        TotalPagado = totalPagado;
        ImportePendiente = Total - TotalPagado;
        
        if (TotalPagado > 0) 
            Status = 1; // Ocupado/Confirmado
        else 
            Status = 0; // Pendiente
            
        if (update)
        {
            OnChanged(nameof(TotalPagado));
            OnChanged(nameof(ImportePendiente));
            OnChanged(nameof(Status));
        }
    }

    private void Calcular()
    {
        if (IsLoading || IsSaving) return;
        Session.ServiceProvider.GetRequiredService<IReservaService>().Calcular(this);
        ImportePendiente = Total - TotalPagado;
    }
}
