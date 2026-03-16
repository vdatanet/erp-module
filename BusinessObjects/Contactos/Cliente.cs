using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Crm;
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
    private CondicionesPago _condicionesPago;
    private Banco _bancoPredeterminado;
    private Cuenta _cuentaContable;
    private Diario _diarioVentas;

    [XafDisplayName("Cuenta Contable")]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Cuenta CuentaContable
    {
        get => _cuentaContable;
        set => SetPropertyValue(nameof(CuentaContable), ref _cuentaContable, value);
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
    public XPCollection<DocumentoVenta> DocumentosVenta => GetCollection<DocumentoVenta>();
    
    [XafDisplayName("Presupuestos")] public XPCollection<Presupuesto> Presupuestos => GetCollection<Presupuesto>();

    [XafDisplayName("Pedidos")] public XPCollection<Pedido> Pedidos => GetCollection<Pedido>();
    
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
        _diarioVentas = companyInfo.DiarioVentasPorDefecto;
        _condicionesPago = companyInfo.CondicionesPagoPorDefecto;
    }
}