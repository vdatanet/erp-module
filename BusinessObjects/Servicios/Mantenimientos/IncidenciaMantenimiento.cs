using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Servicios.TrabajoDeCampo;

namespace erp.Module.BusinessObjects.Servicios.Mantenimientos;

[DefaultClassOptions]
[NavigationItem("Servicios")]
[XafDisplayName("Incidencias")]
public class IncidenciaMantenimiento(Session session) : EntidadBase(session)
{
    private string? _codigo;
    private string? _descripcion;
    private ActivoMantenimiento? _activo;
    private ContratoMantenimiento? _contrato;
    private Cliente? _cliente;
    private DateTime _fechaApertura;
    private DateTime? _fechaCierre;
    private int _prioridad;
    private string? _estado;
    private PedidoTrabajoDeCampo? _trabajoCampo;

    [XafDisplayName("Código")]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [RuleRequiredField]
    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripción")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [XafDisplayName("Activo")]
    public ActivoMantenimiento? Activo
    {
        get => _activo;
        set
        {
            if (SetPropertyValue(nameof(Activo), ref _activo, value))
            {
                if (!IsLoading && !IsSaving && value != null)
                {
                    if (Contrato == null) Contrato = value.Contrato;
                    if (Cliente == null) Cliente = value.Cliente;
                }
            }
        }
    }

    [XafDisplayName("Contrato")]
    public ContratoMantenimiento? Contrato
    {
        get => _contrato;
        set => SetPropertyValue(nameof(Contrato), ref _contrato, value);
    }

    [XafDisplayName("Cliente")]
    public Cliente? Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [XafDisplayName("Fecha Apertura")]
    public DateTime FechaApertura
    {
        get => _fechaApertura;
        set => SetPropertyValue(nameof(FechaApertura), ref _fechaApertura, value);
    }

    [XafDisplayName("Fecha Cierre")]
    public DateTime? FechaCierre
    {
        get => _fechaCierre;
        set => SetPropertyValue(nameof(FechaCierre), ref _fechaCierre, value);
    }

    [XafDisplayName("Prioridad")]
    public int Prioridad
    {
        get => _prioridad;
        set => SetPropertyValue(nameof(Prioridad), ref _prioridad, value);
    }

    [XafDisplayName("Estado")]
    public string? Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [XafDisplayName("Trabajo de Campo (Pedido)")]
    public PedidoTrabajoDeCampo? TrabajoCampo
    {
        get => _trabajoCampo;
        set => SetPropertyValue(nameof(TrabajoCampo), ref _trabajoCampo, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaApertura = DateTime.Now;
        Prioridad = 1;
        Estado = "Abierta";
    }
}
