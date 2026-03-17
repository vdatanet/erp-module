using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.Factories;
using erp.Module.Helpers.Comun;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[XafDisplayName("Reserva")]
[ImageName("BO_Scheduler")]
public class Reserva(Session session) : EventoBase(session)
{
    private Alquiler _alquiler;
    private decimal _totalPagat;
    private Cliente _cliente;
    private int _numero;
    private int _temporada;
    private string _referencia;
    private DateTime _dataReserva;
    private DateTime _validaFins;
    private double _dies;
    private bool _allotjament;
    private bool _parking;
    private bool _ac;
    private int _personesLlencols;
    private int _personesSubjectes;
    private int _personesExemptes;
    private decimal _importAllotjament;
    private decimal _importParking;
    private decimal _importAc;
    private decimal _importLlencols;
    private decimal _importTaxaTuristica;
    private decimal _importAltresExtres;
    private decimal _importDescompte;
    private decimal _perDescompte;
    private decimal _subtotal;
    private decimal _total;
    private decimal _totalTaxaTuristicaInclosa;
    private decimal _importPendent;

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        DataReserva = DateTime.Now.Date;
        ValidaFins = DataReserva.AddDays(7);
        Allotjament = true;
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
                Numero = SequenceFactory.GetNextSequence(Session, $"{GetType().FullName}-{Temporada}", out var formattedSequence, $"{Temporada}/", 4);
                Referencia = formattedSequence;
            }
        }
        base.OnSaving();
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (!IsLoading && (propertyName == nameof(StartOn) || propertyName == nameof(EndOn)))
        {
            Dies = (EndOn - StartOn).TotalDays;
            Temporada = StartOn.Year;
            Calcular();
        }
    }

    [Association("Alquiler-Reservas")]
    [XafDisplayName("Lloguer")]
    [ImmediatePostData]
    public Alquiler Alquiler
    {
        get => _alquiler;
        set
        {
            bool modified = SetPropertyValue(nameof(Alquiler), ref _alquiler, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [Association("Cliente-Reserves")]
    [XafDisplayName("Client")]
    [ImmediatePostData]
    public Cliente Client
    {
        get => _cliente;
        set
        {
            bool modified = SetPropertyValue(nameof(Client), ref _cliente, value);
            if (modified && !IsLoading && Client != null)
            {
                Subject = Client.Nombre;
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

    [XafDisplayName("Referència")]
    [ModelDefault("AllowEdit", "False")]
    [Size(255)]
    public string Referencia
    {
        get => _referencia;
        set => SetPropertyValue(nameof(Referencia), ref _referencia, value);
    }

    [XafDisplayName("Data Reserva")]
    public DateTime DataReserva
    {
        get => _dataReserva;
        set => SetPropertyValue(nameof(DataReserva), ref _dataReserva, value);
    }

    [XafDisplayName("Vàlida fins")]
    public DateTime ValidaFins
    {
        get => _validaFins;
        set => SetPropertyValue(nameof(ValidaFins), ref _validaFins, value);
    }

    [XafDisplayName("Dies")]
    [ModelDefault("AllowEdit", "False")]
    public double Dies
    {
        get => _dies;
        set => SetPropertyValue(nameof(Dies), ref _dies, value);
    }

    [XafDisplayName("Allotjament")]
    [ImmediatePostData]
    public bool Allotjament
    {
        get => _allotjament;
        set
        {
            bool modified = SetPropertyValue(nameof(Allotjament), ref _allotjament, value);
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

    [XafDisplayName("Aire condicionat")]
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

    [XafDisplayName("Persones llençols")]
    [ImmediatePostData]
    public int PersonesLlencols
    {
        get => _personesLlencols;
        set
        {
            bool modified = SetPropertyValue(nameof(PersonesLlencols), ref _personesLlencols, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [XafDisplayName("Persones subjectes")]
    [ImmediatePostData]
    public int PersonesSubjectes
    {
        get => _personesSubjectes;
        set
        {
            bool modified = SetPropertyValue(nameof(PersonesSubjectes), ref _personesSubjectes, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [XafDisplayName("Persones exemptes")]
    public int PersonesExemptes
    {
        get => _personesExemptes;
        set => SetPropertyValue(nameof(PersonesExemptes), ref _personesExemptes, value);
    }

    [XafDisplayName("Import allotjament")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImportAllotjament
    {
        get => _importAllotjament;
        set => SetPropertyValue(nameof(ImportAllotjament), ref _importAllotjament, value);
    }

    [XafDisplayName("Import parking")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImportParking
    {
        get => _importParking;
        set => SetPropertyValue(nameof(ImportParking), ref _importParking, value);
    }

    [XafDisplayName("Import aire condicionat")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImportAc
    {
        get => _importAc;
        set => SetPropertyValue(nameof(ImportAc), ref _importAc, value);
    }

    [XafDisplayName("Import llençols")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImportLlencols
    {
        get => _importLlencols;
        set => SetPropertyValue(nameof(ImportLlencols), ref _importLlencols, value);
    }

    [XafDisplayName("Import taxa turística")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImportTaxaTuristica
    {
        get => _importTaxaTuristica;
        set => SetPropertyValue(nameof(ImportTaxaTuristica), ref _importTaxaTuristica, value);
    }

    [XafDisplayName("Import altres extres")]
    [ImmediatePostData]
    public decimal ImportAltresExtres
    {
        get => _importAltresExtres;
        set
        {
            bool modified = SetPropertyValue(nameof(ImportAltresExtres), ref _importAltresExtres, value);
            if (modified && !IsLoading)
                Calcular();
        }
    }

    [XafDisplayName("Import descompte")]
    [ImmediatePostData]
    public decimal ImportDescompte
    {
        get => _importDescompte;
        set
        {
            bool modified = SetPropertyValue(nameof(ImportDescompte), ref _importDescompte, value);
            if (modified && !IsLoading)
            {
                if (Subtotal > 0)
                    _perDescompte = MoneyMath.RoundMoney(ImportDescompte / Subtotal * 100);
                else
                    _perDescompte = 0;
                OnChanged(nameof(PerDescompte));
                Calcular();
            }
        }
    }

    [XafDisplayName("Descompte (%)")]
    [ImmediatePostData]
    [ModelDefault("EditMask", "N2")]
    [ModelDefault("DisplayFormat", "{0:N2}%")]
    public decimal PerDescompte
    {
        get => _perDescompte;
        set
        {
            bool modified = SetPropertyValue(nameof(PerDescompte), ref _perDescompte, value);
            if (modified && !IsLoading)
            {
                ImportDescompte = MoneyMath.RoundMoney(Subtotal * PerDescompte / 100);
                Calcular();
            }
        }
    }

    [XafDisplayName("Import subtotal")]
    [ModelDefault("AllowEdit", "False")]
    public decimal Subtotal
    {
        get => _subtotal;
        set => SetPropertyValue(nameof(Subtotal), ref _subtotal, value);
    }

    [XafDisplayName("Import total")]
    [ModelDefault("AllowEdit", "False")]
    public decimal Total
    {
        get => _total;
        set => SetPropertyValue(nameof(Total), ref _total, value);
    }

    [XafDisplayName("Import amb taxes")]
    [ModelDefault("AllowEdit", "False")]
    public decimal TotalTaxaTuristicaInclosa
    {
        get => _totalTaxaTuristicaInclosa;
        set => SetPropertyValue(nameof(TotalTaxaTuristicaInclosa), ref _totalTaxaTuristicaInclosa, value);
    }

    [XafDisplayName("Import pendent")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImportPendent
    {
        get => _importPendent;
        set => SetPropertyValue(nameof(ImportPendent), ref _importPendent, value);
    }

    [XafDisplayName("Total Pagat")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal TotalPagat
    {
        get => _totalPagat;
        set => SetPropertyValue(nameof(TotalPagat), ref _totalPagat, value);
    }

    [Association("Reserva-Pagaments")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Pagaments")]
    public XPCollection<Pagament> Pagaments => GetCollection<Pagament>();

    [Association("Reserva-Viatgers")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Viatgers")]
    public XPCollection<Viatger> Viatgers => GetCollection<Viatger>();

    public void SumarPagaments(bool update)
    {
        if (IsLoading || IsSaving) return;
        decimal totalPagat = 0;
        foreach (var pagament in Pagaments)
        {
            totalPagat += pagament.Import;
        }

        TotalPagat = totalPagat;
        ImportPendent = Total - TotalPagat;
        
        if (TotalPagat > 0) 
            Status = 1; // Ocupat/Confirmat
        else 
            Status = 0; // Pendent
            
        if (update)
        {
            OnChanged(nameof(TotalPagat));
            OnChanged(nameof(ImportPendent));
            OnChanged(nameof(Status));
        }
    }

    private void Calcular()
    {
        if (EndOn > StartOn)
        {
            // Allotjament
            if (Allotjament && Alquiler != null && Alquiler.Tarifa != null)
            {
                object suma = Session.Evaluate(
                    typeof(PreuDiari),
                    CriteriaOperator.Parse("Sum(Preu)"),
                    CriteriaOperator.Parse(
                        "Tarifa.Oid = ? AND Data >= ? AND Data < ?",
                        Alquiler.Tarifa.Oid,
                        StartOn.Date,
                        EndOn.Date));
                ImportAllotjament = Convert.ToDecimal(suma);
            }
            else
            {
                ImportAllotjament = 0;
            }

            // Parking
            if (Parking)
            {
                object suma = Session.Evaluate(
                    typeof(PreuDiari),
                    CriteriaOperator.Parse("Sum(Preu)"),
                    CriteriaOperator.Parse("Tarifa.Nombre = 'P' AND Data >= ? AND Data < ?", StartOn.Date, EndOn.Date));
                ImportParking = Convert.ToDecimal(suma);
            }
            else
            {
                ImportParking = 0;
            }

            // Aire condicionat
            if (Ac)
            {
                var extraAc = Session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Aire condicionat'"));
                if (extraAc != null)
                    ImportAc = MoneyMath.RoundMoney((decimal)Dies * extraAc.PreuDiari);
            }
            else
            {
                ImportAc = 0;
            }

            // Llençols
            if (PersonesLlencols > 0)
            {
                var extraLlencols = Session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Llençols'"));
                if (extraLlencols != null)
                    ImportLlencols = MoneyMath.RoundMoney(PersonesLlencols * extraLlencols.PreuDiari);
            }
            else
            {
                ImportLlencols = 0;
            }

            // Taxa turística (màxim 7 dies)
            if (PersonesSubjectes > 0)
            {
                var diesSubjectes = (decimal)Dies;
                if (diesSubjectes > 7) diesSubjectes = 7;
                var extraTaxa = Session.FindObject<Extra>(CriteriaOperator.Parse("Nombre = 'Taxa turística'"));
                if (extraTaxa != null)
                    ImportTaxaTuristica = MoneyMath.RoundMoney(diesSubjectes * PersonesSubjectes * extraTaxa.PreuDiari);
            }
            else
            {
                ImportTaxaTuristica = 0;
            }

            Subtotal = ImportAllotjament + ImportParking;
            Total = Subtotal + ImportAc + ImportLlencols + ImportAltresExtres - ImportDescompte;
            TotalTaxaTuristicaInclosa = Total + ImportTaxaTuristica;
            ImportPendent = Total - TotalPagat;
        }
    }
}
