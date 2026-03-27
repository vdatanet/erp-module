using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Alquileres;
using erp.Module.BusinessObjects.Auxiliares;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Impuestos;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[XafDisplayName("Cliente")]
[ImageName("BO_Customer")]
public class Cliente(Session session) : Tercero(session), IPuedeParticiparEnVentas
{
    private Banco? _bancoPredeterminado;
    private CondicionPago? _condicionPago;
    private CuentaContable? _cuentaCobro;
    private Diario? _diarioVentas;
    private PosicionFiscal? _posicionFiscal;
    private Sector? _sector;


    [XafDisplayName("Cuenta Contable de Cobro")]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public CuentaContable? CuentaCobro
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

    [XafDisplayName("Banco Predeterminado")]
    [DataSourceProperty(nameof(Bancos))]
    public Banco? BancoPredeterminado
    {
        get => _bancoPredeterminado;
        set => SetPropertyValue(nameof(BancoPredeterminado), ref _bancoPredeterminado, value);
    }

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
    public XPCollection<PedidoVenta> Pedidos => new(Session, CriteriaOperator.Parse("Cliente.Oid = ?", Oid));

    [XafDisplayName("Albaranes")]
    public XPCollection<AlbaranVenta> Albaranes => new(Session, CriteriaOperator.Parse("Cliente.Oid = ?", Oid));

    [XafDisplayName("Facturas")]
    public XPCollection<FacturaVenta> Facturas => new(Session, CriteriaOperator.Parse("Cliente.Oid = ?", Oid));

    [Association("Cliente-DocumentosVenta")]
    [XafDisplayName("Documentos de Venta")]
    public XPCollection<DocumentoVenta> DocumentosVenta => GetCollection<DocumentoVenta>();

    [Association("Cliente-Suscripciones")]
    [XafDisplayName("Suscripciones")]
    public XPCollection<Suscripciones.Suscripcion> Suscripciones => GetCollection<Suscripciones.Suscripcion>();

    [Association("Cliente-SolicitudesTC")]
    [XafDisplayName("Solicitudes de Trabajo de Campo")]
    public XPCollection<Servicios.TrabajoDeCampo.SolicitudTrabajoDeCampo> SolicitudesTC => GetCollection<Servicios.TrabajoDeCampo.SolicitudTrabajoDeCampo>();

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
        if (companyInfo == null) return "C";
        return companyInfo.PrefijoClientes ?? "C";
    }

    public static int ImportarDesdeCsv(Session session, string csvContent)
    {
        if (string.IsNullOrEmpty(csvContent)) return 0;

        var lines = csvContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1) return 0; // Encabezado o vacío

        int importedCount = 0;

        // Formato: Nombre;NIF;Email;Telefono;Direccion
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            var values = line.Split(';');
            if (values.Length < 2) continue;

            var nombre = values[0].Trim();
            var nif = values[1].Trim();
            var email = values.Length > 2 ? values[2].Trim() : string.Empty;
            var telefono = values.Length > 3 ? values[3].Trim() : string.Empty;
            var direccion = values.Length > 4 ? values[4].Trim() : string.Empty;

            if (string.IsNullOrEmpty(nombre)) continue;

            // Buscar si ya existe por NIF (si tiene)
            Cliente? cliente = null;
            if (!string.IsNullOrEmpty(nif))
            {
                cliente = session.FindObject<Cliente>(CriteriaOperator.Parse("Nif = ?", nif));
            }

            if (cliente == null)
            {
                cliente = new Cliente(session);
                cliente.Nombre = nombre;
                cliente.Nif = nif;
            }

            if (!string.IsNullOrEmpty(email)) cliente.CorreoElectronico = email;
            if (!string.IsNullOrEmpty(telefono)) cliente.Telefono = telefono;
            if (!string.IsNullOrEmpty(direccion)) cliente.Direccion = direccion;

            importedCount++;
        }

        return importedCount;
    }

    protected override void InitValues()
    {
        base.InitValues();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;

        _cuentaCobro ??= companyInfo.CuentaCobrosPorDefecto ?? CuentaContable;
        _diarioVentas ??= companyInfo.DiarioVentasPorDefecto;
        _posicionFiscal ??= companyInfo.PosicionFiscalPorDefecto;
        _condicionPago ??= companyInfo.CondicionPagoPorDefecto;
    }
}