using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.Helpers.Comun;

namespace erp.Module.BusinessObjects.Productos;

[DefaultClassOptions]
[NavigationItem("Productos")]
[XafDisplayName("Precio Especial")]
[ImageName("BO_Price")]
public class PrecioEspecial(Session session) : EntidadBase(session)
{
    private Producto? _producto;
    private Tercero? _tercero;
    private decimal _precio;
    private decimal _descuento1;
    private decimal _descuento2;
    private decimal _descuento3;
    private string? _notas;
    private ContextoPrecio _contexto;
    private DateTime? _vigenteDesde;
    private DateTime? _vigenteHasta;

    [XafDisplayName("Contexto")]
    public ContextoPrecio Contexto
    {
        get => _contexto;
        set => SetPropertyValue(nameof(Contexto), ref _contexto, value);
    }

    [XafDisplayName("Vigente Desde")]
    public DateTime? VigenteDesde
    {
        get => _vigenteDesde;
        set => SetPropertyValue(nameof(VigenteDesde), ref _vigenteDesde, value);
    }

    [XafDisplayName("Vigente Hasta")]
    public DateTime? VigenteHasta
    {
        get => _vigenteHasta;
        set => SetPropertyValue(nameof(VigenteHasta), ref _vigenteHasta, value);
    }

    [Association("Producto-PreciosEspeciales")]
    [XafDisplayName("Producto")]
    [LookupEditorMode(LookupEditorMode.Search)]
    [ImmediatePostData]
    [DataSourceCriteria("EstaActivo = True")]
    public Producto? Producto
    {
        get => _producto;
        set => SetPropertyValue(nameof(Producto), ref _producto, value);
    }

    [Association("Tercero-PreciosEspeciales")]
    [XafDisplayName("Cliente / Proveedor / Acreedor")]
    [LookupEditorMode(LookupEditorMode.Search)]
    [ImmediatePostData]
    [DataSourceCriteria("Activo = True")]
    public Tercero? Tercero
    {
        get => _tercero;
        set => SetPropertyValue(nameof(Tercero), ref _tercero, value);
    }

    [XafDisplayName("Precio")]
    public decimal Precio
    {
        get => _precio;
        set => SetPropertyValue(nameof(Precio), ref _precio, value);
    }

    [XafDisplayName("Descuento")]
    [ToolTip("Ejemplo: 10+5+2")]
    [NonPersistent]
    public string? TextoDescuento
    {
        get => DiscountParser.Format(Descuento1, Descuento2, Descuento3);
        set
        {
            var (d1, d2, d3) = DiscountParser.Parse(value);
            Descuento1 = d1;
            Descuento2 = d2;
            Descuento3 = d3;
            OnChanged(nameof(TextoDescuento));
        }
    }

    [XafDisplayName("% Descuento 1")]
    public decimal Descuento1
    {
        get => _descuento1;
        set
        {
            if (SetPropertyValue(nameof(Descuento1), ref _descuento1, value) && !IsLoading)
                OnChanged(nameof(TextoDescuento));
        }
    }

    [XafDisplayName("% Descuento 2")]
    public decimal Descuento2
    {
        get => _descuento2;
        set
        {
            if (SetPropertyValue(nameof(Descuento2), ref _descuento2, value) && !IsLoading)
                OnChanged(nameof(TextoDescuento));
        }
    }

    [XafDisplayName("% Descuento 3")]
    public decimal Descuento3
    {
        get => _descuento3;
        set
        {
            if (SetPropertyValue(nameof(Descuento3), ref _descuento3, value) && !IsLoading)
                OnChanged(nameof(TextoDescuento));
        }
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Contexto = ContextoPrecio.Ambos;
    }
}
