using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Tpv;

public enum TipoEventoSesionTpv
{
    Apertura,
    Cierre,
    Reapertura,
    CambioObservaciones,
    RetiradaEfectivo,
    MovimientoManual
}

[XafDisplayName("Evento de Sesión TPV")]
[Persistent("SesionTpvEvento")]
[DefaultClassOptions]
[NavigationItem("Tpv")]
public class SesionTpvEvento(Session session) : EntidadBase(session)
{
    private DateTime _fechaHora;
    private ApplicationUser? _usuario;
    private TipoEventoSesionTpv _tipoEvento;
    private string? _descripcion;
    private decimal _importeAnterior;
    private decimal _importeNuevo;
    private string? _estadoAnterior;
    private string? _estadoNuevo;
    private SesionTpv? _sesion;

    [Association("SesionTpv-Eventos")]
    [XafDisplayName("Sesión")]
    public SesionTpv? Sesion
    {
        get => _sesion;
        set => SetPropertyValue(nameof(Sesion), ref _sesion, value);
    }

    [XafDisplayName("Fecha/Hora")]
    public DateTime FechaHora
    {
        get => _fechaHora;
        set => SetPropertyValue(nameof(FechaHora), ref _fechaHora, value);
    }

    [XafDisplayName("Usuario")]
    public ApplicationUser? Usuario
    {
        get => _usuario;
        set => SetPropertyValue(nameof(Usuario), ref _usuario, value);
    }

    [XafDisplayName("Tipo de Evento")]
    public TipoEventoSesionTpv TipoEvento
    {
        get => _tipoEvento;
        set => SetPropertyValue(nameof(TipoEvento), ref _tipoEvento, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Descripción")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [XafDisplayName("Importe Anterior")]
    public decimal ImporteAnterior
    {
        get => _importeAnterior;
        set => SetPropertyValue(nameof(ImporteAnterior), ref _importeAnterior, value);
    }

    [XafDisplayName("Importe Nuevo")]
    public decimal ImporteNuevo
    {
        get => _importeNuevo;
        set => SetPropertyValue(nameof(ImporteNuevo), ref _importeNuevo, value);
    }

    [XafDisplayName("Estado Anterior")]
    public string? EstadoAnterior
    {
        get => _estadoAnterior;
        set => SetPropertyValue(nameof(EstadoAnterior), ref _estadoAnterior, value);
    }

    [XafDisplayName("Estado Nuevo")]
    public string? EstadoNuevo
    {
        get => _estadoNuevo;
        set => SetPropertyValue(nameof(EstadoNuevo), ref _estadoNuevo, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        // La fecha se asigna preferiblemente desde el servicio o el método RegistrarEvento
        // para garantizar la hora local de la empresa/TPV.
        FechaHora = InformacionEmpresaHelper.GetLocalTime(Session);
    }
}
