using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.Helpers.Contactos;
using System.ComponentModel;

namespace erp.Module.BusinessObjects.Tpv;

public enum VentaTpvEstado
{
    Borrador,
    EnCurso,
    PendienteCobro,
    Finalizada,
    Cancelada
}

[DefaultClassOptions]
[NavigationItem("Tpv")]
[XafDisplayName("Venta TPV")]
[Persistent("VentaTpv")]
[DefaultProperty(nameof(Numero))]
public class VentaTpv(Session session) : EntidadBase(session)
{
    private DateTime _fecha;
    private string? _numero;
    private VentaTpvEstado _estado;
    private SesionTpv? _sesionTpv;
    private Tercero? _cliente;
    private decimal _totalBruto;
    private decimal _totalDescuentos;
    private decimal _totalImpuestos;
    private decimal _totalFinal;
    private decimal _totalPagado;
    private string? _notas;

    [XafDisplayName("Fecha")]
    public DateTime Fecha
    {
        get => _fecha;
        set => SetPropertyValue(nameof(Fecha), ref _fecha, value);
    }

    [XafDisplayName("Número")]
    [Size(50)]
    public string? Numero
    {
        get => _numero;
        set => SetPropertyValue(nameof(Numero), ref _numero, value);
    }

    [XafDisplayName("Estado")]
    public VentaTpvEstado Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [XafDisplayName("Sesión TPV")]
    [Association("SesionTpv-VentasTpv")]
    [RuleRequiredField]
    public SesionTpv? SesionTpv
    {
        get => _sesionTpv;
        set => SetPropertyValue(nameof(SesionTpv), ref _sesionTpv, value);
    }

    [XafDisplayName("Cliente")]
    public Tercero? Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [XafDisplayName("Total Bruto")]
    public decimal TotalBruto
    {
        get => _totalBruto;
        set => SetPropertyValue(nameof(TotalBruto), ref _totalBruto, value);
    }

    [XafDisplayName("Total Descuentos")]
    public decimal TotalDescuentos
    {
        get => _totalDescuentos;
        set => SetPropertyValue(nameof(TotalDescuentos), ref _totalDescuentos, value);
    }

    [XafDisplayName("Total Impuestos")]
    public decimal TotalImpuestos
    {
        get => _totalImpuestos;
        set => SetPropertyValue(nameof(TotalImpuestos), ref _totalImpuestos, value);
    }

    [XafDisplayName("Total Final")]
    public decimal TotalFinal
    {
        get => _totalFinal;
        set => SetPropertyValue(nameof(TotalFinal), ref _totalFinal, value);
    }

    [XafDisplayName("Total Pagado")]
    public decimal TotalPagado
    {
        get => _totalPagado;
        set => SetPropertyValue(nameof(TotalPagado), ref _totalPagado, value);
    }

    [XafDisplayName("Cambio a Devolver")]
    public decimal CambioADevolver => TotalPagado > TotalFinal ? TotalPagado - TotalFinal : 0;

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [Association("VentaTpv-Lineas")]
    [XafDisplayName("Líneas")]
    public XPCollection<LineaVentaTpv> Lineas => GetCollection<LineaVentaTpv>();

    [Association("VentaTpv-Pagos")]
    [XafDisplayName("Pagos")]
    public XPCollection<PagoVentaTpv> Pagos => GetCollection<PagoVentaTpv>();

    [Association("VentaTpv-Eventos")]
    [XafDisplayName("Eventos")]
    public XPCollection<VentaTpvEvento> Eventos => GetCollection<VentaTpvEvento>();

    [Association("VentaTpv-Descuentos")]
    [XafDisplayName("Descuentos")]
    public XPCollection<DescuentoVentaTpv> Descuentos => GetCollection<DescuentoVentaTpv>();

    [XafDisplayName("Factura Simplificada")]
    public FacturaSimplificada? FacturaSimplificada
    {
        get => Session.FindObject<FacturaSimplificada>(new DevExpress.Data.Filtering.BinaryOperator(nameof(FacturaSimplificada.VentaTpv), this));
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Fecha = InformacionEmpresaHelper.GetLocalTime(Session);
        Estado = VentaTpvEstado.Borrador;
    }

    public void RecalcularTotales()
    {
        TotalBruto = 0;
        TotalDescuentos = 0;
        TotalImpuestos = 0;
        
        foreach (var linea in Lineas)
        {
            TotalBruto += linea.BaseImponible;
            TotalDescuentos += linea.DescuentoImporte;
            TotalImpuestos += linea.ImpuestoImporte;
        }

        TotalFinal = TotalBruto + TotalImpuestos - TotalDescuentos;
    }

    public void ActualizarTotalPagado()
    {
        decimal pagado = 0;
        foreach (var pago in Pagos)
        {
            pagado += pago.Importe;
        }
        TotalPagado = pagado;
    }
}
