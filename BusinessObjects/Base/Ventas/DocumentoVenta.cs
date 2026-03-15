using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Comun;
using erp.Module.Factories;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Crm;
using erp.Module.Helpers.Contactos;
using Tarea = erp.Module.BusinessObjects.Planificacion.Tarea;
using erp.Module.Helpers.Comun;
using erp.Module.BusinessObjects.Productos;

namespace erp.Module.BusinessObjects.Base.Ventas;

public abstract class DocumentoVenta(Session session) : EntidadBase(session)
{
    private Cliente _cliente;
    private decimal _baseImponible;
    private decimal _importeImpuestos;
    private decimal _importeTotal;
    private string _serie;
    private string _numero;
    private DateTime _fecha;
    private string _notas;
    private string _codigoBarrasLector;

    [RuleRequiredField("erp.Module.BusinessObjects.Facturacion.Factura.Cliente_Required", DefaultContexts.Save, TargetCriteria = "IsInstanceOfType(this, 'erp.Module.BusinessObjects.Facturacion.Factura') or IsInstanceOfType(this, 'erp.Module.BusinessObjects.Ventas.Presupuesto')")]
    [Association("Cliente-DocumentosVenta")]
    [XafDisplayName("Cliente")]
    public Cliente Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }
    
    [XafDisplayName("Serie")]
    [RuleRequiredField]
    [Appearance("BlockSerieWhenNumeroIsSet", Enabled = false, Criteria = "!IsNewObject(this) and !IsNullOrEmpty(Numero)", Context = "Any")]
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

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [NonPersistent]
    [ImmediatePostData]
    [XafDisplayName("Capturar Código (Lector)")]
    public string CodigoBarrasLector
    {
        get => _codigoBarrasLector;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                SetPropertyValue(nameof(CodigoBarrasLector), ref _codigoBarrasLector, value);
                return;
            }

            var oldValue = _codigoBarrasLector;
            _codigoBarrasLector = value;
            OnChanged(nameof(CodigoBarrasLector), oldValue, value);

            try
            {
                var cleanedValue = value.Trim('\r', '\n', ' ');
                if (!string.IsNullOrWhiteSpace(cleanedValue))
                {
                    CapturarProductoPorCodigo(cleanedValue);
                }
            }
            finally
            {
                _codigoBarrasLector = string.Empty;
                OnChanged(nameof(CodigoBarrasLector), value, string.Empty);
            }
        }
    }

    private void CapturarProductoPorCodigo(string codigo)
    {
        var producto = Session.FindObject<Producto>(CriteriaOperator.Parse("(CodigoBarras = ? OR Codigo = ?) AND EstaActivo = True AND DisponibleEnVentas = True", codigo, codigo));
        if (producto != null)
        {
            var lineaExistente = Lineas.FirstOrDefault(l => l.Producto != null && l.Producto.Oid == producto.Oid);
            if (lineaExistente != null)
            {
                lineaExistente.Cantidad += 1;
            }
            else
            {
                var linea = new LineaDocumentoVenta(Session)
                {
                    DocumentoVenta = this,
                    Producto = producto,
                    Cantidad = 1
                };
                Lineas.Add(linea);
            }
        }
    }
    
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

        var newTaxes = groups.Select(g =>
        {
            var tax = new ImpuestoDocumentoVenta(this.Session)
            {
                DocumentoVenta = this,
                TipoImpuesto = g.TaxType,
                BaseImponible = g.BaseSum
            };
            tax.Secuencia = g.TaxType.Secuencia;
            tax.ImporteImpuestos = AmountCalculator.GetTaxAmount(g.BaseSum, g.TaxType.Tipo, g.TaxType.EsRetencion);
            return tax;
        }).ToList();

        Impuestos.AddRange(newTaxes);

        BaseImponible = MoneyMath.RoundMoney(Lineas.Sum(t => t.BaseImponible));
        ImporteImpuestos = MoneyMath.RoundMoney(Impuestos.Sum(t => t.ImporteImpuestos));
        ImporteTotal = BaseImponible + ImporteImpuestos;
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Fecha = DateTime.Today;
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        Serie ??= companyInfo?.PrefijoFacturasVentaPorDefecto;
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