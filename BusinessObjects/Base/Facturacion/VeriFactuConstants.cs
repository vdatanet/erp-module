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
