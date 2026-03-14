using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Comun;
using Tarea = erp.Module.BusinessObjects.Planificacion.Tarea;

namespace erp.Module.BusinessObjects.Base.Ventas;

public abstract class DocumentoVenta(Session session) : EntidadBase(session)
{
    private decimal _baseImponible;
    private decimal _importeImpuestos;
    private decimal _importeTotal;
    
    [ModelDefault("AllowEdit","False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal BaseImponible
    {
        get => _baseImponible;
        set => SetPropertyValue(nameof(BaseImponible), ref _baseImponible, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal ImporteImpuestos
    {
        get => _importeImpuestos;
        set => SetPropertyValue(nameof(ImporteImpuestos), ref _importeImpuestos, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal ImporteTotal
    {
        get => _importeTotal;
        set => SetPropertyValue(nameof(ImporteTotal), ref _importeTotal, value);
    }

    [Aggregated]
    [Association("DocumentoVenta-Lines")]
    public XPCollection<LineaDocumentoVenta> Lineas
    {
        get
        {
            var collection = GetCollection<LineaDocumentoVenta>(nameof(Lineas));
            if (!collection.IsLoaded)
            {
                collection.CollectionChanged += Lineas_CollectionChanged;
            }
            return collection;
        }
    }
    
    [Aggregated]
    [Association("DocumentoVenta-Taxes")]
    public XPCollection<ImpuestoDocumentoVenta> Impuestos => GetCollection<ImpuestoDocumentoVenta>();
    
    private void Lineas_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
    {
        if (IsLoading || IsSaving || IsDeleted) return;
        if (e.CollectionChangedType != XPCollectionChangedType.AfterRemove) return;
        BorrarResumenImpuestos();
        ReconstruirResumenImpuestos();
    }

    [Aggregated]
    [Association("DocumentoVenta-Tareas")]
    public XPCollection<Tarea> Tareas => GetCollection<Tarea>();

    [Aggregated]
    [Association("DocumentoVenta-Pictures")]
    public XPCollection<Imagen> Imagenes => GetCollection<Imagen>();

    [Aggregated]
    [Association("DocumentoVenta-Attachments")]
    public XPCollection<Adjunto> Adjuntos => GetCollection<Adjunto>();

    public void BorrarResumenImpuestos()
    {
        for (var i = Impuestos.Count - 1; i >= 0; i--)
            Impuestos[i].Delete();
    }

    public void ReconstruirResumenImpuestos()
    {
        var groups = Lineas.SelectMany(l => l.Impuestos)
            .GroupBy(t => t.TipoImpuesto)
            .Select(g => new
            {
                TaxType = g.Key,
                BaseSum = g.Sum(x => x.BaseImponible)
            })
            .OrderBy(x => x.TaxType.Secuencia)
            .ToList();

        var newTaxes = groups.Select(g => new ImpuestoDocumentoVenta(this.Session)
        {
            DocumentoVenta = this,
            TipoImpuesto = g.TaxType,
            Secuencia = g.TaxType.Secuencia,
            BaseImponible = g.BaseSum
        });

        Impuestos.AddRange(newTaxes);

        BaseImponible = Lineas.Sum(t => t.BaseImponible);
        ImporteImpuestos = Lineas.Sum(t => t.ImporteImpuestos);
        ImporteTotal = BaseImponible + ImporteImpuestos;
    }
    
}