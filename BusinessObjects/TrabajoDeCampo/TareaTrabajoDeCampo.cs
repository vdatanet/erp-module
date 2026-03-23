using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.TrabajoDeCampo;

[DefaultClassOptions]
[NavigationItem("Trabajo de campo")]
[XafDisplayName("Tarea de trabajo de campo")]
public class TareaTrabajoDeCampo(Session session) : EntidadBase(session)
{
    private ServicioTrabajoDeCampo? _servicioTC;
    private string? _descripcion;
    private bool _finalizada;
    private DateTime? _fechaFinalizacion;

    [Association("ServicioTC-TareasTC")]
    [XafDisplayName("Servicio TC")]
    public ServicioTrabajoDeCampo? ServicioTC
    {
        get => _servicioTC;
        set => SetPropertyValue(nameof(ServicioTC), ref _servicioTC, value);
    }

    [XafDisplayName("Descripción")]
    public string? Descripcion
    {
        get => _descripcion;
        set => SetPropertyValue(nameof(Descripcion), ref _descripcion, value);
    }

    [XafDisplayName("¿Finalizada?")]
    public bool Finalizada
    {
        get => _finalizada;
        set
        {
            if (SetPropertyValue(nameof(Finalizada), ref _finalizada, value))
            {
                if (!IsLoading && !IsSaving)
                {
                    FechaFinalizacion = value ? DateTime.Now : null;
                }
            }
        }
    }

    [XafDisplayName("Fecha finalización")]
    public DateTime? FechaFinalizacion
    {
        get => _fechaFinalizacion;
        set => SetPropertyValue(nameof(FechaFinalizacion), ref _fechaFinalizacion, value);
    }
}
