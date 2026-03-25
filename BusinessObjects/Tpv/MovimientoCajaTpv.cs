using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using DevExpress.ExpressApp.Security;
using Microsoft.Extensions.DependencyInjection;
using erp.Module.Helpers.Contactos;

namespace erp.Module.BusinessObjects.Tpv;

public enum TipoMovimientoCajaTpv
{
    Apertura,
    Retirada,
    Ingreso,
    Ajuste,
    Cierre
}

[DefaultClassOptions]
[NavigationItem("Tpv")]
[XafDisplayName("Movimiento de Caja TPV")]
[Persistent("MovimientoCajaTpv")]
public class MovimientoCajaTpv(Session session) : EntidadBase(session)
{
    private DateTime _fecha;
    private TipoMovimientoCajaTpv _tipo;
    private decimal _importe;
    private string? _motivo;
    private SesionTpv? _sesionTpv;
    private ApplicationUser? _usuario;

    [XafDisplayName("Fecha")]
    [ModelDefault("DisplayFormat", "G")]
    [RuleRequiredField]
    public DateTime Fecha
    {
        get => _fecha;
        set => SetPropertyValue(nameof(Fecha), ref _fecha, value);
    }

    [XafDisplayName("Tipo")]
    public TipoMovimientoCajaTpv Tipo
    {
        get => _tipo;
        set => SetPropertyValue(nameof(Tipo), ref _tipo, value);
    }

    [XafDisplayName("Importe")]
    [ModelDefault("DisplayFormat", "{0:n2} €")]
    [ModelDefault("EditMask", "n2")]
    public decimal Importe
    {
        get => _importe;
        set => SetPropertyValue(nameof(Importe), ref _importe, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Motivo")]
    public string? Motivo
    {
        get => _motivo;
        set => SetPropertyValue(nameof(Motivo), ref _motivo, value);
    }

    [XafDisplayName("Sesión TPV")]
    [Association("SesionTpv-Movimientos")]
    [RuleRequiredField]
    public SesionTpv? SesionTpv
    {
        get => _sesionTpv;
        set => SetPropertyValue(nameof(SesionTpv), ref _sesionTpv, value);
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
        Fecha = InformacionEmpresaHelper.GetLocalTime(Session);
        var userId = Session.ServiceProvider?.GetService<ISecurityStrategyBase>()?.UserId;
        Usuario = userId != null ? Session.GetObjectByKey<ApplicationUser>(userId) : null;
    }
}
