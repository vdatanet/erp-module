using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.TrabajoDeCampo;

[DefaultClassOptions]
[NavigationItem("Trabajo de campo")]
[XafDisplayName("Tipo de servicio (TC)")]
public class TipoServicioTrabajoDeCampo(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private string? _descripcion;

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

    [Association("TipoServicio-Solicitudes")]
    public XPCollection<SolicitudTrabajoDeCampo> Solicitudes => GetCollection<SolicitudTrabajoDeCampo>(nameof(Solicitudes));
}
