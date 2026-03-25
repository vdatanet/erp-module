using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;

namespace erp.Module.BusinessObjects.Tpv;

[DomainComponent]
public class MovimientoCajaParameters
{
    [XafDisplayName("Tipo")]
    public TipoMovimientoCajaTpv Tipo { get; set; } = TipoMovimientoCajaTpv.Retirada;

    [XafDisplayName("Importe")]
    [RuleRange(0.01, (double)decimal.MaxValue, CustomMessageTemplate = "El importe debe ser mayor que cero")]
    public decimal Importe { get; set; }

    [XafDisplayName("Motivo")]
    [FieldSize(FieldSizeAttribute.Unlimited)]
    [RuleRequiredField("RuleRequiredField_MovimientoCajaParameters_Motivo", DefaultContexts.Save, 
        TargetCriteria = "Tipo = 'Retirada'", CustomMessageTemplate = "El motivo es obligatorio para retiradas")]
    public string? Motivo { get; set; }
}

[DomainComponent]
public class AperturaSesionParameters
{
    [XafDisplayName("Importe Real")]
    public decimal ImporteReal { get; set; }

    [XafDisplayName("Importe Teórico")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImporteTeorico { get; set; }

    [XafDisplayName("Diferencia")]
    [ModelDefault("AllowEdit", "False")]
    public decimal Diferencia => ImporteReal - ImporteTeorico;
}

[DomainComponent]
public class CierreSesionParameters
{
    [XafDisplayName("Importe Contado")]
    public decimal ImporteContado { get; set; }

    [XafDisplayName("Importe Esperado")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImporteEsperado { get; set; }

    [XafDisplayName("Diferencia Arqueo")]
    [ModelDefault("AllowEdit", "False")]
    public decimal DiferenciaArqueo => ImporteContado - ImporteEsperado;

    [XafDisplayName("Observaciones")]
    [FieldSize(FieldSizeAttribute.Unlimited)]
    public string? Observaciones { get; set; }
}
