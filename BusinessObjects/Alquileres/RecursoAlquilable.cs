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
public class RecursoAlquilable(Session session) : RecursoBase(session)
{
    private string? _notas;
    private Producto? _productoRelacionado;
    private bool _estaActivo;
    private string? _codigoRegistro;
    private string? _observaciones;
    private Ubicacion? _ubicacion;
    private int _secuencia;
    private Tarifa? _tarifa;

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => Caption;
        set => Caption = value;
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }
    
    [XafDisplayName("Producto Relacionado")]
    [ToolTip("Producto utilizado para la facturación de este alquiler")]
    public Producto? ProductoRelacionado
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
    
    [Size(255)]
    [XafDisplayName("Código de Registro")]
    public string? CodigoRegistro
    {
        get => _codigoRegistro;
        set => SetPropertyValue(nameof(CodigoRegistro), ref _codigoRegistro, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [XafDisplayName("Ubicación")]
    public Ubicacion? Ubicacion
    {
        get => _ubicacion;
        set => SetPropertyValue(nameof(Ubicacion), ref _ubicacion, value);
    }

    [XafDisplayName("Secuencia")]
    public int Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    [Association("Tarifa-RecursosAlquilables")]
    [XafDisplayName("Tarifa")]
    public Tarifa? Tarifa
    {
        get => _tarifa;
        set => SetPropertyValue(nameof(Tarifa), ref _tarifa, value);
    }

    [Association("RecursoAlquilable-Reservas")]
    [XafDisplayName("Reservas")]
    public XPCollection<Reserva> Reservas => GetCollection<Reserva>();

    [Association("RecursoAlquilable-Simulaciones")]
    [XafDisplayName("Simulaciones")]
    public XPCollection<Simulacion> Simulaciones => GetCollection<Simulacion>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        EstaActivo = true;
    }
}
