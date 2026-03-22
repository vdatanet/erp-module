using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.Factories;
using erp.Module.Helpers.Comun;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Base.Ventas;

public abstract class DocumentoVenta(Session session) : EntidadBase(session)
{
    private decimal _baseImponible;
    private Tercero? _cliente;
    private DateTime _fecha;
    private decimal _importeImpuestos;
    private decimal _importeTotal;
    private string? _notas;
    private int _numero;
    private string? _secuencia;
    private string? _serie;
    private EquipoVenta? _equipoVenta;
    private Contacto? _vendedor;
    private CategoriaVenta? _categoriaVenta;
    private CondicionPago? _condicionPago;
    private Ejercicio? _ejercicio;

    [RuleRequiredField("erp.Module.BusinessObjects.Ventas.FacturaVenta.Cliente_Required", DefaultContexts.Save,
        TargetCriteria =
            "IsInstanceOfType(this, 'erp.Module.BusinessObjects.Ventas.FacturaVenta') or IsInstanceOfType(this, 'erp.Module.BusinessObjects.Ventas.OfertaVenta') or IsInstanceOfType(this, 'erp.Module.BusinessObjects.Ventas.AlbaranVenta')",
        CustomMessageTemplate = "El Cliente del documento de venta es obligatorio")]
    [Association("Tercero-DocumentosVenta")]
    [XafDisplayName("Cliente")]
    [DataSourceCriteria("Activo = true and IsIPuedeParticiparEnVentas")]
    [ImmediatePostData]
    public Tercero? Cliente
    {
        get => _cliente;
        set
        {
            var modified = SetPropertyValue(nameof(Cliente), ref _cliente, value);
            if (modified && !IsLoading && !IsSaving && value is Cliente c)
            {
                CondicionPago = c.CondicionPago;
            }
        }
    }

