using erp.Module.BusinessObjects.Base.Facturacion;

namespace erp.Module.Models.VeriFactu;

public class Invoice
{
    public Invoice(string sequence, DateTime date, string sellerId)
    {
        Sequence = sequence;
        Date = date;
        SellerID = sellerId;
        TaxItems = new List<TaxItem>();
    }

    public string Sequence { get; set; }
    public DateTime Date { get; set; }
    public string SellerID { get; set; }
    public string? SellerName { get; set; }
    public TipoFacturaAmigable InvoiceType { get; set; }
    public string? Text { get; set; }
    
    public string? BuyerID { get; set; }
    public string? BuyerName { get; set; }
    public IDType? BuyerIDType { get; set; }
    public string? BuyerCountryID { get; set; }

    public List<TaxItem> TaxItems { get; set; }
}

public class TaxItem
{
    public decimal TaxBase { get; set; }
    public CalificacionOperacion TaxType { get; set; }
    public ClaveRegimen TaxScheme { get; set; }
    public Impuesto Tax { get; set; }
    public CausaExencion? TaxException { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
}

public class VeriFactuResponse
{
    public EstadoVeriFactu Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? HuellaFiscal { get; set; }
    public string? RawResponse { get; set; }
    public string? ValidationUrl { get; set; }
    public byte[]? QrData { get; set; }
    public string? BatchId { get; set; }
    public byte[]? Xml { get; set; }
}
