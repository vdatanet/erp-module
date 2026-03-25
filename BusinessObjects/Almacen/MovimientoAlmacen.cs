using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Productos;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Almacen;

public enum TipoMovimientoAlmacen
{
    Entrada,
    Salida,
    Ajuste
}

[DefaultClassOptions]
[NavigationItem("Almacén")]
[XafDisplayName("Movimiento de Almacén")]
[Persistent("MovimientoAlmacen")]
[DefaultProperty(nameof(Fecha))]
public class MovimientoAlmacen(Session session) : EntidadBase(session)
{
    private Almacen? _almacen;
    private Producto? _producto;
    private DateTime _fecha;
    private decimal _cantidad;
    private TipoMovimientoAlmacen _tipo;
    private string? _referencia;
    private string? _observaciones;

    [Association("Almacen-Movimientos")]
    [RuleRequiredField("RuleRequiredField_MovimientoAlmacen_Almacen", DefaultContexts.Save)]
    [XafDisplayName("Almacén")]
    public Almacen? Almacen
    {
        get => _almacen;
        set => SetPropertyValue(nameof(Almacen), ref _almacen, value);
    }

    [RuleRequiredField("RuleRequiredField_MovimientoAlmacen_Producto", DefaultContexts.Save)]
    [XafDisplayName("Producto")]
    public Producto? Producto
    {
        get => _producto;
        set => SetPropertyValue(nameof(Producto), ref _producto, value);
    }

    [XafDisplayName("Fecha")]
    [RuleRequiredField("RuleRequiredField_MovimientoAlmacen_Fecha", DefaultContexts.Save)]
    public DateTime Fecha
    {
        get => _fecha;
        set => SetPropertyValue(nameof(Fecha), ref _fecha, value);
    }

    [XafDisplayName("Cantidad")]
    public decimal Cantidad
    {
        get => _cantidad;
        set => SetPropertyValue(nameof(Cantidad), ref _cantidad, value);
    }

    [XafDisplayName("Tipo")]
    public TipoMovimientoAlmacen Tipo
    {
        get => _tipo;
        set => SetPropertyValue(nameof(Tipo), ref _tipo, value);
    }

    [XafDisplayName("Referencia")]
    public string? Referencia
    {
        get => _referencia;
        set => SetPropertyValue(nameof(Referencia), ref _referencia, value);
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
        Fecha = InformacionEmpresaHelper.GetLocalTime(Session);
        Tipo = TipoMovimientoAlmacen.Ajuste;
    }
}