    [XafDisplayName("Serie")]
    [RuleRequiredField("RuleRequiredField_DocumentoVenta_Serie", DefaultContexts.Save, CustomMessageTemplate = "La Serie del documento de venta es obligatoria")]
    [Appearance("BlockSerieWhenNumeroIsSet", Enabled = false,
        Criteria = "!IsNewObject(this) and !IsNullOrEmpty(Secuencia)", Context = "Any")]
    public string? Serie
    {
        get => _serie;
        set => SetPropertyValue(nameof(Serie), ref _serie, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Número")]
    public int Numero
    {
        get => _numero;
        set => SetPropertyValue(nameof(Numero), ref _numero, value);
    }

    [NonCloneable]
    [ModelDefault("AllowEdit", "False")]
    [XafDisplayName("Secuencia")]
    public string? Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    [XafDisplayName("Fecha")]
    public DateTime Fecha
    {
        get => _fecha;
        set => SetPropertyValue(nameof(Fecha), ref _fecha, value);
    }

    [XafDisplayName("Ejercicio")]
    [RuleRequiredField("RuleRequiredField_DocumentoVenta_Ejercicio", DefaultContexts.Save)]
    public Ejercicio? Ejercicio
    {
        get => _ejercicio;
        set => SetPropertyValue(nameof(Ejercicio), ref _ejercicio, value);
    }

    [Browsable(false)]
    [RuleFromBoolProperty("DocumentoVenta_FechaNoBloqueada", DefaultContexts.Save, "La fecha del documento se encuentra en un periodo bloqueado del ejercicio o el ejercicio está bloqueado.", UsedProperties = nameof(Fecha))]
    public bool IsFechaNoBloqueada
    {
        get
        {
            if (Ejercicio == null) return true;
            if (Ejercicio.Estado == EstadoEjercicio.Bloqueado) return false;

            return !Ejercicio.PeriodosBloqueados.Any(p => Fecha >= p.FechaInicio && Fecha <= p.FechaFin);
        }
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
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

    [XafDisplayName("Equipo de Venta")]
    [Association("EquipoVenta-DocumentosVenta")]
    public EquipoVenta? EquipoVenta
    {
        get => _equipoVenta;
        set => SetPropertyValue(nameof(EquipoVenta), ref _equipoVenta, value);
    }

    [XafDisplayName("Vendedor")]
    [ToolTip("El vendedor puede ser un empleado o un agente externo (ambos son Contactos)")]
    [DataSourceCriteria("EsVendedor = true AND Activo = true")]
    [ImmediatePostData]
    public Contacto? Vendedor
    {
        get => _vendedor;
        set
        {
            var modified = SetPropertyValue(nameof(Vendedor), ref _vendedor, value);
            if (modified && !IsLoading && !IsSaving && value != null)
            {
                if (value.EquipoVenta != null)
                    EquipoVenta = value.EquipoVenta;

                foreach (var linea in Lineas)
                {
                    linea.PorcentajeComision = value.PorcentajeComision;
                    linea.ImporteComisionFijo = value.ImporteComisionFijo;
                }
            }
        }
    }

    [XafDisplayName("Categoría de Venta")]
    public CategoriaVenta? CategoriaVenta
    {
        get => _categoriaVenta;
        set => SetPropertyValue(nameof(CategoriaVenta), ref _categoriaVenta, value);
    }

    [XafDisplayName("Condición de Pago")]
    public CondicionPago? CondicionPago
    {
        get => _condicionPago;
        set => SetPropertyValue(nameof(CondicionPago), ref _condicionPago, value);
    }

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Lineas")]
    [XafDisplayName("Líneas")]
    public XPCollection<LineaDocumentoVenta> Lineas
    {
        get
        {
            var collection = GetCollection<LineaDocumentoVenta>();
            collection.CollectionChanged -= Lineas_CollectionChanged;
            collection.CollectionChanged += Lineas_CollectionChanged;
            return collection;
        }
    }

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoVenta-Impuestos")]
    [XafDisplayName("Impuestos")]
    public XPCollection<ImpuestoDocumentoVenta> Impuestos => GetCollection<ImpuestoDocumentoVenta>();

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

    private void Lineas_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
    {
        if (IsLoading || IsSaving || IsDeleted) return;
        BorrarResumenImpuestos();
        ReconstruirResumenImpuestos();
    }

    public void BorrarResumenImpuestos()
    {
        for (var i = Impuestos.Count - 1; i >= 0; i--)
            Impuestos[i].Delete();
    }

    public void ReconstruirResumenImpuestos()
    {
        var groups = Lineas.SelectMany(l => l.Impuestos)
            .Where(t => t.TipoImpuesto != null)
            .GroupBy(t => t.TipoImpuesto!)
            .Select(g => new
            {
                TaxType = g.Key,
                BaseSum = g.Sum(x => x.BaseImponible)
            })
            .OrderBy(x => x.TaxType.Secuencia)
            .ToList();

        var newTaxes = groups.Select(g =>
        {
            var tax = new ImpuestoDocumentoVenta(Session)
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
        
        Ejercicio = Session.Query<Ejercicio>().FirstOrDefault(e => e.Estado == EstadoEjercicio.Abierto && e.FechaInicio <= Fecha && e.FechaFin >= Fecha);
    }

    public virtual bool GetAsignarNumeroAlGuardar()
    {
        return true;
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        if (GetAsignarNumeroAlGuardar() && string.IsNullOrEmpty(Secuencia) && !string.IsNullOrEmpty(Serie))
            AsignarNumero();
    }

    public virtual void AsignarNumero()
    {
        if (!string.IsNullOrEmpty(Serie) && Ejercicio != null)
        {
            var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session) ?? throw new UserFriendlyException("No se ha podido obtener la configuración de la empresa.");
            int padding = companyInfo.PaddingNumero;

            string sequenceName = $"{GetType().FullName}.{Ejercicio.Anio}.{Serie}";
            string prefix = $"{Serie}/{Ejercicio.Anio}";

            Numero = SequenceFactory.GetNextSequence(Session, sequenceName, out var formattedSequence,
                prefix, padding);
            Secuencia = formattedSequence;
        }
    }
}