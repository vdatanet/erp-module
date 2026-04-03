using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Contabilidad;

namespace erp.Module.Models.Contabilidad;

[DomainComponent]
[XafDisplayName("Balance de Sumas y Saldos")]
public class BalanceSumasSaldosItem
{
    [XafDisplayName("Código")]
    public string? Codigo { get; set; }

    [XafDisplayName("Nombre")]
    public string? Nombre { get; set; }

    [XafDisplayName("Suma Debe")]
    [ModelDefault("DisplayFormat", "{0:N2}")]
    public decimal SumaDebe { get; set; }

    [XafDisplayName("Suma Haber")]
    [ModelDefault("DisplayFormat", "{0:N2}")]
    public decimal SumaHaber { get; set; }

    [XafDisplayName("Saldo Deudor")]
    [ModelDefault("DisplayFormat", "{0:N2}")]
    public decimal SaldoDeudor => SumaDebe > SumaHaber ? SumaDebe - SumaHaber : 0;

    [XafDisplayName("Saldo Acreedor")]
    [ModelDefault("DisplayFormat", "{0:N2}")]
    public decimal SaldoAcreedor => SumaHaber > SumaDebe ? SumaHaber - SumaDebe : 0;
}

[DomainComponent]
[XafDisplayName("Parámetros Balance de Sumas y Saldos")]
public class BalanceSumasSaldosParameters
{
    [XafDisplayName("Ejercicio")]
    public Ejercicio? Ejercicio { get; set; }

    [XafDisplayName("Fecha Inicio")]
    public DateTime? FechaInicio { get; set; }

    [XafDisplayName("Fecha Fin")]
    public DateTime? FechaFin { get; set; }
}
