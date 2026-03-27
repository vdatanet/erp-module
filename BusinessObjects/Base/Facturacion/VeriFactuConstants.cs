namespace erp.Module.BusinessObjects.Base.Facturacion;

public enum EstadoVeriFactu
{
    Borrador,
    Enviado,
    EnviadoConErrores,
    ErrorTecnico,
    Rechazado
}

public static class VeriFactuConstants
{
    public const string Correcto = "Correcto";
    public const string Error = "Error";
    public const string Parcial = "Parcial";
}
