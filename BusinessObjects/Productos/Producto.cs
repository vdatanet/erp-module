using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.BusinessObjects.Inventario;
using erp.Module.Helpers.Contactos;
using erp.Module.Helpers.Comun;

namespace erp.Module.BusinessObjects.Productos;

[DefaultClassOptions]
[NavigationItem("Productos")]
[XafDisplayName("Producto")]
[ImageName("BO_Product")]
[DefaultProperty(nameof(Codigo))]
public class Producto(Session session) : EntidadBase(session)
{
    private Categoria? _categoria;
    private string? _codigo;
    private string? _codigoBarras;
    private decimal _costeEstandar;
    private CuentaContable? _cuentaCompras;
    private CuentaContable? _cuentaVentas;
    private bool _disponibleEnCompras;
    private bool _disponibleEnTpv;
    private bool _disponibleEnVentas;
    private bool _disponibleEnWeb;
    private bool _gestionaStock;
    private bool _permiteVentaSinStock;
    private bool _requiereReservaStock;
    private bool _esServicio;
    private bool _esConsumible;
    private bool _esCompuesto;
    private UnidadFacturacion? _unidadFacturacion;
    private bool _estaActivo;
    private MediaDataObject? _foto;
    private MediaDataObject? _miniatura;
    private string? _nombre;
    private string? _notas;
    private decimal _precioVenta;

    [RuleUniqueValue]
    [XafDisplayName("Código")]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [RuleUniqueValue]
    [XafDisplayName("Código Barras")]
    public string? CodigoBarras
    {
        get => _codigoBarras;
        set => SetPropertyValue(nameof(CodigoBarras), ref _codigoBarras, value);
    }

