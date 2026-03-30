using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.Factories;
using erp.Module.Helpers.Contactos;
using erp.Module.Services.Reserva;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[XafDisplayName("Reserva")]
[ImageName("BO_Scheduler")]
[DefaultProperty(nameof(Secuencia))]
public class Reserva(Session session) : EventoBase(session), IReservaCalculable
{
    private bool _ac;
    private bool _alojamiento;
    private Cliente? _cliente;
    private double _dias;
    private DateTime _fechaReserva;
    private decimal _importeAc;
    private decimal _importeAlojamiento;
    private decimal _importeDescuento;
    private decimal _importeOtrosExtras;
    private decimal _importeParking;
    private decimal _importePendiente;
    private decimal _importeSabanas;
    private decimal _importeTasaTuristica;
    private string? _notas;
    private int _numero;
    private bool _parking;
    private decimal _perDescuento;
    private int _personasExentas;
    private int _personasSabanas;
    private int _personasSujetas;
    private RecursoAlquilable? _recursoAlquilable;
    private string? _secuencia;
    private decimal _subtotal;
    private Ejercicio? _ejercicio;
    private decimal _total;
    private decimal _totalPagado;
    private decimal _totalTasaTuristicaIncluida;
    private DateTime _validaHasta;

    [Association("Cliente-Reservas")]
    [XafDisplayName("Cliente")]
    [ImmediatePostData]
    [DataSourceCriteria("Activo = true")]
    public Cliente? Cliente
    {
        get => _cliente;
        set
        {
            var modified = SetPropertyValue(nameof(Cliente), ref _cliente, value);
            if (modified && !IsLoading && Cliente != null) Subject = Cliente.Nombre;
        }
    }

    [XafDisplayName("Número")]
    [ModelDefault("AllowEdit", "False")]
    public int Numero
    {
        get => _numero;
        set => SetPropertyValue(nameof(Numero), ref _numero, value);
    }

    [XafDisplayName("Ejercicio")]
    [RuleRequiredField("RuleRequiredField_Reserva_Ejercicio", DefaultContexts.Save, CustomMessageTemplate = "El Ejercicio de la reserva es obligatorio")]
    public Ejercicio? Ejercicio
    {
        get => _ejercicio;
        set => SetPropertyValue(nameof(Ejercicio), ref _ejercicio, value);
    }

    [Browsable(false)]
    [RuleFromBoolProperty("Reserva_FechaNoBloqueada", DefaultContexts.Save, "La fecha de inicio de la reserva se encuentra en un periodo bloqueado del ejercicio o el ejercicio está bloqueado.", UsedProperties = nameof(StartOn))]
    public bool IsFechaNoBloqueada
    {
        get
        {
            if (Ejercicio == null) return true;
            if (Ejercicio.Estado == EstadoEjercicio.Bloqueado) return false;

            return !Ejercicio.PeriodosBloqueados.Any(p => StartOn >= p.FechaInicio && StartOn <= p.FechaFin);
        }
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

    [XafDisplayName("Personas exentas")]
    public int PersonasExentas
    {
        get => _personasExentas;
        set => SetPropertyValue(nameof(PersonasExentas), ref _personasExentas, value);
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

    [Association("Reserva-EfectosCobro")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Pagos")]
    public XPCollection<EfectoCobro> Pagos => GetCollection<EfectoCobro>();

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

    [Association("RecursoAlquilable-Reservas")]
    [XafDisplayName("Recurso Alquilable")]
    [ImmediatePostData]
    public RecursoAlquilable? RecursoAlquilable
    {
        get => _recursoAlquilable;
        set
        {
            var modified = SetPropertyValue(nameof(RecursoAlquilable), ref _recursoAlquilable, value);
            if (modified && !IsLoading)
                Calcular();
        }
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
            var modified = SetPropertyValue(nameof(Alojamiento), ref _alojamiento, value);
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
            var modified = SetPropertyValue(nameof(Parking), ref _parking, value);
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
            var modified = SetPropertyValue(nameof(Ac), ref _ac, value);
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
            var modified = SetPropertyValue(nameof(PersonasSabanas), ref _personasSabanas, value);
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
            var modified = SetPropertyValue(nameof(PersonasSujetas), ref _personasSujetas, value);
            if (modified && !IsLoading)
                Calcular();
        }
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
            var modified = SetPropertyValue(nameof(ImporteOtrosExtras), ref _importeOtrosExtras, value);
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
            var modified = SetPropertyValue(nameof(ImporteDescuento), ref _importeDescuento, value);
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
            var modified = SetPropertyValue(nameof(PerDescuento), ref _perDescuento, value);
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

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var localTime = InformacionEmpresaHelper.GetLocalTime(Session).Date;
        StartOn = localTime;
        EndOn = localTime.AddDays(1);
        FechaReserva = localTime;
        ValidaHasta = FechaReserva.AddDays(7);
        Alojamiento = true;
        Parking = false;
        Ac = false;
        Ejercicio = Session.Query<Ejercicio>().FirstOrDefault(e => e.Anio == StartOn.Year);
    }

    protected override void OnSaving()
    {
        if (!IsLoading && Session.IsNewObject(this) && Numero == 0 && StartOn != DateTime.MinValue)
        {
            if (Ejercicio != null && Ejercicio.Anio.HasValue)
            {
                var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
                if (companyInfo == null)
                    throw new UserFriendlyException("No se ha podido obtener la configuración de la empresa.");

                int padding = companyInfo.PaddingNumero;
                string prefijo = companyInfo.PrefijoReservas ??
                                 throw new UserFriendlyException("El prefijo de reservas no está configurado.");
                string prefixSequence = $"{prefijo}/{Ejercicio.Anio}";

                Numero = SequenceFactory.GetNextSequence(Session, $"{GetType().FullName}.{Ejercicio.Anio}",
                    out var formattedSequence, prefixSequence, padding);
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
            if (propertyName == nameof(StartOn))
            {
                Ejercicio = Session.Query<Ejercicio>().FirstOrDefault(e => e.Anio == StartOn.Year);
            }
            Calcular();
        }
    }

    public void SumarPagos(bool update)
    {
        if (IsLoading || IsSaving) return;
        decimal totalPagado = 0;
        foreach (var pago in Pagos)
        {
            if (pago.Estado == EstadoEfecto.Cobrado)
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