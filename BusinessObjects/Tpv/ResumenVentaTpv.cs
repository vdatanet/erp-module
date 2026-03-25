using DevExpress.ExpressApp.DC;
using DevExpress.Xpo;
using System.ComponentModel;

namespace erp.Module.BusinessObjects.Tpv;

[DomainComponent]
public class ResumenVentaTpv
{
    [XafDisplayName("Base Imponible")]
    public decimal BaseImponible { get; set; }

    [XafDisplayName("Impuestos")]
    public decimal Impuestos { get; set; }

    [XafDisplayName("Total Bruto")]
    public decimal TotalBruto { get; set; }

    [XafDisplayName("Total Descuentos")]
    public decimal TotalDescuentos { get; set; }

    [XafDisplayName("Total Final")]
    public decimal TotalFinal { get; set; }

    [XafDisplayName("Total Pagado")]
    public decimal TotalPagado { get; set; }

    [XafDisplayName("Cambio a Devolver")]
    public decimal CambioADevolver { get; set; }
}
