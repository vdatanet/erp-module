using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Helpers.Contactos;
using erp.Module.Helpers.Facturacion;
using Microsoft.Extensions.DependencyInjection;
using erp.Module.Controllers.Configuraciones;

namespace erp.Module.BusinessObjects.Facturacion;

[DefaultClassOptions]
[XafDisplayName("Puesto VeriFactu")]
[NavigationItem("Configuraciones")]
public class VeriFactuPuesto(Session session) : BaseObject(session), IPersistentVeriFactuNumeroInstalacion
{
    private InformacionEmpresa? _empresa;
    private string? _nombrePuesto;
    private string? _hardwareFingerprint;
    private string? _numeroInstalacion;
    private bool _activo = true;

    [Association("Empresa-VeriFactuPuestos")]
    [XafDisplayName("Empresa")]
    public InformacionEmpresa? Empresa
    {
        get => _empresa;
        set => SetPropertyValue(nameof(Empresa), ref _empresa, value);
    }

    [Size(255)]
    [XafDisplayName("Nombre Puesto (Host)")]
    public string? NombrePuesto
    {
        get => _nombrePuesto;
        set => SetPropertyValue(nameof(NombrePuesto), ref _nombrePuesto, value);
    }

    [Size(255)]
    [XafDisplayName("Huella Hardware")]
    [ReadOnly(true)]
    public string? HardwareFingerprint
    {
        get => _hardwareFingerprint;
        set => SetPropertyValue(nameof(HardwareFingerprint), ref _hardwareFingerprint, value);
    }

    [Size(50)]
    [XafDisplayName("Número Instalación")]
    [ReadOnly(true)]
    public string? NumeroInstalacion
    {
        get => _numeroInstalacion;
        set => SetPropertyValue(nameof(NumeroInstalacion), ref _numeroInstalacion, value);
    }

    [XafDisplayName("Activo")]
    public bool Activo
    {
        get => _activo;
        set => SetPropertyValue(nameof(Activo), ref _activo, value);
    }

    public void ReiniciarNumeroInstalacion(string motivo)
    {
        var anterior = NumeroInstalacion;
        var nuevo = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        
        NumeroInstalacion = nuevo;
        HardwareFingerprint = HardwareFingerprintHelper.GetFingerprint();

        var log = new VeriFactuLogReinicio(Session)
        {
            Empresa = Empresa,
            Puesto = this,
            Fecha = InformacionEmpresaHelper.GetLocalTime(Session),
            Motivo = motivo,
            NumeroInstalacionAnterior = anterior,
            NumeroInstalacionNuevo = nuevo,
            Usuario = GetCurrentUser()
        };
        
        Session.Save(log);
    }

    private ApplicationUser? GetCurrentUser()
    {
        try
        {
            var security = Session.ServiceProvider?.GetService<ISecurityStrategyBase>();
            return security?.UserId == null ? null : Session.GetObjectByKey<ApplicationUser>(security.UserId);
        }
        catch
        {
            return null;
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        NombrePuesto = Environment.MachineName;
    }
}
