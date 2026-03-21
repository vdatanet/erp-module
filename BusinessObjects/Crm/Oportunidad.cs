using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Ventas;

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
    private Campana? _campana;
    private Cliente? _cliente;
    private string? _descripcion;
    private EstadoOportunidad _estado;
    private DateTime _fechaCierreEstimada;
    private Fuente? _fuente;
    private Medio? _medio;
    private string? _notas;
    private double _probabilidad;
    private ApplicationUser? _responsable;
    private EquipoVenta? _equipoVenta;
    private Contacto? _vendedor;
    private string? _titulo;
    private decimal _valorEstimado;

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Título")]
    public string? Titulo
    {
        get => _titulo;
        set => SetPropertyValue(nameof(Titulo), ref _titulo, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripción")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [RuleRequiredField]
    [Association("Cliente-Oportunidades")]
    [XafDisplayName("Cliente")]
    public Cliente? Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [Association("Campana-Oportunidades")]
    [XafDisplayName("Campaña")]
    public Campana? Campana
    {
        get => _campana;
        set => SetPropertyValue(nameof(Campana), ref _campana, value);
    }

    [Association("Medio-Oportunidades")]
    [XafDisplayName("Medio")]
    public Medio? Medio
    {
        get => _medio;
        set => SetPropertyValue(nameof(Medio), ref _medio, value);
    }

    [Association("Fuente-Oportunidades")]
    [XafDisplayName("Fuente")]
    public Fuente? Fuente
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

    [XafDisplayName("Fecha Cierre Estimada")]
    public DateTime FechaCierreEstimada
    {
        get => _fechaCierreEstimada;
        set => SetPropertyValue(nameof(FechaCierreEstimada), ref _fechaCierreEstimada, value);
    }

    [XafDisplayName("Responsable")]
    public ApplicationUser? Responsable
    {
        get => _responsable;
        set => SetPropertyValue(nameof(Responsable), ref _responsable, value);
    }

    [XafDisplayName("Equipo de Venta")]
    [Association("EquipoVenta-Oportunidades")]
    public EquipoVenta? EquipoVenta
    {
        get => _equipoVenta;
        set => SetPropertyValue(nameof(EquipoVenta), ref _equipoVenta, value);
    }

    [XafDisplayName("Vendedor")]
    [ToolTip("El vendedor puede ser un empleado o un agente externo (ambos son Contactos)")]
    [DataSourceCriteria("EsVendedor = true")]
    public Contacto? Vendedor
    {
        get => _vendedor;
        set => SetPropertyValue(nameof(Vendedor), ref _vendedor, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [Association("Oportunidad-Presupuestos")]
    [XafDisplayName("Presupuestos")]
    public XPCollection<Presupuesto> Presupuestos
    {
        get
        {
            var collection = GetCollection<Presupuesto>();
            return collection;
        }
    }

    [Association("Oportunidad-Pedidos")]
    [XafDisplayName("Pedidos")]
    public XPCollection<Pedido> Pedidos
    {
        get
        {
            var collection = GetCollection<Pedido>();
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

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadoOportunidad.Prospecto;
    }
}