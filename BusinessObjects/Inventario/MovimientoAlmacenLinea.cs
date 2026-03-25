using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Productos;

namespace erp.Module.BusinessObjects.Inventario;

[DefaultClassOptions]
[NavigationItem("Inventario")]
[XafDisplayName("Línea de Movimiento de Almacén")]
[Persistent("MovimientoAlmacenLinea")]
public class MovimientoAlmacenLinea(Session session) : EntidadBase(session)
{
    private MovimientoAlmacen? _movimiento;
    private Producto? _producto;
    private decimal _cantidad;
    private string? _observaciones;

    [Association("Movimiento-Lineas")]
    [XafDisplayName("Movimiento")]
    [RuleRequiredField("RuleRequiredField_MovimientoAlmacenLinea_Movimiento", DefaultContexts.Save)]
    public MovimientoAlmacen? Movimiento
    {
        get => _movimiento;
        set => SetPropertyValue(nameof(Movimiento), ref _movimiento, value);
    }

    [RuleRequiredField("RuleRequiredField_MovimientoAlmacenLinea_Producto", DefaultContexts.Save)]
    [XafDisplayName("Producto")]
    public Producto? Producto
    {
        get => _producto;
        set => SetPropertyValue(nameof(Producto), ref _producto, value);
    }

    [XafDisplayName("Cantidad")]
    public decimal Cantidad
    {
        get => _cantidad;
        set => SetPropertyValue(nameof(Cantidad), ref _cantidad, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Cantidad = 1;
    }
}
