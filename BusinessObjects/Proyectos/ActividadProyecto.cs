using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.ControlHorario;

namespace erp.Module.BusinessObjects.Proyectos;

[DefaultClassOptions]
[NavigationItem("Proyectos")]
[ImageName("BO_Tarea")]
[XafDisplayName("Actividad")]
public class ActividadProyecto(Session session) : EntidadBase(session)
{
    private Proyecto _proyecto;
    private string _codigo;
    private string _nombre;
    private string _descripcion;
    private bool _estaActivo = true;

    [Association("Proyecto-Activities")]
    [RuleRequiredField]
    [XafDisplayName("Proyecto")]
    public Proyecto Proyecto
    {
        get => _proyecto;
        set => SetPropertyValue(nameof(Proyecto), ref _proyecto, value);
    }

    [Size(64)]
    [XafDisplayName("Código")]
    public string Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [Size(256)]
    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [ModelDefault("RowCount", "4")]
    [XafDisplayName("Descripción")]
    public string Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [XafDisplayName("Activo")]
    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    [Association("ProjectActivity-TimesheetEntries")]
    [XafDisplayName("Partes de tiempo")]
    public XPCollection<EntradaParte> TimesheetEntries => GetCollection<EntradaParte>(nameof(TimesheetEntries));
}