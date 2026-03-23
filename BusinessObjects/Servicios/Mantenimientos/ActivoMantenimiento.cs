using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;

namespace erp.Module.BusinessObjects.Servicios.Mantenimientos;

[DefaultClassOptions]
[NavigationItem("Servicios")]
[XafDisplayName("Activos")]
public class ActivoMantenimiento(Session session) : EntidadBase(session)
{
    private string? _codigo;
    private string? _nombre;
    private string? _descripcion;
    private string? _numeroSerie;
    private Cliente? _cliente;
    private ContratoMantenimiento? _contrato;
    private DateTime _fechaAlta;
    private string? _observaciones;

    [XafDisplayName("Código")]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripción")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [XafDisplayName("Número de Serie")]
    public string? NumeroSerie
    {
        get => _numeroSerie;
        set => SetPropertyValue(nameof(NumeroSerie), ref _numeroSerie, value);
    }

    [XafDisplayName("Cliente")]
    public Cliente? Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [Association("Contrato-Activos")]
    [XafDisplayName("Contrato")]
    public ContratoMantenimiento? Contrato
    {
        get => _contrato;
        set
        {
            if (SetPropertyValue(nameof(Contrato), ref _contrato, value))
            {
                if (!IsLoading && !IsSaving && value != null && Cliente == null)
                {
                    Cliente = value.Cliente;
                }
            }
        }
    }

    [XafDisplayName("Fecha Alta")]
    public DateTime FechaAlta
    {
        get => _fechaAlta;
        set => SetPropertyValue(nameof(FechaAlta), ref _fechaAlta, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [Association("Activo-Tareas")]
    [XafDisplayName("Tareas")]
    public XPCollection<TareaMantenimiento> Tareas => GetCollection<TareaMantenimiento>(nameof(Tareas));

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaAlta = DateTime.Today;
    }
}
