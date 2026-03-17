using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Facturacion;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.Helpers.Contactos;
using erp.Module.BusinessObjects.Configuraciones;

using erp.Module.Factories;

using erp.Module.BusinessObjects.Alquileres;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[ImageName("BO_Customer")]
public class Cliente(Session session) : Tercero(session)
{
    private bool _activo;
    private DateTime _fechaAlta;
    private DateTime? _fechaBaja;
    private CondicionPago? _condicionPago;
    private Banco? _bancoPredeterminado;
    private Cuenta? _cuentaContable;
    private Cuenta? _cuentaCobro;
    private Diario? _diarioVentas;
    private PosicionFiscal? _posicionFiscal;
    private Sector? _sector;

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
    
    [XafDisplayName("Cuenta Contable")]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Cuenta? CuentaContable
    {
        get => _cuentaContable;
        set => SetPropertyValue(nameof(CuentaContable), ref _cuentaContable, value);
    }

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
    public XPCollection<Banco> Bancos => GetCollection<Banco>(nameof(Bancos));
    
    [Association("Cliente-Contactos")]
    [XafDisplayName("Contactos")]
    public XPCollection<Contacto> Contactos => GetCollection<Contacto>();

    [XafDisplayName("Viajeros")]
    public XPCollection<Viajero> Viajeros => new(Session, CriteriaOperator.Parse("Cliente = ?", this));

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
    
    [XafDisplayName("Presupuestos")]
    public XPCollection<Presupuesto> Presupuestos => new(Session, CriteriaOperator.Parse("Cliente = ?", this));

    [XafDisplayName("Pedidos")]
    public XPCollection<Pedido> Pedidos => new(Session, CriteriaOperator.Parse("Cliente = ?", this));

    [XafDisplayName("Facturas")]
    public XPCollection<Factura> Facturas => new(Session, CriteriaOperator.Parse("Cliente = ?", this));

    [Association("Cliente-Reservas")]
    [XafDisplayName("Reservas")]
    public XPCollection<Reserva> Reservas => GetCollection<Reserva>();

    public enum Sexes
    {
        [XafDisplayName("Masculino")]
        Masculino,
        [XafDisplayName("Femenino")]
        Femenino
    }

    
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
        _condicionPago = companyInfo.CondicionPagoPorDefecto;
        _activo = true;
        _fechaAlta = DateTime.Now;
    }
}