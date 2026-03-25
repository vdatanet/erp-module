using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Inventario;

public enum TipoMovimientoAlmacen
{
    Entrada,
    Salida,
    Ajuste,
    Transferencia
}

[DefaultClassOptions]
[NavigationItem("Inventario")]
[XafDisplayName("Movimiento de Almacén")]
[Persistent("MovimientoAlmacen")]
[DefaultProperty(nameof(Fecha))]
public class MovimientoAlmacen(Session session) : EntidadBase(session)
{
    private Almacen? _almacen;
    private Almacen? _almacenDestino;
    private DateTime _fecha;
    private TipoMovimientoAlmacen _tipo;
    private string? _referencia;
    private string? _observaciones;

    [Association("Almacen-Movimientos")]
    [RuleRequiredField("RuleRequiredField_MovimientoAlmacen_Almacen", DefaultContexts.Save)]
    [XafDisplayName("Almacén Origen")]
    public Almacen? Almacen
    {
        get => _almacen;
        set => SetPropertyValue(nameof(Almacen), ref _almacen, value);
    }

    [XafDisplayName("Almacén Destino")]
    public Almacen? AlmacenDestino
    {
        get => _almacenDestino;
        set => SetPropertyValue(nameof(AlmacenDestino), ref _almacenDestino, value);
    }

    [XafDisplayName("Fecha")]
    [RuleRequiredField("RuleRequiredField_MovimientoAlmacen_Fecha", DefaultContexts.Save)]
    public DateTime Fecha
    {
        get => _fecha;
        set => SetPropertyValue(nameof(Fecha), ref _fecha, value);
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

    [Association("Movimiento-Lineas")]
    [DevExpress.Xpo.Aggregated]
    [XafDisplayName("Líneas")]
    public XPCollection<MovimientoAlmacenLinea> Lineas => GetCollection<MovimientoAlmacenLinea>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Fecha = InformacionEmpresaHelper.GetLocalTime(Session);
        Tipo = TipoMovimientoAlmacen.Ajuste;
    }
}
