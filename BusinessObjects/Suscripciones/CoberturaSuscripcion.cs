using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Servicios.TrabajoDeCampo;

namespace erp.Module.BusinessObjects.Suscripciones;

[DefaultClassOptions]
[NavigationItem("Suscripciones")]
[XafDisplayName("Cobertura de Suscripción")]
public class CoberturaSuscripcion(Session session) : EntidadBase(session)
{
    private Suscripcion? _suscripcion;
    private TipoServicioTrabajoDeCampo? _tipoServicio;
    private TipoCobertura _tipoCobertura;
    private int _limiteVisitas;
    private decimal _limiteHoras;
    private decimal _consumoAcumuladoHoras;
    private int _consumoAcumuladoVisitas;
    private DateTime _fechaDesde;
    private DateTime? _fechaHasta;

    [Association("Suscripcion-Coberturas")]
    public Suscripcion? Suscripcion
    {
        get => _suscripcion;
        set => SetPropertyValue(nameof(Suscripcion), ref _suscripcion, value);
    }

    [XafDisplayName("Servicio Cubierto")]
    public TipoServicioTrabajoDeCampo? TipoServicio
    {
        get => _tipoServicio;
        set => SetPropertyValue(nameof(TipoServicio), ref _tipoServicio, value);
    }

    [XafDisplayName("Tipo de Cobertura")]
    public TipoCobertura TipoCobertura
    {
        get => _tipoCobertura;
        set => SetPropertyValue(nameof(TipoCobertura), ref _tipoCobertura, value);
    }

    [XafDisplayName("Límite Visitas")]
    public int LimiteVisitas
    {
        get => _limiteVisitas;
        set => SetPropertyValue(nameof(LimiteVisitas), ref _limiteVisitas, value);
    }

    [XafDisplayName("Límite Horas")]
    public decimal LimiteHoras
    {
        get => _limiteHoras;
        set => SetPropertyValue(nameof(LimiteHoras), ref _limiteHoras, value);
    }

    [XafDisplayName("Consumo Horas")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ConsumoAcumuladoHoras
    {
        get => _consumoAcumuladoHoras;
        set => SetPropertyValue(nameof(ConsumoAcumuladoHoras), ref _consumoAcumuladoHoras, value);
    }

    [XafDisplayName("Consumo Visitas")]
    [ModelDefault("AllowEdit", "False")]
    public int ConsumoAcumuladoVisitas
    {
        get => _consumoAcumuladoVisitas;
        set => SetPropertyValue(nameof(ConsumoAcumuladoVisitas), ref _consumoAcumuladoVisitas, value);
    }

    [XafDisplayName("Válido Desde")]
    public DateTime FechaDesde
    {
        get => _fechaDesde;
        set => SetPropertyValue(nameof(FechaDesde), ref _fechaDesde, value);
    }

    [XafDisplayName("Válido Hasta")]
    public DateTime? FechaHasta
    {
        get => _fechaHasta;
        set => SetPropertyValue(nameof(FechaHasta), ref _fechaHasta, value);
    }

    [Association("Cobertura-Consumos")]
    public XPCollection<ConsumoSuscripcion> Consumos => GetCollection<ConsumoSuscripcion>(nameof(Consumos));

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaDesde = DateTime.Today;
        TipoCobertura = TipoCobertura.Total;
    }
}
