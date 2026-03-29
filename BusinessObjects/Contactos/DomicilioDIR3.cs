using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects.Contactos;

[DefaultClassOptions]
[NavigationItem("Contactos")]
[XafDisplayName("Domicilio DIR3")]
[ImageName("BO_Address")]
public class DomicilioDIR3(Session session) : Domicilio(session)
{
    private Cliente? _clienteDIR;
    private string? _oficinaContable;
    private string? _organoGestor;
    private string? _unidadTramitadora;
    private string? _organoProponente;

    [XafDisplayName("Cliente (Dirección DIR3)")]
    [Association("Cliente-DireccionesDIR3")]
    public Cliente? ClienteDIR
    {
        get => _clienteDIR;
        set => SetPropertyValue(nameof(ClienteDIR), ref _clienteDIR, value);
    }

    [Size(10)]
    [XafDisplayName("Oficina Contable")]
    public string? OficinaContable
    {
        get => _oficinaContable;
        set => SetPropertyValue(nameof(OficinaContable), ref _oficinaContable, value);
    }

    [Size(10)]
    [XafDisplayName("Órgano Gestor")]
    public string? OrganoGestor
    {
        get => _organoGestor;
        set => SetPropertyValue(nameof(OrganoGestor), ref _organoGestor, value);
    }

    [Size(10)]
    [XafDisplayName("Unidad Tramitadora")]
    public string? UnidadTramitadora
    {
        get => _unidadTramitadora;
        set => SetPropertyValue(nameof(UnidadTramitadora), ref _unidadTramitadora, value);
    }

    [Size(10)]
    [XafDisplayName("Órgano Proponente")]
    public string? OrganoProponente
    {
        get => _organoProponente;
        set => SetPropertyValue(nameof(OrganoProponente), ref _organoProponente, value);
    }
}
