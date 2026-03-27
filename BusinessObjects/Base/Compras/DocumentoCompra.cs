using System.ComponentModel;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Documentos;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.Factories;
using erp.Module.Helpers.Comun;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Base.Compras;

public abstract class DocumentoCompra(Session session) : EntidadBase(session)
{
    private decimal _baseImponible;
    private DateTime _fecha;
    private decimal _importeImpuestos;
    private decimal _importeTotal;
    private string? _notas;
    private int _numero;
    private Tercero? _proveedor;
    private string? _secuencia;
    private string? _serie;
    private CondicionPago? _condicionPago;
    private MedioPago? _medioPago;
    private Ejercicio? _ejercicio;

    [RuleRequiredField("erp.Module.BusinessObjects.Compras.FacturaCompra.Proveedor_Required", DefaultContexts.Save,
        TargetCriteria =
            "IsInstanceOfType(this, 'erp.Module.BusinessObjects.Compras.FacturaCompra') or IsInstanceOfType(this, 'erp.Module.BusinessObjects.Compras.OfertaCompra') or IsInstanceOfType(this, 'erp.Module.BusinessObjects.Compras.AlbaranCompra')",
        CustomMessageTemplate = "El Proveedor del documento de compra es obligatorio")]
    [Association("Tercero-DocumentosCompra")]
    [XafDisplayName("Proveedor")]
    [DataSourceCriteria("Activo = true and IsIPuedeParticiparEnCompras")]
    [ImmediatePostData]
    public Tercero? Proveedor
    {
        get => _proveedor;
        set
        {
            var modified = SetPropertyValue(nameof(Proveedor), ref _proveedor, value);
            if (modified && !IsLoading && !IsSaving)
            {
                AsignarProveedor(value);
            }
        }
    }

    [XafDisplayName("Serie")]
    [RuleRequiredField("RuleRequiredField_DocumentoCompra_Serie", DefaultContexts.Save, CustomMessageTemplate = "La Serie del documento de compra es obligatoria")]
    [Appearance("BlockSerieWhenNumeroIsSetCompra", Enabled = false,
        Criteria = "!IsNewObject(this) and !IsNullOrEmpty(Secuencia)", Context = "Any")]
    public string? Serie
    {
        get => _serie;
        set => SetPropertyValue(nameof(Serie), ref _serie, value);
    }

    [XafDisplayName("Condición de Pago")]
    public CondicionPago? CondicionPago
    {
        get => _condicionPago;
        set => SetPropertyValue(nameof(CondicionPago), ref _condicionPago, value);
    }

    [XafDisplayName("Medio de Pago")]
    public MedioPago? MedioPago
    {
        get => _medioPago;
        set => SetPropertyValue(nameof(MedioPago), ref _medioPago, value);
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
    [RuleRequiredField("RuleRequiredField_DocumentoCompra_Ejercicio", DefaultContexts.Save)]
    public Ejercicio? Ejercicio
    {
        get => _ejercicio;
        set => SetPropertyValue(nameof(Ejercicio), ref _ejercicio, value);
    }

    [Browsable(false)]
    [RuleFromBoolProperty("DocumentoCompra_FechaNoBloqueada", DefaultContexts.Save, "La fecha del documento se encuentra en un periodo bloqueado del ejercicio o el ejercicio está bloqueado.", UsedProperties = nameof(Fecha))]
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

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoCompra-DocumentoCompraLineas")]
    [XafDisplayName("Líneas")]
    public XPCollection<DocumentoCompraLinea> Lineas
    {
        get
        {
            var collection = GetCollection<DocumentoCompraLinea>();
            collection.CollectionChanged -= Lineas_CollectionChanged;
            collection.CollectionChanged += Lineas_CollectionChanged;
            return collection;
        }
    }

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoCompra-DocumentoCompraImpuestos")]
    [XafDisplayName("Impuestos")]
    public XPCollection<DocumentoCompraImpuesto> Impuestos => GetCollection<DocumentoCompraImpuesto>();

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoCompra-Tareas")]
    [XafDisplayName("Tareas")]
    public XPCollection<Tarea> Tareas => GetCollection<Tarea>();

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoCompra-Fotos")]
    [XafDisplayName("Imágenes")]
    public XPCollection<Imagen> Imagenes => GetCollection<Imagen>();

    [DevExpress.Xpo.Aggregated]
    [Association("DocumentoCompra-Documentos")]
    [XafDisplayName("Documentos")]
    public XPCollection<Documento> Documentos => GetCollection<Documento>();

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
            var tax = new DocumentoCompraImpuesto(Session)
            {
                DocumentoCompra = this,
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

    /// <summary>
    ///     Regla de negocio: Al asignar un proveedor se copian sus datos de referencia al documento.
    /// </summary>
    public virtual void AsignarProveedor(Tercero? value)
    {
        if (value == null)
        {
            CondicionPago = null;
            MedioPago = null;
            return;
        }

        CondicionPago = value.CondicionPago;
        MedioPago = value.MedioPago;
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

            var sequenceName = companyInfo.TipoNumeracionDocumento switch
            {
                TipoNumeracionDocumento.PrefijoNumero => $"{GetType().FullName}.{Serie}",
                TipoNumeracionDocumento.PrefijoEjercicioNumero => $"{GetType().FullName}.{Ejercicio.Anio}.{Serie}",
                TipoNumeracionDocumento.PrefijoEjercicioMesNumero =>
                    $"{GetType().FullName}.{Ejercicio.Anio}.{Fecha.Month:D2}.{Serie}",
                _ => $"{GetType().FullName}.{Ejercicio.Anio}.{Serie}"
            };

            var prefix = Serie;

            Numero = SequenceFactory.GetNextSequence(Session, sequenceName, out var formattedSequence,
                prefix, padding, companyInfo, Fecha);
            Secuencia = formattedSequence;
        }
    }
}