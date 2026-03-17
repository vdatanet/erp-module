using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.Services;
using Microsoft.Extensions.DependencyInjection;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[XafDisplayName("Simulación")]
[ImageName("BO_Scheduler")]
[DefaultProperty(nameof(Nombre))]
public class Simulacion(Session session) : EntidadBase(session), IReservaCalculable
{
    private Alquiler? _alquiler;
    private string? _nombre;
    private DateTime _startOn;
    private DateTime _endOn;
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

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        StartOn = DateTime.Now.Date;
        EndOn = StartOn.AddDays(1);
        Alojamiento = false;
        Parking = false;
        Ac = false;
    }

    private void Calcular()
    {
        if (IsLoading || IsSaving) return;
        Session.ServiceProvider.GetRequiredService<IReservaService>().Calcular(this);
    }

    private void CalcularDescuento()
    {
        if (IsLoading || IsSaving) return;
        Session.ServiceProvider.GetRequiredService<IReservaService>().CalcularDescuento(this);
    }

    [Size(255)]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Association("Alquiler-Simulaciones")]
    [XafDisplayName("Alquiler")]
    [ImmediatePostData]
    public Alquiler? Alquiler
    {
        get => _alquiler;
        set
        {
            bool modified = SetPropertyValue(nameof(Alquiler), ref _alquiler, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [ImmediatePostData]
    [XafDisplayName("Desde")]
    public DateTime StartOn
    {
        get => _startOn;
        set
        {
            bool modified = SetPropertyValue(nameof(StartOn), ref _startOn, value);
            if (modified && !IsLoading)
            {
                _dias = (EndOn - StartOn).TotalDays;
                OnChanged(nameof(Dias));
                Calcular();
            }
        }
    }

    [ImmediatePostData]
    [XafDisplayName("Hasta")]
    public DateTime EndOn
    {
        get => _endOn;
        set
        {
            bool modified = SetPropertyValue(nameof(EndOn), ref _endOn, value);
            if (modified && !IsLoading)
            {
                _dias = (EndOn - StartOn).TotalDays;
                OnChanged(nameof(Dias));
                Calcular();
            }
        }
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Días")]
    public double Dias
    {
        get => _dias;
        set => SetPropertyValue(nameof(Dias), ref _dias, value);
    }

    [ImmediatePostData]
    [XafDisplayName("Alojamiento")]
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

    [ImmediatePostData]
    [XafDisplayName("Parking")]
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

    [ImmediatePostData]
    [XafDisplayName("Aire acondicionado")]
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

    [ImmediatePostData]
    [XafDisplayName("Personas sábanas")]
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

    [ImmediatePostData]
    [XafDisplayName("Personas sujetas tasa")]
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

    [XafDisplayName("Personas exentas tasa")]
    public int PersonasExentas
    {
        get => _personasExentas;
        set => SetPropertyValue(nameof(PersonasExentas), ref _personasExentas, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Importe alojamiento")]
    public decimal ImporteAlojamiento
    {
        get => _importeAlojamiento;
        set => SetPropertyValue(nameof(ImporteAlojamiento), ref _importeAlojamiento, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Importe parking")]
    public decimal ImporteParking
    {
        get => _importeParking;
        set => SetPropertyValue(nameof(ImporteParking), ref _importeParking, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Importe AC")]
    public decimal ImporteAc
    {
        get => _importeAc;
        set => SetPropertyValue(nameof(ImporteAc), ref _importeAc, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Importe sábanas")]
    public decimal ImporteSabanas
    {
        get => _importeSabanas;
        set => SetPropertyValue(nameof(ImporteSabanas), ref _importeSabanas, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Importe tasa turística")]
    public decimal ImporteTasaTuristica
    {
        get => _importeTasaTuristica;
        set => SetPropertyValue(nameof(ImporteTasaTuristica), ref _importeTasaTuristica, value);
    }

    [ImmediatePostData]
    [XafDisplayName("Importe otros extras")]
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

    [ImmediatePostData]
    [XafDisplayName("Importe descuento")]
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

    [ImmediatePostData]
    [XafDisplayName("Descuento (%)")]
    [ModelDefault("EditMask", "N2")]
    [ModelDefault("DisplayFormat", "{0:N2}%")]
    public decimal PerDescuento
    {
        get => _perDescuento;
        set
        {
            bool modified = SetPropertyValue(nameof(PerDescuento), ref _perDescuento, value);
            if (modified && !IsLoading)
                CalcularDescuento();
        }
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Subtotal")]
    public decimal Subtotal
    {
        get => _subtotal;
        set => SetPropertyValue(nameof(Subtotal), ref _subtotal, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Total")]
    public decimal Total
    {
        get => _total;
        set => SetPropertyValue(nameof(Total), ref _total, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Total (incl. tasa)")]
    public decimal TotalTasaTuristicaIncluida
    {
        get => _totalTasaTuristicaIncluida;
        set => SetPropertyValue(nameof(TotalTasaTuristicaIncluida), ref _totalTasaTuristicaIncluida, value);
    }
}
