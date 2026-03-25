using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using System.ComponentModel;
using TimeZoneConverter;

namespace erp.Module.BusinessObjects.Configuraciones;

[DefaultClassOptions]
[NavigationItem("Configuraciones")]
[XafDisplayName("Zona Horaria")]
[DefaultProperty(nameof(Nombre))]
[ImageName("BO_Scheduler")]
public class ZonaHoraria(Session session) : EntidadBase(session)
{
    private string? _nombre;
    private string? _idZonaHoraria;
    private bool _activo = true;

    [Size(255)]
    [XafDisplayName("Nombre")]
    [RuleRequiredField("RuleRequiredField_ZonaHoraria_Nombre", DefaultContexts.Save, CustomMessageTemplate = "El Nombre es obligatorio")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(255)]
    [XafDisplayName("ID Zona Horaria")]
    [RuleRequiredField("RuleRequiredField_ZonaHoraria_IdZonaHoraria", DefaultContexts.Save, CustomMessageTemplate = "El ID de la Zona Horaria es obligatorio")]
    public string? IdZonaHoraria
    {
        get => _idZonaHoraria;
        set => SetPropertyValue(nameof(IdZonaHoraria), ref _idZonaHoraria, value);
    }

    [XafDisplayName("Activo")]
    public bool Activo
    {
        get => _activo;
        set => SetPropertyValue(nameof(Activo), ref _activo, value);
    }

    public TimeZoneInfo? GetTimeZoneInfo()
    {
        if (string.IsNullOrEmpty(IdZonaHoraria)) return null;

        try
        {
            return TZConvert.GetTimeZoneInfo(IdZonaHoraria);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
