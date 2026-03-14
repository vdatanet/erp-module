using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Comun;
using erp.Module.Factories;
using Tarea = erp.Module.BusinessObjects.Planificacion.Tarea;

namespace erp.Module.BusinessObjects.Base.Ventas;

public abstract class DocumentoVenta(Session session) : EntidadBase(session)
{
    private decimal _baseImponible;
    private decimal _importeImpuestos;
    private decimal _importeTotal;
    private string _serie;
    private string _numero;
    private DateTime _fecha;
    
    [XafDisplayName("Serie")]
    [RuleRequiredField]
    public string Serie
    {
        get => _serie;
        set => SetPropertyValue(nameof(Serie), ref _serie, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Número")]
    public string Numero
    {
        get => _numero;
        set => SetPropertyValue(nameof(Numero), ref _numero, value);
    }

    [XafDisplayName("Fecha")]
    public DateTime Fecha
    {
        get => _fecha;
        set => SetPropertyValue(nameof(Fecha), ref _fecha, value);
    }

    [ModelDefault("AllowEdit","False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Base Imponible")]
    public decimal BaseImponible
    {
        get => _baseImponible;
        set => SetPropertyValue(nameof(BaseImponible), ref _baseImponible, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Impuestos")]
    public decimal ImporteImpuestos
    {
        get => _importeImpuestos;
        set => SetPropertyValue(nameof(ImporteImpuestos), ref _importeImpuestos, value);
    }

    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [XafDisplayName("Total")]
    public decimal ImporteTotal
    {
        get => _importeTotal;
        set => SetPropertyValue(nameof(ImporteTotal), ref _importeTotal, value);
    }

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Lineas")]
    [XafDisplayName("Líneas")]
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
    
    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Impuestos")]
    [XafDisplayName("Impuestos")]
    public XPCollection<ImpuestoDocumentoVenta> Impuestos => GetCollection<ImpuestoDocumentoVenta>();
    
    private void Lineas_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
    {
        if (IsLoading || IsSaving || IsDeleted) return;
        if (e.CollectionChangedType != XPCollectionChangedType.AfterRemove) return;
        BorrarResumenImpuestos();
        ReconstruirResumenImpuestos();
    }

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Tareas")]
    [XafDisplayName("Tareas")]
    public XPCollection<Tarea> Tareas => GetCollection<Tarea>();

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Fotos")]
    [XafDisplayName("Imágenes")]
    public XPCollection<Imagen> Imagenes => GetCollection<Imagen>();

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Adjuntos")]
    [XafDisplayName("Adjuntos")]
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

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Fecha = DateTime.Today;
    }

    public virtual bool GetAsignarNumeroAlGuardar() => true;

    protected override void OnSaving()
    {
        base.OnSaving();
        if (GetAsignarNumeroAlGuardar() && string.IsNullOrEmpty(Numero) && !string.IsNullOrEmpty(Serie))
        {
            AsignarNumero();
        }
    }

    public virtual void AsignarNumero()
    {
        Numero = SequenceFactory.GetNextSequence(Session, $"{this.GetType().FullName}.{Serie}", Serie, 5);
    }
}