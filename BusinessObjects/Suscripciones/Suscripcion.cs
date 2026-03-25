using System.ComponentModel;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Productos;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Suscripciones;

[DefaultClassOptions]
[NavigationItem("Suscripciones")]
[ImageName("BO_Contract")]
[DefaultProperty(nameof(NombreDisplay))]
public class Suscripcion(Session session) : EntidadBase(session)
{
    private TipoSuscripcion? _tipoSuscripcion;
    private Cliente? _cliente;
    private Producto? _producto;
    private string? _nombrePersonalizado;
    private Guid _entidadRelacionadaId;
    private string? _entidadRelacionadaTipo;
    private DateTime _fechaInicio;
    private DateTime? _fechaFin;
    private decimal _importeFinal;
    private EstadoSuscripcion _estado;
    private DateTime? _ultimaFechaCobro;
    private DateTime? _proximaFechaCobro;
    private string? _observaciones;
    private PedidoVenta? _pedidoVigente;

    [RuleRequiredField("RuleRequiredField_Suscripcion_Cliente", DefaultContexts.Save, CustomMessageTemplate = "El Cliente de la Suscripción es obligatorio")]
    [Association("Cliente-Suscripciones")]
    [XafDisplayName("Cliente")]
    public Cliente? Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [XafDisplayName("Producto / Concepto")]
    public Producto? Producto
    {
        get => _producto;
        set => SetPropertyValue(nameof(Producto), ref _producto, value);
    }

    [RuleRequiredField("RuleRequiredField_Suscripcion_TipoSuscripcion", DefaultContexts.Save, CustomMessageTemplate = "El Tipo de Suscripción es obligatorio")]
    [Association("TipoSuscripcion-Suscripciones")]
    [XafDisplayName("Tipo de Suscripción")]
    public TipoSuscripcion? TipoSuscripcion
    {
        get => _tipoSuscripcion;
        set
        {
            if (SetPropertyValue(nameof(TipoSuscripcion), ref _tipoSuscripcion, value))
            {
                if (!IsLoading && value != null)
                {
                    ImporteFinal = value.ImporteBase;
                }
            }
        }
    }

    [XafDisplayName("Nombre Personalizado")]
    public string? NombrePersonalizado
    {
        get => _nombrePersonalizado;
        set => SetPropertyValue(nameof(NombrePersonalizado), ref _nombrePersonalizado, value);
    }

    [Browsable(false)]
    public string NombreDisplay => string.IsNullOrEmpty(NombrePersonalizado) 
        ? (TipoSuscripcion?.Nombre ?? "Nueva Suscripción") 
        : NombrePersonalizado;

    [XafDisplayName("ID Entidad Relacionada")]
    public Guid EntidadRelacionadaId
    {
        get => _entidadRelacionadaId;
        set => SetPropertyValue(nameof(EntidadRelacionadaId), ref _entidadRelacionadaId, value);
    }

    [XafDisplayName("Tipo Entidad Relacionada")]
    public string? EntidadRelacionadaTipo
    {
        get => _entidadRelacionadaTipo;
        set => SetPropertyValue(nameof(EntidadRelacionadaTipo), ref _entidadRelacionadaTipo, value);
    }

    [RuleRequiredField("RuleRequiredField_Suscripcion_FechaInicio", DefaultContexts.Save, CustomMessageTemplate = "La Fecha de Inicio de la Suscripción es obligatoria")]
    [XafDisplayName("Fecha Inicio")]
    public DateTime FechaInicio
    {
        get => _fechaInicio;
        set => SetPropertyValue(nameof(FechaInicio), ref _fechaInicio, value);
    }

    [XafDisplayName("Fecha Fin")]
    public DateTime? FechaFin
    {
        get => _fechaFin;
        set => SetPropertyValue(nameof(FechaFin), ref _fechaFin, value);
    }

    [ModelDefault("DisplayFormat", "{0:C2}")]
    [ModelDefault("EditMask", "c2")]
    [XafDisplayName("Importe Final")]
    public decimal ImporteFinal
    {
        get => _importeFinal;
        set => SetPropertyValue(nameof(ImporteFinal), ref _importeFinal, value);
    }

