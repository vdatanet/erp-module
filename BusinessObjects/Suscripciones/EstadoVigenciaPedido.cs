using DevExpress.ExpressApp.DC;

namespace erp.Module.BusinessObjects.Suscripciones;

public enum EstadoVigenciaPedido
{
    [XafDisplayName("Vigente")]
    Vigente,
    [XafDisplayName("Sustituido")]
    Sustituido,
    [XafDisplayName("Cerrado")]
    Cerrado,
    [XafDisplayName("Anulado")]
    Anulado
}
