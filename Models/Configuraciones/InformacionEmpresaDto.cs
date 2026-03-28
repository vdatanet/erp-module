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
    public string? ZonaHorariaPorDefecto { get; set; }
    public string? DiarioVentasPorDefecto { get; set; }
    public string? DiarioVentasSimplificadasPorDefecto { get; set; }
    public string? TextoDefectoVeriFactu { get; set; }
    public string? PrefijoFacturasSimplificadasPorDefecto { get; set; }
}
