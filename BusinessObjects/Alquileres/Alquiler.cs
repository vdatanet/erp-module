using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Productos;

namespace erp.Module.BusinessObjects.Alquileres;

[DefaultClassOptions]
[NavigationItem("Alquileres")]
[ImageName("BO_Product")]
[DefaultProperty(nameof(Nombre))]
public class Alquiler(Session session) : RecursoBase(session)
{
    private string _descripcion;
    private decimal _precioPorDia;
    private TipoAlquiler _tipoAlquiler;
    private Producto _productoRelacionado;
    private bool _estaActivo;
    private Capacitats _capacitat;
    private string _hut;
    private string _observacions;
    private Plantes _planta;
    private int _sequencia;
    private Tarifa _tarifa;
    private TipusAlquilerDetall _tipus;

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Nom")]
    public string Nombre
    {
        get => Caption;
        set => Caption = value;
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripció")]
    public string Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Preu per Dia")]
    public decimal PrecioPorDia
    {
        get => _precioPorDia;
        set => SetPropertyValue(nameof(PrecioPorDia), ref _precioPorDia, value);
    }

    [Association("TipoAlquiler-Alquileres")]
    [RuleRequiredField]
    [XafDisplayName("Tipus de Lloguer")]
    public TipoAlquiler TipoAlquiler
    {
        get => _tipoAlquiler;
        set => SetPropertyValue(nameof(TipoAlquiler), ref _tipoAlquiler, value);
    }

    [XafDisplayName("Producte Relacionat")]
    [ToolTip("Producte utilitzat per a la facturació d'aquest lloguer")]
    public Producto ProductoRelacionado
    {
        get => _productoRelacionado;
        set => SetPropertyValue(nameof(ProductoRelacionado), ref _productoRelacionado, value);
    }

    [XafDisplayName("Actiu")]
    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    [XafDisplayName("Capacitat")]
    public Capacitats Capacitat
    {
        get => _capacitat;
        set => SetPropertyValue(nameof(Capacitat), ref _capacitat, value);
    }

    [Size(255)]
    [XafDisplayName("HUT")]
    public string Hut
    {
        get => _hut;
        set => SetPropertyValue(nameof(Hut), ref _hut, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observacions")]
    public string Observacions
    {
        get => _observacions;
        set => SetPropertyValue(nameof(Observacions), ref _observacions, value);
    }

    [XafDisplayName("Planta")]
    public Plantes Planta
    {
        get => _planta;
        set => SetPropertyValue(nameof(Planta), ref _planta, value);
    }

    [XafDisplayName("Seqüència")]
    public int Sequencia
    {
        get => _sequencia;
        set => SetPropertyValue(nameof(Sequencia), ref _sequencia, value);
    }

    [Association("Tarifa-Alquileres")]
    [XafDisplayName("Tarifa")]
    public Tarifa Tarifa
    {
        get => _tarifa;
        set => SetPropertyValue(nameof(Tarifa), ref _tarifa, value);
    }

    [XafDisplayName("Tipus Detall")]
    public TipusAlquilerDetall Tipus
    {
        get => _tipus;
        set => SetPropertyValue(nameof(Tipus), ref _tipus, value);
    }

    [Association("Alquiler-Reservas")]
    [XafDisplayName("Reserves")]
    public XPCollection<Reserva> Reservas => GetCollection<Reserva>();

    [Association("Alquiler-Simulacions")]
    [XafDisplayName("Simulacions")]
    public XPCollection<Simulacio> Simulacions => GetCollection<Simulacio>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        EstaActivo = true;
        Tipus = TipusAlquilerDetall.A1;
        Planta = Plantes.Baixa;
        Capacitat = Capacitats.Dos;
    }

    public enum Plantes
    {
        [XafDisplayName("Baixa")]
        Baixa,
        [XafDisplayName("Primera")]
        Primera,
        [XafDisplayName("Segona")]
        Segona,
        [XafDisplayName("Tercera")]
        Tercera
    }

    public enum Capacitats
    {
        [XafDisplayName("2/4")]
        Dos,
        [XafDisplayName("4/6")]
        Quatre
    }

    public enum TipusAlquilerDetall
    {
        [XafDisplayName("A0")]
        A0,
        [XafDisplayName("A1")]
        A1,
        [XafDisplayName("B1")]
        B1,
        [XafDisplayName("A2")]
        A2,
        [XafDisplayName("B2")]
        B2,
        [XafDisplayName("A3")]
        A3,
        [XafDisplayName("B3")]
        B3
    }
}
