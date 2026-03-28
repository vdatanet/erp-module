using DevExpress.ExpressApp.DC;

namespace erp.Module.BusinessObjects.Base.Facturacion;

public enum EstadoVeriFactu
{
    Borrador,
    Validado,
    PendienteVeriFactu,
    EnviadaVeriFactu,
    AceptadaVeriFactu,
    RechazadaVeriFactu,
    Impresa,
    ErrorTecnico
}

public static class VeriFactuConstants
{
    public const string Correcto = "Correcto";
    public const string Error = "Error";
    public const string Parcial = "Parcial";
}

public enum TipoFacturaAmigable
{
    [XafDisplayName("Factura Completa")] F1,
    [XafDisplayName("Factura Simplificada")] F2,
    [XafDisplayName("Factura en Sustitución de Simplificadas")] F3,
    [XafDisplayName("Resumen de Facturas")] F4,
    [XafDisplayName("Factura Rectificativa (Art. 80.1, 80.2 y 80.6)")] R1,
    [XafDisplayName("Factura Rectificativa (Art. 80.3)")] R2,
    [XafDisplayName("Factura Rectificativa (Art. 80.4)")] R3,
    [XafDisplayName("Factura Rectificativa (Resto)")] R4,
    [XafDisplayName("Factura Rectificativa en Facturas Simplificadas")] R5
}

public enum TipoRectificativaAmigable
{
    [XafDisplayName("Sustitutiva")] S,
    [XafDisplayName("Diferencias")] I
}
