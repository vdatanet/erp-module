using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Crm;
using erp.Module.BusinessObjects.Suscripciones;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[XafDisplayName("Pedido de Venta")]
[NavigationItem("Ventas")]
[ImageName("BO_Order")]
public class PedidoVenta(Session session) : DocumentoVenta(session)
{
    private Oportunidad? _oportunidad;
    private EstadoPedido _estadoPedido;
    private Suscripcion? _suscripcion;
    private EstadoVigenciaPedido _estadoVigencia;
    private DateTime? _fechaInicioVigencia;
    private DateTime? _fechaFinVigencia;
    private string? _motivoSustitucion;
    private decimal _importeSuscripcion;
    private Periodicidad _periodicidadSuscripcion;

    [Association("Suscripcion-PedidosVenta")]
    [XafDisplayName("Suscripción")]
    public Suscripcion? Suscripcion
    {
        get => _suscripcion;
        set => SetPropertyValue(nameof(Suscripcion), ref _suscripcion, value);
    }

    [XafDisplayName("Estado Vigencia")]
    public EstadoVigenciaPedido EstadoVigencia
    {
        get => _estadoVigencia;
        set => SetPropertyValue(nameof(EstadoVigencia), ref _estadoVigencia, value);
    }

    [XafDisplayName("Inicio Vigencia")]
    public DateTime? FechaInicioVigencia
    {
        get => _fechaInicioVigencia;
        set => SetPropertyValue(nameof(FechaInicioVigencia), ref _fechaInicioVigencia, value);
    }

    [XafDisplayName("Fin Vigencia")]
    public DateTime? FechaFinVigencia
    {
        get => _fechaFinVigencia;
        set => SetPropertyValue(nameof(FechaFinVigencia), ref _fechaFinVigencia, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Motivo Sustitución")]
    public string? MotivoSustitucion
    {
        get => _motivoSustitucion;
        set => SetPropertyValue(nameof(MotivoSustitucion), ref _motivoSustitucion, value);
    }

    [XafDisplayName("Importe Suscripción")]
    public decimal ImporteSuscripcion
    {
        get => _importeSuscripcion;
        set => SetPropertyValue(nameof(ImporteSuscripcion), ref _importeSuscripcion, value);
    }

    [XafDisplayName("Periodicidad Suscripción")]
    public Periodicidad PeriodicidadSuscripcion
    {
        get => _periodicidadSuscripcion;
        set => SetPropertyValue(nameof(PeriodicidadSuscripcion), ref _periodicidadSuscripcion, value);
    }

    [Association("Oportunidad-PedidosVenta")]
    [XafDisplayName("Oportunidad")]
    public Oportunidad? Oportunidad
    {
        get => _oportunidad;
        set
        {
            if (!SetPropertyValue(nameof(Oportunidad), ref _oportunidad, value) || IsLoading || IsSaving) return;
            if (value != null)
            {
                if (value.Cliente != null) Cliente = value.Cliente;
                if (value.EquipoVenta != null) EquipoVenta = value.EquipoVenta;
                if (value.Vendedor != null) Vendedor = value.Vendedor;
            }
        }
    }

    [XafDisplayName("Estado")]
    [ModelDefault("AllowEdit", "False")]
    public EstadoPedido EstadoPedido
    {
        get => _estadoPedido;
        set => SetPropertyValue(nameof(EstadoPedido), ref _estadoPedido, value);
    }

    [XafDisplayName("Borrador")]
    public bool Borrador => EstadoPedido == EstadoPedido.Borrador;

    [XafDisplayName("Confirmado")]
    public bool Confirmado => EstadoPedido >= EstadoPedido.Confirmado && EstadoPedido != EstadoPedido.Anulado;

    [XafDisplayName("Emitido")]
    public bool Emitido => false;

    [XafDisplayName("Impreso")]
    public bool Impreso => false;

    [XafDisplayName("Anulado")]
    public bool Anulado => EstadoPedido == EstadoPedido.Anulado;

    [XafDisplayName("Bloqueado")]
    public bool Bloqueado => false;

    [XafDisplayName("Sincronizado")]
    public bool Sincronizado => false;
}