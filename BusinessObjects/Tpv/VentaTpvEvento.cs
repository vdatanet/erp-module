using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.Helpers.Contactos;
using System.ComponentModel;

namespace erp.Module.BusinessObjects.Tpv;

[XafDisplayName("Evento de Venta TPV")]
[Persistent("VentaTpvEvento")]
public class VentaTpvEvento(Session session) : EntidadBase(session)
{
    private VentaTpv? _ventaTpv;
    private DateTime _fechaHora;
    private string? _accion;
    private string? _descripcion;
    private ApplicationUser? _usuario;

    [XafDisplayName("Venta TPV")]
    [Association("VentaTpv-Eventos")]
    public VentaTpv? VentaTpv
    {
        get => _ventaTpv;
        set => SetPropertyValue(nameof(VentaTpv), ref _ventaTpv, value);
    }

    [XafDisplayName("Fecha/Hora")]
    public DateTime FechaHora
    {
        get => _fechaHora;
        set => SetPropertyValue(nameof(FechaHora), ref _fechaHora, value);
    }

    [XafDisplayName("Acción")]
    [Size(100)]
    public string? Accion
    {
        get => _accion;
        set => SetPropertyValue(nameof(Accion), ref _accion, value);
    }

    [XafDisplayName("Descripción")]
    [Size(SizeAttribute.Unlimited)]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [XafDisplayName("Usuario")]
    public ApplicationUser? Usuario
    {
        get => _usuario;
        set => SetPropertyValue(nameof(Usuario), ref _usuario, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaHora = InformacionEmpresaHelper.GetLocalTime(Session);
    }
}
