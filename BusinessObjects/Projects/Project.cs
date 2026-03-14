using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.TimeTracking;

namespace erp.Module.BusinessObjects.Projects;

[DefaultClassOptions]
[NavigationItem("Projects")]
[ImageName("BO_Project")]
[XafDisplayName("Project")]
public class Project(Session session) : BaseEntity(session)
{
    private string _codigo;
    private string _nombre;
    private string _descripcion;
    private bool _estaActivo = true;

    [Size(64)]
    [RuleRequiredField]
    [RuleUniqueValue]
    [XafDisplayName("Código")]
    public string Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [Size(255)]
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

    [Association("Project-Activities")]
    [XafDisplayName("Activities")]
    public XPCollection<ProjectActivity> Activities => GetCollection<ProjectActivity>(nameof(Activities));

    [Association("Project-TimesheetEntries")]
    [XafDisplayName("Partes de tiempo")]
    public XPCollection<TimesheetEntry> TimesheetEntries => GetCollection<TimesheetEntry>(nameof(TimesheetEntries));
}