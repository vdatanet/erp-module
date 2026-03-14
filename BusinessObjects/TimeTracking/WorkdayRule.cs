using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Contacts;

namespace erp.Module.BusinessObjects.TimeTracking;

[DefaultClassOptions]
[NavigationItem("Time Tracking")]
[ImageName("BO_Rules")]
[XafDefaultProperty(nameof(Nombre))]
public class WorkdayRule(Session session) : BaseEntity(session)
{
    private string _nombre;

    private TimeSpan _inicioJornada = new(10, 0, 0);
    private TimeSpan _finJornada = new(18, 0, 0);
    private TimeSpan _objetivoDiario = new(8, 0, 0);

    private TimeSpan _toleranciaEntradaTemprana = TimeSpan.Zero;
    private TimeSpan _toleranciaEntradaTarde = TimeSpan.Zero;
    private TimeSpan _toleranciaSalidaTemprana = TimeSpan.Zero;
    private TimeSpan _toleranciaSalidaTarde = TimeSpan.Zero;

    [Size(255)]
    [RuleRequiredField]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    //[ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    [ImmediatePostData]
    public TimeSpan InicioJornada
    {
        get => _inicioJornada;
        set
        {
            SetPropertyValue(nameof(InicioJornada), ref _inicioJornada, value);
            RecalcularObjetivoDiario();
        }
    }

    //[ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "hh\\:mm")]
    [ImmediatePostData]
    public TimeSpan FinJornada
    {
        get => _finJornada;
        set
        {
            SetPropertyValue(nameof(FinJornada), ref _finJornada, value);
            RecalcularObjetivoDiario();
        }
    }

    //[ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "h'h 'm'm'")]
    [ModelDefault("AllowEdit", "False")]
    public TimeSpan ObjetivoDiario
    {
        get => _objetivoDiario;
        set => SetPropertyValue(nameof(ObjetivoDiario), ref _objetivoDiario, value);
    }

    //[ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "h'h 'm'm'")]
    public TimeSpan ToleranciaEntradaTemprana
    {
        get => _toleranciaEntradaTemprana;
        set => SetPropertyValue(nameof(ToleranciaEntradaTemprana), ref _toleranciaEntradaTemprana, value);
    }

    //[ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "h'h 'm'm'")]
    public TimeSpan ToleranciaEntradaTarde
    {
        get => _toleranciaEntradaTarde;
        set => SetPropertyValue(nameof(ToleranciaEntradaTarde), ref _toleranciaEntradaTarde, value);
    }

    //[ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "h'h 'm'm'")]
    public TimeSpan ToleranciaSalidaTemprana
    {
        get => _toleranciaSalidaTemprana;
        set => SetPropertyValue(nameof(ToleranciaSalidaTemprana), ref _toleranciaSalidaTemprana, value);
    }

    //[ModelDefault(nameof(IModelCommonMemberViewItem.DisplayFormat), "h'h 'm'm'")]
    public TimeSpan ToleranciaSalidaTarde
    {
        get => _toleranciaSalidaTarde;
        set => SetPropertyValue(nameof(ToleranciaSalidaTarde), ref _toleranciaSalidaTarde, value);
    }
    
    [Association("WorkdayRule-Employees")]
    public XPCollection<Employee> Employees => GetCollection<Employee>(nameof(Employees));
    
    protected override void OnSaving()
    {
        base.OnSaving();
        RecalcularObjetivoDiario();
    }

    private void RecalcularObjetivoDiario()
    {
        if (FinJornada >= InicioJornada)
            ObjetivoDiario = FinJornada - InicioJornada;
        else
            ObjetivoDiario = TimeSpan.FromHours(24) - InicioJornada + FinJornada;
    }
}