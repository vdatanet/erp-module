using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Facturacion;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.Helpers.Contactos;
using erp.Module.BusinessObjects.Configuracion;

using erp.Module.Factories;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Customer")]
public class Cliente(Session session) : Tercero(session)
{
    private decimal _descuentoComercial;
    private decimal _limiteCredito;
    private bool _activo;
    private DateTime _fechaAlta;
    private DateTime? _fechaBaja;
    private CondicionesPago _condicionesPago;
    private Banco _bancoPredeterminado;
    private Cuenta _cuentaContable;
    private Cuenta _cuentaCobro;
    private Diario _diarioVentas;
    private PosicionFiscal _posicionFiscal;

    [XafDisplayName("Activo")]
    public bool Activo
    {
        get => _activo;
        set
        {
            if (!SetPropertyValue(nameof(Activo), ref _activo, value)) return;
            if (IsLoading || IsSaving) return;
            if (value)
            {
                FechaBaja = null;
            }
            else
            {
                FechaBaja = DateTime.Now;
            }
        }
    }

    [XafDisplayName("Fecha de Alta")]
    [ModelDefault("AllowEdit", "False")]
    public DateTime FechaAlta
    {
        get => _fechaAlta;
        set => SetPropertyValue(nameof(FechaAlta), ref _fechaAlta, value);
    }

    [XafDisplayName("Fecha de Baja")]
    [ModelDefault("AllowEdit", "False")]
    public DateTime? FechaBaja
    {
        get => _fechaBaja;
        set => SetPropertyValue(nameof(FechaBaja), ref _fechaBaja, value);
    }

    [XafDisplayName("Límite de Crédito")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal LimiteCredito
    {
        get => _limiteCredito;
        set => SetPropertyValue(nameof(LimiteCredito), ref _limiteCredito, value);
    }

    [XafDisplayName("Descuento Comercial (%)")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal DescuentoComercial
    {
        get => _descuentoComercial;
        set => SetPropertyValue(nameof(DescuentoComercial), ref _descuentoComercial, value);
    }

    [XafDisplayName("Cuenta Contable")]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Cuenta CuentaContable
    {
        get => _cuentaContable;
        set => SetPropertyValue(nameof(CuentaContable), ref _cuentaContable, value);
    }

    [XafDisplayName("Cuenta de Cobro")]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Cuenta CuentaCobro
    {
        get => _cuentaCobro;
        set => SetPropertyValue(nameof(CuentaCobro), ref _cuentaCobro, value);
    }

    [XafDisplayName("Diario de Ventas")]
    [DataSourceCriteria("EstaActivo = True")]
    public Diario DiarioVentas
    {
        get => _diarioVentas;
        set => SetPropertyValue(nameof(DiarioVentas), ref _diarioVentas, value);
    }

    [XafDisplayName("Condiciones de Pago")]
    public CondicionesPago CondicionesPago
    {
        get => _condicionesPago;
        set => SetPropertyValue(nameof(CondicionesPago), ref _condicionesPago, value);
    }
    
    [XafDisplayName("Banco Predeterminado")]
    [DataSourceProperty(nameof(Bancos))]
    public Banco BancoPredeterminado
    {
        get => _bancoPredeterminado;
        set => SetPropertyValue(nameof(BancoPredeterminado), ref _bancoPredeterminado, value);
    }

    [XafDisplayName("Posición Fiscal")]
    public PosicionFiscal PosicionFiscal
    {
        get => _posicionFiscal;
        set => SetPropertyValue(nameof(PosicionFiscal), ref _posicionFiscal, value);
    }

    [Association("Cliente-Bancos")]
    [XafDisplayName("Bancos")]
    public XPCollection<Banco> Bancos => GetCollection<Banco>(nameof(Bancos));
    
    [Association("Cliente-Contactos")]
    [XafDisplayName("Contactos")]
    public XPCollection<Contacto> Contactos => GetCollection<Contacto>();

    [Association("Cliente-Domicilios")]
    [XafDisplayName("Domicilios")]
    public XPCollection<Domicilio> Domicilios => GetCollection<Domicilio>(nameof(Domicilios));

    [Association("Cliente-Oportunidades")]
    [XafDisplayName("Oportunidades")]
    public XPCollection<Oportunidad> Oportunidades => GetCollection<Oportunidad>();

    [Association("Cliente-DocumentosVenta")]
    [XafDisplayName("Documentos de Venta")]
    [VisibleInDetailView(false)]
    public XPCollection<DocumentoVenta> DocumentosVenta => GetCollection<DocumentoVenta>();
    
    [XafDisplayName("Presupuestos")] public XPCollection<Presupuesto> Presupuestos => GetCollection<Presupuesto>();

    [XafDisplayName("Pedidos")] public XPCollection<Pedido> Pedidos => GetCollection<Pedido>();

    [XafDisplayName("Facturas")] public XPCollection<Factura> Facturas => GetCollection<Factura>();
    
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    public override string GetPrefijoCodigo()
    {
        return "C";
    }

    private void InitValues()
    {
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        _cuentaContable = companyInfo.CuentaClientesPorDefecto;
        _cuentaCobro = companyInfo.CuentaCobrosPorDefecto;
        _diarioVentas = companyInfo.DiarioVentasPorDefecto;
        _posicionFiscal = companyInfo.PosicionFiscalPorDefecto;
        _condicionesPago = companyInfo.CondicionesPagoPorDefecto;
        _activo = true;
        _fechaAlta = DateTime.Now;
    }
}