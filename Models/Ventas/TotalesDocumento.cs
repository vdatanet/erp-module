namespace erp.Module.Models.Ventas;

public record TotalesDocumento
{
    public decimal BaseImponible { get; init; }
    public decimal ImporteImpuestos { get; init; }
    public decimal ImporteTotal { get; init; }
    public decimal TotalBruto { get; init; }
    public decimal TotalNeto { get; init; }
    public decimal ImporteIva { get; init; }
    public decimal ImporteRetencion { get; init; }
    public decimal ImportePagado { get; init; }
    public decimal ImportePendiente { get; init; }
}
