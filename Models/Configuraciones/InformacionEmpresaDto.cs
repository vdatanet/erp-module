namespace erp.Module.Models.Configuraciones;

public class InformacionEmpresaDto
{
    public string? Nombre { get; set; }
    public string? NombreComercial { get; set; }
    public string? Nif { get; set; }
    public string? Direccion { get; set; }
    public string? CodigoPostal { get; set; }
    public string? Poblacion { get; set; }
    public string? Provincia { get; set; }
    public string? Pais { get; set; }
    public string? Telefono { get; set; }
    public string? Movil { get; set; }
    public string? CorreoElectronico { get; set; }
    public string? SitioWeb { get; set; }
    public byte[]? Logo { get; set; }
    public string? OneSignalAppId { get; set; }
    public string? OneSignalRestApiKey { get; set; }
    public string? OneSignalUserAuthKey { get; set; }
    public string? OneSignalDefaultEmailFrom { get; set; }
    public string? OneSignalDefaultEmailName { get; set; }
}
