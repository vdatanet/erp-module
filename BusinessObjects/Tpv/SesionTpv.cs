using System.ComponentModel;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Tpv;

public enum EstadoSesionTpv
{
    Abierta,
    Cerrada
}

[DefaultClassOptions]
[NavigationItem("Tpv")]
[XafDisplayName("Sesión TPV")]
[Persistent("SesionTpv")]
[DefaultProperty(nameof(Apertura))]
public class SesionTpv(Session session) : EntidadBase(session)
{
    private DateTime _apertura;
    private DateTime? _cierre;
    private EstadoSesionTpv _estado;
    private decimal _importeApertura;
    private decimal _importeCierre;
    private string? _observaciones;
    private Tpv? _tpv;
    private ApplicationUser? _usuario;

    [XafDisplayName("TPV")]
    [RuleRequiredField("RuleRequiredField_SesionTpv_Tpv", DefaultContexts.Save, CustomMessageTemplate = "El TPV de la Sesión es obligatorio")]
    [Association("Tpv-Sesiones")]
    public Tpv? Tpv
    {
        get => _tpv;
        set => SetPropertyValue(nameof(Tpv), ref _tpv, value);
    }

    [XafDisplayName("Usuario")]
    [RuleRequiredField("RuleRequiredField_SesionTpv_Usuario", DefaultContexts.Save, CustomMessageTemplate = "El Usuario de la Sesión es obligatorio")]
    public ApplicationUser? Usuario
    {
        get => _usuario;
        set => SetPropertyValue(nameof(Usuario), ref _usuario, value);
    }

    [XafDisplayName("Apertura")]
    [RuleRequiredField("RuleRequiredField_SesionTpv_Apertura", DefaultContexts.Save, CustomMessageTemplate = "La Fecha de Apertura de la Sesión es obligatoria")]
    public DateTime Apertura
    {
        get => _apertura;
        set => SetPropertyValue(nameof(Apertura), ref _apertura, value);
    }

    [XafDisplayName("Cierre")]
    [Appearance("CierreVisibleWhenClosed", Visibility = ViewItemVisibility.Hide, Criteria = "Estado = 'Abierta'")]
    public DateTime? Cierre
    {
        get => _cierre;
        set => SetPropertyValue(nameof(Cierre), ref _cierre, value);
    }

    [XafDisplayName("Importe Apertura")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal ImporteApertura
    {
        get => _importeApertura;
        set => SetPropertyValue(nameof(ImporteApertura), ref _importeApertura, value);
    }

    [XafDisplayName("Importe Cierre")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [Appearance("ImporteCierreVisibleWhenClosed", Visibility = ViewItemVisibility.Hide,
        Criteria = "Estado = 'Abierta'")]
    public decimal ImporteCierre
    {
        get => _importeCierre;
        set => SetPropertyValue(nameof(ImporteCierre), ref _importeCierre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [XafDisplayName("Estado")]
    [ModelDefault("AllowEdit", "False")]
    public EstadoSesionTpv Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [Association("SesionTpv-FacturasSimplificadas")]
    [XafDisplayName("Facturas Simplificadas")]
    public XPCollection<FacturaSimplificada> FacturasSimplificadas => GetCollection<FacturaSimplificada>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Apertura = DateTime.Now;
        Estado = EstadoSesionTpv.Abierta;
    }

    /*[Action(Caption = "Cerrar Sesión", TargetObjectsCriteria = "Estado = 'Abierta'",
        ConfirmationMessage = "¿Desea cerrar la sesión?", ImageName = "Action_Close")]*/
    public void CerrarSesion()
    {
        Estado = EstadoSesionTpv.Cerrada;
        Cierre = DateTime.Now;
        // El importe de cierre se suele introducir manualmente en la vista de detalle antes de cerrar, 
        // o se podría calcular aquí la suma de facturas si fuera necesario.
    }
}