using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.BusinessObjects.Productos;
using System.ComponentModel;

namespace erp.Module.BusinessObjects.Tpv;

[XafDisplayName("Línea de Venta TPV")]
[Persistent("LineaVentaTpv")]
public class LineaVentaTpv(Session session) : EntidadBase(session)
{
    private VentaTpv? _ventaTpv;
    private Producto? _producto;
    private string? _descripcion;
    private decimal _cantidad;
    private decimal _precioUnitario;
    private decimal _descuentoPorcentaje;
    private decimal _descuentoImporte;
    private decimal _baseImponible;
    private decimal _impuestoImporte;
    private decimal _totalLinea;

    [XafDisplayName("Venta TPV")]
    [Association("VentaTpv-Lineas")]
    public VentaTpv? VentaTpv
    {
        get => _ventaTpv;
        set => SetPropertyValue(nameof(VentaTpv), ref _ventaTpv, value);
    }

    [XafDisplayName("Producto")]
    [RuleRequiredField]
    public Producto? Producto
    {
        get => _producto;
        set 
        {
            if (SetPropertyValue(nameof(Producto), ref _producto, value) && !IsLoading && value != null)
            {
                Descripcion = value.Nombre;
                PrecioUnitario = value.PrecioVenta;
                Recalcular();
            }
        }
    }

    [XafDisplayName("Descripción")]
    [Size(SizeAttribute.Unlimited)]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [XafDisplayName("Cantidad")]
    public decimal Cantidad
    {
        get => _cantidad;
        set 
        {
            if (SetPropertyValue(nameof(Cantidad), ref _cantidad, value) && !IsLoading)
                Recalcular();
        }
    }

    [XafDisplayName("Precio Unitario")]
    public decimal PrecioUnitario
    {
        get => _precioUnitario;
        set 
        {
            if (SetPropertyValue(nameof(PrecioUnitario), ref _precioUnitario, value) && !IsLoading)
                Recalcular();
        }
    }

    [XafDisplayName("Descuento %")]
    public decimal DescuentoPorcentaje
    {
        get => _descuentoPorcentaje;
        set 
        {
            if (SetPropertyValue(nameof(DescuentoPorcentaje), ref _descuentoPorcentaje, value) && !IsLoading)
                Recalcular();
        }
    }

    [XafDisplayName("Descuento Importe")]
    public decimal DescuentoImporte
    {
        get => _descuentoImporte;
        set => SetPropertyValue(nameof(DescuentoImporte), ref _descuentoImporte, value);
    }

    [XafDisplayName("Base Imponible")]
    public decimal BaseImponible
    {
        get => _baseImponible;
        set => SetPropertyValue(nameof(BaseImponible), ref _baseImponible, value);
    }

    [XafDisplayName("Impuesto Importe")]
    public decimal ImpuestoImporte
    {
        get => _impuestoImporte;
        set => SetPropertyValue(nameof(ImpuestoImporte), ref _impuestoImporte, value);
    }

    [XafDisplayName("Total Línea")]
    public decimal TotalLinea
    {
        get => _totalLinea;
        set => SetPropertyValue(nameof(TotalLinea), ref _totalLinea, value);
    }

    [Association("LineaVentaTpv-Impuestos")]
    [XafDisplayName("Impuestos")]
    public XPCollection<TipoImpuesto> Impuestos => GetCollection<TipoImpuesto>();

    [Association("LineaVentaTpv-Descuentos")]
    [XafDisplayName("Descuentos")]
    public XPCollection<DescuentoVentaTpv> Descuentos => GetCollection<DescuentoVentaTpv>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Cantidad = 1;
    }

    public void Recalcular()
    {
        decimal bruto = Cantidad * PrecioUnitario;
        DescuentoImporte = bruto * (DescuentoPorcentaje / 100);
        BaseImponible = bruto - DescuentoImporte;
        
        // Simplificación para el ejemplo, en un caso real se calcularía por cada tipo de impuesto
        decimal porcentajeImpuesto = 0;
        foreach(var imp in Impuestos)
        {
            porcentajeImpuesto += imp.Tipo;
        }
        
        ImpuestoImporte = BaseImponible * (porcentajeImpuesto / 100);
        TotalLinea = BaseImponible + ImpuestoImporte;

        VentaTpv?.RecalcularTotales();
    }
}
