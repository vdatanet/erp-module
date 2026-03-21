using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Contactos;

namespace erp.Module.BusinessObjects.Crm;

[DefaultClassOptions]
[NavigationItem("Crm")]
[XafDisplayName("Lead")]
[ImageName("BO_Lead")]
public class Lead(Session session) : Contacto(session)
{
    private string? _asunto;
    private string? _mensaje;
    private ApplicationUser? _responsable;
    private EquipoVenta? _equipoVenta;
    private Contacto? _vendedor;
    private Campana? _campana;
    private Medio? _medio;
    private Fuente? _fuente;

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Asunto")]
    public string? Asunto
    {
        get => _asunto;
        set => SetPropertyValue(nameof(Asunto), ref _asunto, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Mensaje")]
    public string? Mensaje
    {
        get => _mensaje;
        set => SetPropertyValue(nameof(Mensaje), ref _mensaje, value);
    }

    [XafDisplayName("Responsable")]
    public ApplicationUser? Responsable
    {
        get => _responsable;
        set => SetPropertyValue(nameof(Responsable), ref _responsable, value);
    }

    [XafDisplayName("Equipo de Venta")]
    [Association("EquipoVenta-Leads")]
    public EquipoVenta? EquipoVenta
    {
        get => _equipoVenta;
        set => SetPropertyValue(nameof(EquipoVenta), ref _equipoVenta, value);
    }

    [XafDisplayName("Vendedor")]
    [ToolTip("El vendedor puede ser un empleado o un agente externo (ambos son Contactos)")]
    public Contacto? Vendedor
    {
        get => _vendedor;
        set => SetPropertyValue(nameof(Vendedor), ref _vendedor, value);
    }

    [XafDisplayName("Campaña")]
    [Association("Campana-Leads")]
    public Campana? Campana
    {
        get => _campana;
        set => SetPropertyValue(nameof(Campana), ref _campana, value);
    }

    [XafDisplayName("Medio")]
    [Association("Medio-Leads")]
    public Medio? Medio
    {
        get => _medio;
        set => SetPropertyValue(nameof(Medio), ref _medio, value);
    }

    [XafDisplayName("Fuente")]
    [Association("Fuente-Leads")]
    public Fuente? Fuente
    {
        get => _fuente;
        set => SetPropertyValue(nameof(Fuente), ref _fuente, value);
    }

    [Action(Caption = "Convertir a Cliente y Oportunidad", ConfirmationMessage = "¿Está seguro de que desea convertir este Lead?", ImageName = "BO_Opportunity", TargetObjectsCriteria = "Cliente IS NULL")]
    public void Madurar()
    {
        if (Cliente != null) return;

        // 1. Crear el Cliente si no existe
        var cliente = new Cliente(Session);
        cliente.Nombre = Nombre;
        cliente.NombreComercial = NombreComercial;
        cliente.CorreoElectronico = CorreoElectronico;
        cliente.Telefono = Telefono;
        cliente.Direccion = Direccion;
        cliente.CodigoPostal = CodigoPostal;
        cliente.Poblacion = Poblacion;
        cliente.Provincia = Provincia;
        cliente.Pais = Pais;
        
        // Asociar el lead al nuevo cliente
        Cliente = cliente;

        // 2. Crear la Oportunidad
        var oportunidad = new Oportunidad(Session);
        oportunidad.Titulo = Asunto;
        oportunidad.Descripcion = Mensaje;
        oportunidad.Cliente = cliente;
        oportunidad.Responsable = Responsable;
        oportunidad.EquipoVenta = EquipoVenta;
        oportunidad.Vendedor = Vendedor;
        oportunidad.Campana = Campana;
        oportunidad.Medio = Medio;
        oportunidad.Fuente = Fuente;
        oportunidad.Notas = Notas;
    }
}
