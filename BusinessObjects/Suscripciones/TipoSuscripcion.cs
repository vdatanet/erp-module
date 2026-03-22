using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Impuestos;

namespace erp.Module.BusinessObjects.Suscripciones;

[DefaultClassOptions]
[NavigationItem("Suscripciones")]
[ImageName("BO_Contract")]
[DefaultProperty(nameof(Nombre))]
public class TipoSuscripcion(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private string? _descripcion;
    private decimal _importeBase;
    private string _moneda = "EUR";
    private Periodicidad _periodicidad;
    private int _intervalo;
    private string? _reglaCobro;
    private bool _estaActivo;
    private bool _generarAutomaticamente;

    [RuleRequiredField("RuleRequiredField_TipoSuscripcion_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre del Tipo de Suscripción es obligatorio")]
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

    [ModelDefault("DisplayFormat", "{0:C2}")]
    [ModelDefault("EditMask", "c2")]
    [XafDisplayName("Importe Base")]
    public decimal ImporteBase
    {
        get => _importeBase;
        set => SetPropertyValue(nameof(ImporteBase), ref _importeBase, value);
    }

    [Size(3)]
    [XafDisplayName("Moneda")]
    public string Moneda
    {
        get => _moneda;
        set => SetPropertyValue(nameof(Moneda), ref _moneda, value);
    }

    [XafDisplayName("Periodicidad")]
    public Periodicidad Periodicidad
    {
        get => _periodicidad;
        set => SetPropertyValue(nameof(Periodicidad), ref _periodicidad, value);
    }

    [XafDisplayName("Intervalo")]
    [ToolTip("ejemplo: cada 1 mes, cada 3 meses, cada 1 año")]
    public int Intervalo
    {
        get => _intervalo;
        set => SetPropertyValue(nameof(Intervalo), ref _intervalo, value);
    }

    [XafDisplayName("Regla de Cobro")]
    public string? ReglaCobro
    {
        get => _reglaCobro;
        set => SetPropertyValue(nameof(ReglaCobro), ref _reglaCobro, value);
    }

    [XafDisplayName("Activo")]
    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    [XafDisplayName("Estado")]
    public EstadoTipoSuscripcion Estado
    {
        get
        {
            return EstaActivo ? EstadoTipoSuscripcion.Activo : EstadoTipoSuscripcion.Inactivo;
        }
        set
        {
            EstaActivo = (value == EstadoTipoSuscripcion.Activo);
        }
    }

    [XafDisplayName("Generar Automáticamente")]
    public bool GenerarAutomaticamente
    {
        get => _generarAutomaticamente;
        set => SetPropertyValue(nameof(GenerarAutomaticamente), ref _generarAutomaticamente, value);
    }

    [EditorAlias(EditorAliases.TagBoxListPropertyEditor)]
    [Association("TipoSuscripcion-Impuestos")]
    [XafDisplayName("Impuestos")]
    public XPCollection<TipoImpuesto> Impuestos => GetCollection<TipoImpuesto>();

    [Association("TipoSuscripcion-Suscripciones")]
    [XafDisplayName("Suscripciones")]
    public XPCollection<Suscripcion> Suscripciones => GetCollection<Suscripcion>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        EstaActivo = true;
        Intervalo = 1;
        Periodicidad = Periodicidad.Mensual;
        Moneda = "EUR";
    }
}
