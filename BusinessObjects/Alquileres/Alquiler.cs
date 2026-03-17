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
    private Capacidades _capacidad;
    private string _hut;
    private string _observaciones;
    private Plantas _planta;
    private int _secuencia;
    private Tarifa _tarifa;
    private TiposAlquilerDetalle _tipoDetalle;

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => Caption;
        set => Caption = value;
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripción")]
    public string Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [ImmediatePostData]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Precio por Día")]
    public decimal PrecioPorDia
    {
        get => _precioPorDia;
        set => SetPropertyValue(nameof(PrecioPorDia), ref _precioPorDia, value);
    }

    [Association("TipoAlquiler-Alquileres")]
    [RuleRequiredField]
    [XafDisplayName("Tipo de Alquiler")]
    public TipoAlquiler TipoAlquiler
    {
        get => _tipoAlquiler;
        set => SetPropertyValue(nameof(TipoAlquiler), ref _tipoAlquiler, value);
    }

    [XafDisplayName("Producto Relacionado")]
    [ToolTip("Producto utilizado para la facturación de este alquiler")]
    public Producto ProductoRelacionado
    {
        get => _productoRelacionado;
        set => SetPropertyValue(nameof(ProductoRelacionado), ref _productoRelacionado, value);
    }

    [XafDisplayName("Activo")]
    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    [XafDisplayName("Capacidad")]
    public Capacidades Capacidad
    {
        get => _capacidad;
        set => SetPropertyValue(nameof(Capacidad), ref _capacidad, value);
    }

    [Size(255)]
    [XafDisplayName("HUT")]
    public string Hut
    {
        get => _hut;
        set => SetPropertyValue(nameof(Hut), ref _hut, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [XafDisplayName("Planta")]
    public Plantas Planta
    {
        get => _planta;
        set => SetPropertyValue(nameof(Planta), ref _planta, value);
    }

    [XafDisplayName("Secuencia")]
    public int Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    [Association("Tarifa-Alquileres")]
    [XafDisplayName("Tarifa")]
    public Tarifa Tarifa
    {
        get => _tarifa;
        set => SetPropertyValue(nameof(Tarifa), ref _tarifa, value);
    }

    [XafDisplayName("Tipo Detalle")]
    public TiposAlquilerDetalle TipoDetalle
    {
        get => _tipoDetalle;
        set => SetPropertyValue(nameof(TipoDetalle), ref _tipoDetalle, value);
    }

    [Association("Alquiler-Reservas")]
    [XafDisplayName("Reservas")]
    public XPCollection<Reserva> Reservas => GetCollection<Reserva>();

    [Association("Alquiler-Simulaciones")]
    [XafDisplayName("Simulaciones")]
    public XPCollection<Simulacion> Simulaciones => GetCollection<Simulacion>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        EstaActivo = true;
        TipoDetalle = TiposAlquilerDetalle.A1;
        Planta = Plantas.Baja;
        Capacidad = Capacidades.Dos;
    }

    public enum Plantas
    {
        [XafDisplayName("Baja")]
        Baja,
        [XafDisplayName("Primera")]
        Primera,
        [XafDisplayName("Segunda")]
        Segunda,
        [XafDisplayName("Tercera")]
        Tercera
    }

    public enum Capacidades
    {
        [XafDisplayName("2/4")]
        Dos,
        [XafDisplayName("4/6")]
        Cuatro
    }

    public enum TiposAlquilerDetalle
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
