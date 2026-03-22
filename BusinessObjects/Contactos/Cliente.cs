using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Alquileres;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Customer")]
public class Cliente(Session session) : Tercero(session), IPuedeParticiparEnVentas
{
    private Banco? _bancoPredeterminado;
    private CondicionPago? _condicionPago;
    private Cuenta? _cuentaCobro;
    private Diario? _diarioVentas;
    private PosicionFiscal? _posicionFiscal;
    private Sector? _sector;


    [XafDisplayName("Cuenta de Cobro")]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Cuenta? CuentaCobro
    {
        get => _cuentaCobro;
        set => SetPropertyValue(nameof(CuentaCobro), ref _cuentaCobro, value);
    }

    [XafDisplayName("Diario de Ventas")]
    [DataSourceCriteria("EstaActivo = True")]
    public Diario? DiarioVentas
    {
        get => _diarioVentas;
        set => SetPropertyValue(nameof(DiarioVentas), ref _diarioVentas, value);
    }

    [XafDisplayName("Condiciones de Pago")]
    public CondicionPago? CondicionPago
    {
        get => _condicionPago;
        set => SetPropertyValue(nameof(CondicionPago), ref _condicionPago, value);
    }

    [XafDisplayName("Banco Predeterminado")]
    [DataSourceProperty(nameof(Bancos))]
    public Banco? BancoPredeterminado
    {
        get => _bancoPredeterminado;
        set => SetPropertyValue(nameof(BancoPredeterminado), ref _bancoPredeterminado, value);
    }

    [XafDisplayName("Posición Fiscal")]
    public PosicionFiscal? PosicionFiscal
    {
        get => _posicionFiscal;
        set => SetPropertyValue(nameof(PosicionFiscal), ref _posicionFiscal, value);
    }

    [XafDisplayName("Sector")]
    public Sector? Sector
    {
        get => _sector;
        set => SetPropertyValue(nameof(Sector), ref _sector, value);
    }

    [Association("Cliente-Bancos")]
    [XafDisplayName("Bancos")]
    public XPCollection<Banco> Bancos => GetCollection<Banco>();

    [Association("Cliente-Contactos")]
    [XafDisplayName("Contactos")]
    public XPCollection<Contacto> Contactos => GetCollection<Contacto>();

    [XafDisplayName("Viajeros")]
    public XPCollection<Viajero> Viajeros => new(Session, CriteriaOperator.Parse("Cliente = ?", this));

    [Association("Cliente-Domicilios")]
    [XafDisplayName("Domicilios")]
    public XPCollection<Domicilio> Domicilios => GetCollection<Domicilio>();

    [Association("Cliente-Oportunidades")]
    [XafDisplayName("Oportunidades")]
    public XPCollection<Oportunidad> Oportunidades => GetCollection<Oportunidad>();

    [XafDisplayName("Ofertas")]
    public XPCollection<OfertaVenta> Ofertas => new(Session, CriteriaOperator.Parse("Cliente.Oid = ?", Oid));

    [XafDisplayName("Pedidos")]
    public XPCollection<Pedido> Pedidos => new(Session, CriteriaOperator.Parse("Cliente.Oid = ?", Oid));

    [XafDisplayName("Albaranes")]
    public XPCollection<Albaran> Albaranes => new(Session, CriteriaOperator.Parse("Cliente.Oid = ?", Oid));

    [XafDisplayName("Facturas")]
    public XPCollection<Factura> Facturas => new(Session, CriteriaOperator.Parse("Cliente.Oid = ?", Oid));

    [Association("Cliente-Suscripciones")]
    [XafDisplayName("Suscripciones")]
    public XPCollection<Suscripciones.Suscripcion> Suscripciones => GetCollection<Suscripciones.Suscripcion>();

    [Association("Cliente-Reservas")]
    [XafDisplayName("Reservas")]
    public XPCollection<Reserva> Reservas => GetCollection<Reserva>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        // InitValues se llama en el base.AfterConstruction()
    }

    public override string GetPrefijoCodigo()
    {
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        return companyInfo?.PrefijoClientes ?? "C";
    }

    protected override void InitValues()
    {
        base.InitValues();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;

        _cuentaCobro = companyInfo.CuentaCobrosPorDefecto;
        _diarioVentas = companyInfo.DiarioVentasPorDefecto;
        _posicionFiscal = companyInfo.PosicionFiscalPorDefecto;
        _condicionPago = companyInfo.CondicionPagoPorDefecto;
    }
}