    [XafDisplayName("Estado")]
    public EstadoSuscripcion Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [XafDisplayName("Última Fecha Cobro")]
    public DateTime? UltimaFechaCobro
    {
        get => _ultimaFechaCobro;
        set => SetPropertyValue(nameof(UltimaFechaCobro), ref _ultimaFechaCobro, value);
    }

    [XafDisplayName("Próxima Fecha Cobro")]
    public DateTime? ProximaFechaCobro
    {
        get => _proximaFechaCobro;
        set => SetPropertyValue(nameof(ProximaFechaCobro), ref _proximaFechaCobro, value);
    }

    [XafDisplayName("Pedido Vigente")]
    [ModelDefault("AllowEdit", "False")]
    public PedidoVenta? PedidoVigente
    {
        get => _pedidoVigente;
        set => SetPropertyValue(nameof(PedidoVigente), ref _pedidoVigente, value);
    }

    [Association("Suscripcion-PedidosVenta")]
    [XafDisplayName("Historial de Pedidos")]
    public XPCollection<PedidoVenta> Pedidos => GetCollection<PedidoVenta>(nameof(Pedidos));

    [Association("Suscripcion-Coberturas")]
    [XafDisplayName("Coberturas")]
    public XPCollection<CoberturaSuscripcion> Coberturas => GetCollection<CoberturaSuscripcion>(nameof(Coberturas));

    [XafDisplayName("Vencida")]
    public bool EstaVencida
    {
        get
        {
            if (Estado != EstadoSuscripcion.Activa || !ProximaFechaCobro.HasValue)
                return false;
            return ProximaFechaCobro.Value <= InformacionEmpresaHelper.GetLocalTime(Session).Date;
        }
    }

    [XafDisplayName("Vence este Mes")]
    public bool VenceEsteMes
    {
        get
        {
            if (Estado != EstadoSuscripcion.Activa || !ProximaFechaCobro.HasValue)
                return false;
            var hoy = InformacionEmpresaHelper.GetLocalTime(Session).Date;
            return ProximaFechaCobro.Value.Year == hoy.Year && ProximaFechaCobro.Value.Month == hoy.Month;
        }
    }

    [Browsable(false)]
    public bool PuedeGenerarFactura => Estado == EstadoSuscripcion.Activa && Cliente != null;

    /*[Action(Caption = "Generar Factura", ImageName = "Action_Generate", TargetObjectsCriteria = "PuedeGenerarFactura")]*/
    public void GenerarFacturaAction()
    {
        GenerarFactura();
        Session.CommitTransaction();
    }

    /*[Action(Caption = "Cambiar Condiciones", ImageName = "Action_Edit", TargetObjectsCriteria = "Estado = 'Activa'")]*/
    public void CambiarCondiciones(string motivo, decimal nuevoPrecio, Periodicidad nuevaPeriodicidad)
    {
        if (PedidoVigente != null)
        {
            PedidoVigente.EstadoVigencia = EstadoVigenciaPedido.Sustituido;
            PedidoVigente.FechaFinVigencia = InformacionEmpresaHelper.GetLocalTime(Session).Date;
            PedidoVigente.MotivoSustitucion = motivo;
        }

        ImporteFinal = nuevoPrecio;
        if (TipoSuscripcion != null)
        {
            TipoSuscripcion.Periodicidad = nuevaPeriodicidad;
        }

        CrearPedidoVigente();
    }

    private void CrearPedidoVigente()
    {
        var localTime = InformacionEmpresaHelper.GetLocalTime(Session);
        var nuevoPedido = new PedidoVenta(Session)
        {
            Suscripcion = this,
            Cliente = Cliente,
            Fecha = localTime.Date,
            FechaInicioVigencia = localTime.Date,
            EstadoVigencia = EstadoVigenciaPedido.Vigente,
            ImporteSuscripcion = ImporteFinal,
            PeriodicidadSuscripcion = TipoSuscripcion?.Periodicidad ?? Periodicidad.Mensual
        };

        var linea = new DocumentoVentaLinea(Session)
        {
            DocumentoVenta = nuevoPedido,
            Producto = Producto,
            Cantidad = 1,
            PrecioUnitario = ImporteFinal,
            NombreProducto = NombreDisplay
        };

        if (Producto != null)
        {
            foreach (var tax in Producto.ImpuestosVentas)
            {
                linea.TiposImpuestoVenta.Add(tax);
            }
        }
        else if (TipoSuscripcion != null)
        {
            foreach (var tax in TipoSuscripcion.Impuestos)
            {
                linea.TiposImpuestoVenta.Add(tax);
            }
        }

        nuevoPedido.Lineas.Add(linea);
        nuevoPedido.RecalcularTotales();

        PedidoVigente = nuevoPedido;
    }

