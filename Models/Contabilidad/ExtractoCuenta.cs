using System.ComponentModel;
using DevExpress.ExpressApp.Data;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using erp.Module.BusinessObjects.Contabilidad;

namespace erp.Module.Models.Contabilidad;

[DomainComponent]
[XafDisplayName("Extracto de Cuenta")]
public class ExtractoCuentaItem
{
    [Key]
    [Browsable(false)]
    public Guid Oid { get; set; } = Guid.NewGuid();

    [XafDisplayName("Fecha")]
    public DateTime Fecha { get; set; }

    [XafDisplayName("Asiento")]
    public string? Asiento { get; set; }

    [XafDisplayName("Concepto")]
    public string? Concepto { get; set; }

    [XafDisplayName("Debe")]
    [ModelDefault("DisplayFormat", "{0:N2}")]
    public decimal Debe { get; set; }

    [XafDisplayName("Haber")]
    [ModelDefault("DisplayFormat", "{0:N2}")]
    public decimal Haber { get; set; }

    [XafDisplayName("Saldo")]
    [ModelDefault("DisplayFormat", "{0:N2}")]
    public decimal Saldo { get; set; }
}

[DomainComponent]
[XafDisplayName("Parámetros Extracto de Cuenta")]
public class ExtractoCuentaParameters
{
    [Key]
    [Browsable(false)]
    public Guid Oid { get; set; } = Guid.NewGuid();

    [XafDisplayName("Cuenta Contable")]
    [RuleRequiredField]
    public CuentaContable? CuentaContable { get; set; }

    [XafDisplayName("Fecha Inicio")]
    public DateTime? FechaInicio { get; set; }

    [XafDisplayName("Fecha Fin")]
    public DateTime? FechaFin { get; set; }
}
