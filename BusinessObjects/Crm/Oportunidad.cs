using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Planificacion;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.Helpers.Comun;

namespace erp.Module.BusinessObjects.Crm;

public enum EstadoOportunidad
{
    [XafDisplayName("Prospecto")] Prospecto,
    [XafDisplayName("Calificada")] Calificada,
    [XafDisplayName("Propuesta")] Propuesta,
    [XafDisplayName("Negociación")] Negociacion,
    [XafDisplayName("Ganada")] Ganada,
    [XafDisplayName("Perdida")] Perdida
}

[DefaultClassOptions]
[NavigationItem("Crm")]
[XafDefaultProperty(nameof(Titulo))]
[ImageName("BO_Lead")]
public class Oportunidad(Session session) : EntidadBase(session)
{
    private Campana _campana;
    private Cliente _cliente;
    private string _descripcion;
    private EstadoOportunidad _estado;
    private DateTime _fechaCierreEstimada;
    private Fuente _fuente;
    private Medio _medio;
    private string _notas;
    private double _probabilidad;
    private ApplicationUser _responsable;
    private decimal _sumaPedidos;
    private decimal _sumaPresupuestos;
    private string _titulo;
    private decimal _valorEstimado;

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Título")]
    public string Titulo
    {
        get => _titulo;
        set => SetPropertyValue(nameof(Titulo), ref _titulo, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripción")]
    public string Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [RuleRequiredField]
    [Association("Cliente-Oportunidades")]
    [XafDisplayName("Cliente")]
    public Cliente Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [Association("Campana-Oportunidades")]
    [XafDisplayName("Campaña")]
    public Campana Campana
    {
        get => _campana;
        set => SetPropertyValue(nameof(Campana), ref _campana, value);
    }

    [Association("Medio-Oportunidades")]
    [XafDisplayName("Medio")]
    public Medio Medio
    {
        get => _medio;
        set => SetPropertyValue(nameof(Medio), ref _medio, value);
    }

    [Association("Fuente-Oportunidades")]
    [XafDisplayName("Fuente")]
    public Fuente Fuente
    {
        get => _fuente;
        set => SetPropertyValue(nameof(Fuente), ref _fuente, value);
    }

    [XafDisplayName("Estado")]
    public EstadoOportunidad Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [XafDisplayName("Probabilidad (%)")]
    public double Probabilidad
    {
        get => _probabilidad;
        set => SetPropertyValue(nameof(Probabilidad), ref _probabilidad, value);
    }

    [XafDisplayName("Valor Estimado")]
    [DbType("decimal(18,2)")]
    public decimal ValorEstimado
    {
        get => _valorEstimado;
        set => SetPropertyValue(nameof(ValorEstimado), ref _valorEstimado, value);
    }

    [XafDisplayName("Suma Presupuestos")]
    [DbType("decimal(18,2)")]
    [ModelDefault("AllowEdit", "False")]
    public decimal SumaPresupuestos
    {
        get => _sumaPresupuestos;
        set => SetPropertyValue(nameof(SumaPresupuestos), ref _sumaPresupuestos, value);
    }

    [XafDisplayName("Suma Pedidos")]
    [DbType("decimal(18,2)")]
    [ModelDefault("AllowEdit", "False")]
    public decimal SumaPedidos
    {
        get => _sumaPedidos;
        set => SetPropertyValue(nameof(SumaPedidos), ref _sumaPedidos, value);
    }

    [XafDisplayName("Fecha Cierre Estimada")]
    public DateTime FechaCierreEstimada
    {
        get => _fechaCierreEstimada;
        set => SetPropertyValue(nameof(FechaCierreEstimada), ref _fechaCierreEstimada, value);
    }

    [XafDisplayName("Responsable")]
    public ApplicationUser Responsable
    {
        get => _responsable;
        set => SetPropertyValue(nameof(Responsable), ref _responsable, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [DevExpress.Xpo.Aggregated]
    [Association("Oportunidad-Presupuestos")]
    [XafDisplayName("Presupuestos")]
    public XPCollection<Presupuesto> Presupuestos
    {
        get
        {
            var collection = GetCollection<Presupuesto>();
            if (!collection.IsLoaded) collection.CollectionChanged += Presupuestos_CollectionChanged;
            return collection;
        }
    }

    [DevExpress.Xpo.Aggregated]
    [Association("Oportunidad-Pedidos")]
    [XafDisplayName("Pedidos")]
    public XPCollection<Pedido> Pedidos
    {
        get
        {
            var collection = GetCollection<Pedido>();
            if (!collection.IsLoaded) collection.CollectionChanged += Pedidos_CollectionChanged;
            return collection;
        }
    }

    [Association("Oportunidad-Tareas")]
    [XafDisplayName("Tareas")]
    public XPCollection<Tarea> Tareas => GetCollection<Tarea>();

    [DevExpress.Xpo.Aggregated]
    [Association("Oportunidad-Fotos")]
    [XafDisplayName("Imágenes")]
    public XPCollection<Imagen> Imagenes => GetCollection<Imagen>();

    [DevExpress.Xpo.Aggregated]
    [Association("Oportunidad-Adjuntos")]
    [XafDisplayName("Adjuntos")]
    public XPCollection<Adjunto> Adjuntos => GetCollection<Adjunto>();

    public void ActualizarSumaPresupuestos(bool forceChangeEvents)
    {
        if (IsLoading || IsSaving) return;
        var newSuma = MoneyMath.RoundMoney(Presupuestos.Sum(p => p.BaseImponible));
        if (SumaPresupuestos != newSuma) SumaPresupuestos = newSuma;
    }

    public void ActualizarSumaPedidos(bool forceChangeEvents)
    {
        if (IsLoading || IsSaving) return;
        var newSuma = MoneyMath.RoundMoney(Pedidos.Sum(p => p.BaseImponible));
        if (SumaPedidos != newSuma) SumaPedidos = newSuma;
    }

    private void Presupuestos_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
    {
        if (IsLoading || IsSaving || IsDeleted) return;
        ActualizarSumaPresupuestos(true);
    }

    private void Pedidos_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
    {
        if (IsLoading || IsSaving || IsDeleted) return;
        ActualizarSumaPedidos(true);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadoOportunidad.Prospecto;
    }
}