using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Productos;

namespace erp.Module.BusinessObjects.Produccion;

[DefaultClassOptions]
[NavigationItem("Producción")]
[XafDisplayName("Parte de Trabajo - Material")]
public class ParteTrabajoMaterial(Session session) : EntidadBase(session)
{
    private Parte? _parte;
    private Producto? _producto;
    private string? _descripcion;
    private double _cantidad;
    private decimal _precioCoste;
    private decimal _precioVenta;
    private bool _facturable;

    [Association("Parte-Materiales")]
    [XafDisplayName("Parte")]
    public Parte? Parte
    {
        get => _parte;
        set => SetPropertyValue(nameof(Parte), ref _parte, value);
    }

    [XafDisplayName("Producto")]
    public Producto? Producto
    {
        get => _producto;
        set
        {
            if (SetPropertyValue(nameof(Producto), ref _producto, value))
            {
                if (!IsLoading && !IsSaving && value != null)
                {
                    Descripcion = value.Nombre;
                    PrecioCoste = value.CosteEstandar;
                    PrecioVenta = value.PrecioVenta;
                }
            }
        }
    }

    [XafDisplayName("Descripción")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [XafDisplayName("Cantidad")]
    public double Cantidad
    {
        get => _cantidad;
        set => SetPropertyValue(nameof(Cantidad), ref _cantidad, value);
    }

    [XafDisplayName("Precio Coste")]
    public decimal PrecioCoste
    {
        get => _precioCoste;
        set => SetPropertyValue(nameof(PrecioCoste), ref _precioCoste, value);
    }

    [XafDisplayName("Precio Venta")]
    public decimal PrecioVenta
    {
        get => _precioVenta;
        set => SetPropertyValue(nameof(PrecioVenta), ref _precioVenta, value);
    }

    [XafDisplayName("¿Facturable?")]
    public bool Facturable
    {
        get => _facturable;
        set => SetPropertyValue(nameof(Facturable), ref _facturable, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Cantidad = 1;
        Facturable = true;
    }
}
