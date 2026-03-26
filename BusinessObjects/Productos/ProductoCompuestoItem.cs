using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Productos;

[DefaultClassOptions]
[XafDisplayName("Componente de Producto")]
public class ProductoCompuestoItem(Session session) : EntidadBase(session)
{
    private Producto? _productoPadre;
    private Producto? _componente;
    private decimal _cantidad;

    [Association("Producto-Componentes")]
    [XafDisplayName("Producto Padre")]
    public Producto? ProductoPadre
    {
        get => _productoPadre;
        set
        {
            if (SetPropertyValue(nameof(ProductoPadre), ref _productoPadre, value))
            {
                if (!IsLoading && !IsSaving && value != null)
                {
                    value.EsCompuesto = true;
                }
            }
        }
    }

    [XafDisplayName("Componente")]
    [RuleRequiredField]
    [DataSourceCriteria("Oid != '@this.ProductoPadre.Oid'")]
    [Association("Componente-ProductosPadres")]
    public Producto? Componente
    {
        get => _componente;
        set
        {
            if (SetPropertyValue(nameof(Componente), ref _componente, value))
            {
                if (!IsLoading && !IsSaving && ProductoPadre != null)
                {
                    ProductoPadre.RecalcularPreciosDesdeComponentes();
                }
            }
        }
    }

    [XafDisplayName("Cantidad")]
    [RuleValueComparison("", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0)]
    public decimal Cantidad
    {
        get => _cantidad;
        set
        {
            if (SetPropertyValue(nameof(Cantidad), ref _cantidad, value))
            {
                if (!IsLoading && !IsSaving && ProductoPadre != null)
                {
                    ProductoPadre.RecalcularPreciosDesdeComponentes();
                }
            }
        }
    }
}