    [RuleUniqueValue]
    [RuleRequiredField("RuleRequiredField_Producto_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre del Producto es obligatorio")]
    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Association("Categoria-Productos")]
    [DataSourceCriteria("EstaActivo = True")]
    [XafDisplayName("Categoría")]
    public Categoria? Categoria
    {
        get => _categoria;
        set => SetPropertyValue(nameof(Categoria), ref _categoria, value);
    }

    [XafDisplayName("Coste Estándar")]
    public decimal CosteEstandar
    {
        get => _costeEstandar;
        set => SetPropertyValue(nameof(CosteEstandar), ref _costeEstandar, value);
    }

    [XafDisplayName("Precio Venta")]
    public decimal PrecioVenta
    {
        get => _precioVenta;
        set => SetPropertyValue(nameof(PrecioVenta), ref _precioVenta, value);
    }

    [PersistentAlias("PrecioVenta - CosteEstandar")]
    [XafDisplayName("Margen Bruto (Importe)")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("AllowEdit", "False")]
    public decimal MargenBrutoImporte => Convert.ToDecimal(EvaluateAlias(nameof(MargenBrutoImporte)));

    [PersistentAlias("Iif(PrecioVenta != 0, (PrecioVenta - CosteEstandar) * 100 / PrecioVenta, 0)")]
    [XafDisplayName("Margen Bruto (%)")]
    [ModelDefault("DisplayFormat", "{0:n2}%")]
    [ModelDefault("AllowEdit", "False")]
    public decimal MargenBrutoPorcentaje => Convert.ToDecimal(EvaluateAlias(nameof(MargenBrutoPorcentaje)));

    [XafDisplayName("Precio Venta con Impuestos")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    public decimal PrecioVentaConImpuestos
    {
        get
        {
            decimal porcentajeImpuesto = 0;
            foreach (var imp in ImpuestosVentas)
            {
                porcentajeImpuesto += imp.Tipo;
            }
            return PrecioVenta * (1 + (porcentajeImpuesto / 100));
        }
    }

    [XafDisplayName("Cuenta Contable Ventas")]
    public CuentaContable? CuentaVentas
    {
        get => _cuentaVentas;
        set => SetPropertyValue(nameof(CuentaVentas), ref _cuentaVentas, value);
    }

    [XafDisplayName("Cuenta Contable Compras")]
    public CuentaContable? CuentaCompras
    {
        get => _cuentaCompras;
        set => SetPropertyValue(nameof(CuentaCompras), ref _cuentaCompras, value);
    }

    [XafDisplayName("Activo")]
    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    [XafDisplayName("Disponible en Ventas")]
    public bool DisponibleEnVentas
    {
        get => _disponibleEnVentas;
        set => SetPropertyValue(nameof(DisponibleEnVentas), ref _disponibleEnVentas, value);
    }

    [XafDisplayName("Disponible en Compras")]
    public bool DisponibleEnCompras
    {
        get => _disponibleEnCompras;
        set => SetPropertyValue(nameof(DisponibleEnCompras), ref _disponibleEnCompras, value);
    }

    [XafDisplayName("Disponible en TPV")]
    public bool DisponibleEnTpv
    {
        get => _disponibleEnTpv;
        set => SetPropertyValue(nameof(DisponibleEnTpv), ref _disponibleEnTpv, value);
    }

    [XafDisplayName("Disponible en Web")]
    public bool DisponibleEnWeb
    {
        get => _disponibleEnWeb;
        set => SetPropertyValue(nameof(DisponibleEnWeb), ref _disponibleEnWeb, value);
    }

    [XafDisplayName("Gestiona Stock")]
    public bool GestionaStock
    {
        get => _gestionaStock;
        set => SetPropertyValue(nameof(GestionaStock), ref _gestionaStock, value);
    }

    [XafDisplayName("Permite Venta Sin Stock")]
    public bool PermiteVentaSinStock
    {
        get => _permiteVentaSinStock;
        set => SetPropertyValue(nameof(PermiteVentaSinStock), ref _permiteVentaSinStock, value);
    }

    [XafDisplayName("Requiere Reserva Stock")]
    public bool RequiereReservaStock
    {
        get => _requiereReservaStock;
        set => SetPropertyValue(nameof(RequiereReservaStock), ref _requiereReservaStock, value);
    }

    [XafDisplayName("Es Servicio")]
    public bool EsServicio
    {
        get => _esServicio;
        set => SetPropertyValue(nameof(EsServicio), ref _esServicio, value);
    }

    [XafDisplayName("Es Consumible")]
    public bool EsConsumible
    {
        get => _esConsumible;
        set => SetPropertyValue(nameof(EsConsumible), ref _esConsumible, value);
    }

    [XafDisplayName("Es Compuesto")]
    public bool EsCompuesto
    {
        get => _esCompuesto;
        set
        {
            if (SetPropertyValue(nameof(EsCompuesto), ref _esCompuesto, value))
            {
                if (value && !IsLoading && !IsSaving)
                {
                    RecalcularPreciosDesdeComponentes();
                }
            }
        }
    }

    [XafDisplayName("Unidad de Facturación")]
    public UnidadFacturacion? UnidadFacturacion
    {
        get => _unidadFacturacion;
        set => SetPropertyValue(nameof(UnidadFacturacion), ref _unidadFacturacion, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [XafDisplayName("Foto")]
    public MediaDataObject? Foto
    {
        get => _foto;
        set
        {
            var oldFoto = _foto;
            if (SetPropertyValue(nameof(Foto), ref _foto, value))
            {
                if (oldFoto != null)
                {
                    oldFoto.Changed -= Foto_Changed;
                }
                if (_foto != null)
                {
                    _foto.Changed += Foto_Changed;
                }
                if (!IsSaving)
                {
                    UpdateThumbnail(value);
                }
            }
        }
    }

    private void Foto_Changed(object sender, ObjectChangeEventArgs e)
    {
        if (e.PropertyName == "MediaData" && !IsSaving)
        {
            UpdateThumbnail(Foto);
        }
    }

    [XafDisplayName("Miniatura")]
    [ImageEditor(DetailViewImageEditorMode = ImageEditorMode.PictureEdit, ListViewImageEditorMode = ImageEditorMode.PictureEdit)]
    public MediaDataObject? Miniatura
    {
        get => _miniatura;
        set => SetPropertyValue(nameof(Miniatura), ref _miniatura, value);
    }

    private void UpdateThumbnail(MediaDataObject? sourceFoto)
    {
        if (sourceFoto?.MediaData != null)
        {
            Miniatura ??= new MediaDataObject(Session);
            Miniatura.MediaData = erp.Module.Helpers.ImageHelper.GetThumbnailBytes(sourceFoto.MediaData);
            OnChanged(nameof(Miniatura));
        }
        else
        {
            Miniatura = null;
        }
    }

    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("Productos-ImpuestosVentas")]
    [DataSourceCriteria("DisponibleEnVentas = True AND EstaActivo = True")]
    [XafDisplayName("Impuestos Ventas")]
    public XPCollection<TipoImpuesto> ImpuestosVentas => GetCollection<TipoImpuesto>();

    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("Productos-ImpuestosCompras")]
    [DataSourceCriteria("DisponibleEnCompras = True AND EstaActivo = True")]
    [XafDisplayName("Impuestos Compras")]
    public XPCollection<TipoImpuesto> ImpuestosCompras => GetCollection<TipoImpuesto>();

    [DevExpress.Xpo.Aggregated]
    [Association("Producto-Tareas")]
    [XafDisplayName("Tareas")]
    public XPCollection<Tarea> Tareas => GetCollection<Tarea>();

    [DevExpress.Xpo.Aggregated]
    [Association("Producto-Fotos")]
    [XafDisplayName("Imágenes")]
    public XPCollection<Imagen> Imagenes => GetCollection<Imagen>();

    [DevExpress.Xpo.Aggregated]
    [Association("Producto-Adjuntos")]
    [XafDisplayName("Adjuntos")]
    public XPCollection<Adjunto> Adjuntos => GetCollection<Adjunto>();

    [DevExpress.Xpo.Aggregated]
    [Association("Producto-PreciosPorCantidad")]
    [XafDisplayName("Precios por Cantidad")]
    public XPCollection<PrecioPorCantidad> PreciosPorCantidad => GetCollection<PrecioPorCantidad>();

    [DevExpress.Xpo.Aggregated]
    [Association("Producto-PreciosEspeciales")]
    [XafDisplayName("Precios Especiales (Clientes/Proveedores)")]
    public XPCollection<PrecioEspecial> PreciosEspeciales => GetCollection<PrecioEspecial>();

    [Association("Producto-StockActual")]
    [XafDisplayName("Stock Actual")]
    public XPCollection<StockActual> StockActual => GetCollection<StockActual>();

    [DevExpress.Xpo.Aggregated]
    [Association("Producto-Componentes")]
    [XafDisplayName("Componentes")]
    public XPCollection<ProductoCompuestoItem> Componentes => GetCollection<ProductoCompuestoItem>();

    [Association("Componente-ProductosPadres")]
    [XafDisplayName("Donde es Componente")]
    public XPCollection<ProductoCompuestoItem> DondeEsComponente => GetCollection<ProductoCompuestoItem>();

    [Action(Caption = "Recalcular Precios desde Componentes",
        ConfirmationMessage = "¿Desea recalcular el coste y precio de venta a partir de sus componentes?",
        ToolTip = "Suma el coste y precio de venta de todos los componentes multiplicados por su cantidad.",
        ImageName = "Action_ResetViewSettings",
        TargetObjectsCriteria = "EsCompuesto = True",
        SelectionDependencyType = MethodActionSelectionDependencyType.RequireSingleObject)]
    public void RecalcularPreciosDesdeComponentes()
    {
        RecalcularPreciosDesdeComponentesInternal(new HashSet<Guid>());
    }

    private void RecalcularPreciosDesdeComponentesInternal(HashSet<Guid> procesados)
    {
        if (!EsCompuesto) return;
        if (procesados.Contains(Oid)) return; // Evitar ciclos infinitos
        procesados.Add(Oid);

        decimal nuevoCoste = 0;
        decimal nuevoPrecioVenta = 0;

        foreach (var item in Componentes)
        {
            if (item.Componente == null) continue;

            // Si el componente es compuesto, recalcularlo primero
            if (item.Componente.EsCompuesto)
            {
                item.Componente.RecalcularPreciosDesdeComponentesInternal(procesados);
            }

            nuevoCoste += item.Componente.CosteEstandar * item.Cantidad;
            nuevoPrecioVenta += item.Componente.PrecioVenta * item.Cantidad;
        }

        CosteEstandar = nuevoCoste;
        PrecioVenta = nuevoPrecioVenta;
    }

    [Action(Caption = "Restablecer Código de Barras", 
        ConfirmationMessage = "¿Desea restablecer el código de barras al valor original?", 
        ToolTip = "Restablece el valor del código de barras al Oid del producto",
        ImageName = "Action_ResetViewSettings",
        SelectionDependencyType = MethodActionSelectionDependencyType.RequireSingleObject)]
    public void ResetCodigoBarras()
    {
        CodigoBarras = GuidHelper.GetShortHash(Oid);
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();
        if (_foto != null)
        {
            _foto.Changed += Foto_Changed;
        }
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (IsSaving) return;
        if (propertyName == nameof(Foto))
        {
            UpdateThumbnail(Foto);
        }

        if (propertyName is nameof(CosteEstandar) or nameof(PrecioVenta))
        {
            if (!IsLoading && !IsSaving)
            {
                foreach (var item in DondeEsComponente)
                {
                    item.ProductoPadre?.RecalcularPreciosDesdeComponentes();
                }
            }
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        EstaActivo = true;
        DisponibleEnVentas = true;
        DisponibleEnCompras = true;
        DisponibleEnTpv = false;
        DisponibleEnWeb = false;
        GestionaStock = false;
        PermiteVentaSinStock = false;
        RequiereReservaStock = false;
        EsServicio = false;
        EsConsumible = false;
        EsCompuesto = false;
        CodigoBarras = GuidHelper.GetShortHash(Oid);
        Codigo = CodigoBarras;

        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;

        CuentaVentas ??= companyInfo.CuentaVentasPorDefecto;
        CuentaCompras ??= companyInfo.CuentaComprasPorDefecto;
        UnidadFacturacion ??= companyInfo.UnidadFacturacionPredeterminada;

        foreach (var tax in companyInfo.ImpuestosVentas)
        {
            ImpuestosVentas.Add(tax);
        }

        foreach (var tax in companyInfo.ImpuestosCompras)
        {
            ImpuestosCompras.Add(tax);
        }
    }
}