    public FacturaVenta GenerarFactura()
    {
        if (Cliente == null)
            throw new InvalidOperationException("La suscripción debe tener un cliente asignado.");

        var localTime = InformacionEmpresaHelper.GetLocalTime(Session);
        var factura = new FacturaVenta(Session)
        {
            Cliente = Cliente,
            Fecha = localTime.Date
        };

        var linea = new DocumentoVentaLinea(Session)
        {
            DocumentoVenta = factura,
            Producto = Producto,
            Cantidad = 1,
            PrecioUnitario = ImporteFinal,
            NombreProducto = NombreDisplay
        };

        if (Producto != null)
        {
            foreach (var tax in Producto.ImpuestosVentas)
            {
                linea.TiposImpuestoVenta.Add(tax);
            }
        }
        else if (TipoSuscripcion != null)
        {
            foreach (var tax in TipoSuscripcion.Impuestos)
            {
                linea.TiposImpuestoVenta.Add(tax);
            }
        }

        factura.Lineas.Add(linea);
        factura.RecalcularTotales();

        // Actualizar fechas
        UltimaFechaCobro = localTime.Date;
        ProximaFechaCobro = CalcularProximaFecha(ProximaFechaCobro ?? localTime.Date);

        return factura;
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    private DateTime CalcularProximaFecha(DateTime fechaActual)
    {
        var intervalo = TipoSuscripcion?.Intervalo ?? 1;
        var periodicidad = TipoSuscripcion?.Periodicidad ?? Periodicidad.Mensual;

        return periodicidad switch
        {
            Periodicidad.Mensual => fechaActual.AddMonths(intervalo),
            Periodicidad.Trimestral => fechaActual.AddMonths(intervalo * 3),
            Periodicidad.Semestral => fechaActual.AddMonths(intervalo * 6),
            Periodicidad.Anual => fechaActual.AddYears(intervalo),
            _ => fechaActual.AddMonths(intervalo)
        };
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaInicio = InformacionEmpresaHelper.GetLocalTime(Session).Date;
        Estado = EstadoSuscripcion.Activa;

        if (!IsLoading && TipoSuscripcion != null)
        {
            ImporteFinal = TipoSuscripcion.ImporteBase;
            // Podríamos heredar coberturas del TipoSuscripcion aquí si las añadiéramos al TipoSuscripcion
        }
    }

    protected override void OnSaved()
    {
        base.OnSaved();
        if (PedidoVigente == null && Estado == EstadoSuscripcion.Activa && Cliente != null)
        {
            CrearPedidoVigente();
            Session.CommitTransaction();
        }
    }

    public static int ProcesarFacturacionMensual(Session session)
    {
        var hoy = InformacionEmpresaHelper.GetLocalTime(session).Date;
        var primerDiaMes = new DateTime(hoy.Year, hoy.Month, 1);
        var ultimoDiaMes = primerDiaMes.AddMonths(1).AddDays(-1);

        var criteria = CriteriaOperator.Parse("Estado = ? AND ProximaFechaCobro >= ? AND ProximaFechaCobro <= ?", 
            EstadoSuscripcion.Activa, primerDiaMes, ultimoDiaMes);

        var suscripciones = new XPCollection<Suscripcion>(session, criteria);
        int generadas = 0;

        foreach (var suscripcion in suscripciones.ToList())
        {
            try
            {
                suscripcion.GenerarFactura();
                generadas++;
            }
            catch (Exception ex)
            {
                DevExpress.Persistent.Base.Tracing.Tracer.LogError(ex);
            }
        }

        return generadas;
    }
}
