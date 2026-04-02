using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Configuraciones;

namespace erp.Module.BusinessObjects.Facturacion;

[DefaultClassOptions]
[XafDisplayName("Log de Reinicio VeriFactu")]
[NavigationItem(false)]
public class VeriFactuLogReinicio(Session session) : BaseObject(session)
{
    private InformacionEmpresa? _empresa;
    private DateTime _fecha;
    private ApplicationUser? _usuario;
    private string? _motivo;
    private string? _numeroInstalacionAnterior;
    private string? _numeroInstalacionNuevo;

    [Association("Empresa-VeriFactuLogs")]
    [XafDisplayName("Empresa")]
    public InformacionEmpresa? Empresa
    {
        get => _empresa;
        set => SetPropertyValue(nameof(Empresa), ref _empresa, value);
    }

    public DateTime Fecha
    {
        get => _fecha;
        set => SetPropertyValue(nameof(Fecha), ref _fecha, value);
    }

    public ApplicationUser? Usuario
    {
        get => _usuario;
        set => SetPropertyValue(nameof(Usuario), ref _usuario, value);
    }

    [Size(SizeAttribute.Unlimited)]
    public string? Motivo
    {
        get => _motivo;
        set => SetPropertyValue(nameof(Motivo), ref _motivo, value);
    }

    public string? NumeroInstalacionAnterior
    {
        get => _numeroInstalacionAnterior;
        set => SetPropertyValue(nameof(NumeroInstalacionAnterior), ref _numeroInstalacionAnterior, value);
    }

    public string? NumeroInstalacionNuevo
    {
        get => _numeroInstalacionNuevo;
        set => SetPropertyValue(nameof(NumeroInstalacionNuevo), ref _numeroInstalacionNuevo, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Fecha = DateTime.Now;
    }
}
