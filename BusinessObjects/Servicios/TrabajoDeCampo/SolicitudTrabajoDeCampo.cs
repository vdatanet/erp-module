using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Servicios.TrabajoDeCampo;

[DefaultClassOptions]
[NavigationItem("Servicios")]
[XafDisplayName("Solicitud de trabajo de campo")]
public class SolicitudTrabajoDeCampo(Session session) : EntidadBase(session)
{
    private Cliente? _cliente;
    private Domicilio? _domicilio;
    private TipoServicioTrabajoDeCampo? _tipoServicio;
    private string? _descripcion;
    private DateTime _fechaSolicitud;
    private string? _contacto;
    private string? _telefonoContacto;

    [RuleRequiredField]
    [Association("Cliente-SolicitudesTC")]
    [XafDisplayName("Cliente")]
    public Cliente? Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [DataSourceProperty("Cliente.Domicilios")]
    [XafDisplayName("Domicilio de ejecución")]
    public Domicilio? Domicilio
    {
        get => _domicilio;
        set => SetPropertyValue(nameof(Domicilio), ref _domicilio, value);
    }

    [RuleRequiredField]
    [Association("TipoServicio-Solicitudes")]
    [XafDisplayName("Tipo de servicio")]
    public TipoServicioTrabajoDeCampo? TipoServicio
    {
        get => _tipoServicio;
        set => SetPropertyValue(nameof(TipoServicio), ref _tipoServicio, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripción del trabajo")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [XafDisplayName("Fecha solicitud")]
    public DateTime FechaSolicitud
    {
        get => _fechaSolicitud;
        set => SetPropertyValue(nameof(FechaSolicitud), ref _fechaSolicitud, value);
    }

    [XafDisplayName("Persona de contacto")]
    public string? Contacto
    {
        get => _contacto;
        set => SetPropertyValue(nameof(Contacto), ref _contacto, value);
    }

    [XafDisplayName("Teléfono de contacto")]
    public string? TelefonoContacto
    {
        get => _telefonoContacto;
        set => SetPropertyValue(nameof(TelefonoContacto), ref _telefonoContacto, value);
    }

    [Association("Solicitud-PedidosTC")]
    [XafDisplayName("Pedidos generados")]
    public XPCollection<PedidoTrabajoDeCampo> Pedidos => GetCollection<PedidoTrabajoDeCampo>(nameof(Pedidos));

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaSolicitud = InformacionEmpresaHelper.GetLocalTime(Session);
    }
}
