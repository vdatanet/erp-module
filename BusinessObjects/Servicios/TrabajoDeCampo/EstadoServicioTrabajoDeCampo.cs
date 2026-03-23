using DevExpress.ExpressApp.DC;

namespace erp.Module.BusinessObjects.Servicios.TrabajoDeCampo;

public enum EstadoServicioTrabajoDeCampo
{
    [XafDisplayName("Pendiente de planificación")]
    PendientePlanificacion,
    [XafDisplayName("Planificado")]
    Planificado,
    [XafDisplayName("En curso")]
    EnCurso,
    [XafDisplayName("En pausa")]
    EnPausa,
    [XafDisplayName("Finalizado")]
    Finalizado,
    [XafDisplayName("Cancelado")]
    Cancelado
}
