using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Servicios.TrabajoDeCampo;
using erp.Module.BusinessObjects.Suscripciones;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Servicios.PartesTrabajo;

[DefaultClassOptions]
[NavigationItem("Servicios")]
[XafDisplayName("Partes de Trabajo")]
public class Parte(Session session) : DocumentoVenta(session)
{
    private bool _esFacturable;
    private double _horasTotales;
    private decimal _importeManoObra;
    private decimal _importeMateriales;
    private Suscripcion? _suscripcionCubridora;
    private CoberturaSuscripcion? _coberturaAplicada;

    [XafDisplayName("Suscripción Cubridora")]
    public Suscripcion? SuscripcionCubridora
    {
        get => _suscripcionCubridora;
        set => SetPropertyValue(nameof(SuscripcionCubridora), ref _suscripcionCubridora, value);
    }

    [XafDisplayName("Cobertura Aplicada")]
    public CoberturaSuscripcion? CoberturaAplicada
    {
        get => _coberturaAplicada;
        set => SetPropertyValue(nameof(CoberturaAplicada), ref _coberturaAplicada, value);
    }

    [XafDisplayName("¿Es Facturable?")]
    public bool EsFacturable
    {
        get => _esFacturable;
        set => SetPropertyValue(nameof(EsFacturable), ref _esFacturable, value);
    }

    [XafDisplayName("Horas Totales")]
    public double HorasTotales
    {
        get => _horasTotales;
        set => SetPropertyValue(nameof(HorasTotales), ref _horasTotales, value);
    }

    [XafDisplayName("Importe Mano de Obra")]
    public decimal ImporteManoObra
    {
        get => _importeManoObra;
        set => SetPropertyValue(nameof(ImporteManoObra), ref _importeManoObra, value);
    }

    [XafDisplayName("Importe Materiales")]
    public decimal ImporteMateriales
    {
        get => _importeMateriales;
        set => SetPropertyValue(nameof(ImporteMateriales), ref _importeMateriales, value);
    }

    [Association("Parte-Tiempos")]
    [XafDisplayName("Tiempos")]
    public XPCollection<ParteTrabajoTiempo> Tiempos => GetCollection<ParteTrabajoTiempo>(nameof(Tiempos));

    [Association("Parte-Materiales")]
    [XafDisplayName("Materiales")]
    public XPCollection<ParteTrabajoMaterial> Materiales => GetCollection<ParteTrabajoMaterial>(nameof(Materiales));

    private ServicioTrabajoDeCampo? _servicioTC;
    [XafDisplayName("Servicio TC")]
    public ServicioTrabajoDeCampo? ServicioTC
    {
        get => _servicioTC;
        set => SetPropertyValue(nameof(ServicioTC), ref _servicioTC, value);
    }

    public void BuscarYAplicarCobertura()
    {
        if (Cliente == null || ServicioTC?.PedidoTC?.Solicitud?.TipoServicio == null) return;

        var tipoServicio = ServicioTC.PedidoTC.Solicitud.TipoServicio;
        var hoy = DateTime.Today;

        // Buscar suscripciones activas del cliente que tengan una cobertura para este tipo de servicio
        var criteria = CriteriaOperator.Parse("Suscripcion.Cliente = ? AND Suscripcion.Estado = ? AND TipoServicio = ? AND FechaDesde <= ? AND (FechaHasta IS NULL OR FechaHasta >= ?)",
            Cliente, EstadoSuscripcion.Activa, tipoServicio, hoy, hoy);

        var cobertura = Session.FindObject<CoberturaSuscripcion>(criteria);

        if (cobertura != null)
        {
            SuscripcionCubridora = cobertura.Suscripcion;
            CoberturaAplicada = cobertura;
            
            // Si la cobertura es total, el parte no es facturable (o se marca como tal para revisión)
            if (cobertura.TipoCobertura == TipoCobertura.Total)
            {
                EsFacturable = false;
            }
            // Otros tipos de cobertura (Horas, Visitas) requerirán lógica adicional al registrar consumo
        }
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        
        if (SuscripcionCubridora == null)
        {
            BuscarYAplicarCobertura();
        }

        if (!IsDeleted && CoberturaAplicada != null)
        {
            RegistrarConsumo();
        }
    }

    private void RegistrarConsumo()
    {
        if (CoberturaAplicada == null) return;

        // Evitar duplicados si ya existe un consumo para este parte
        var consumoExistente = Session.FindObject<ConsumoSuscripcion>(CriteriaOperator.Parse("ParteTrabajo = ?", this));
        if (consumoExistente != null) return;

        var consumo = new ConsumoSuscripcion(Session)
        {
            Cobertura = CoberturaAplicada,
            ParteTrabajo = this,
            Fecha = DateTime.Today,
            CantidadHoras = (decimal)HorasTotales,
            CantidadVisitas = 1
        };

        // Actualizar acumulados en la cobertura
        CoberturaAplicada.ConsumoAcumuladoHoras += (decimal)HorasTotales;
        CoberturaAplicada.ConsumoAcumuladoVisitas += 1;
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        Serie ??= companyInfo?.PrefijoParteTrabajoPorDefecto;
        EsFacturable = true;
    }
}