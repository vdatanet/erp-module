using DevExpress.ExpressApp.DC;

namespace erp.Module.BusinessObjects.Suscripciones;

public enum Periodicidad
{
    [XafDisplayName("Mensual")]
    Mensual,
    [XafDisplayName("Trimestral")]
    Trimestral,
    [XafDisplayName("Semestral")]
    Semestral,
    [XafDisplayName("Anual")]
    Anual
}

public enum EstadoSuscripcion
{
    [XafDisplayName("Activa")]
    Activa,
    [XafDisplayName("Pausada")]
    Pausada,
    [XafDisplayName("Cancelada")]
    Cancelada,
    [XafDisplayName("Finalizada")]
    Finalizada
}

public enum EstadoTipoSuscripcion
{
    [XafDisplayName("Activo")]
    Activo,
    [XafDisplayName("Inactivo")]
    Inactivo
}
