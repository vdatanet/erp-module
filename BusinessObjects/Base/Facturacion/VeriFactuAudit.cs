using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace erp.Module.BusinessObjects.Base.Facturacion;

[NavigationItem("Facturación")]
[XafDisplayName("Auditoría de Envíos VeriFactu")]
public class VeriFactuAudit(Session session) : BaseObject(session)
{
    private Guid _tenantId;
    private string _invoiceId;
    private string _numeroSerie;
    private string _nifEmisor;
    private string _estadoEnvio;
    private string _batchId;
    private DateTime _fechaEnvio;

    [XafDisplayName("ID del Tenant")]
    public Guid TenantId
    {
        get => _tenantId;
        set => SetPropertyValue(nameof(TenantId), ref _tenantId, value);
    }

    [XafDisplayName("ID de Factura (Secuencia)")]
    public string InvoiceId
    {
        get => _invoiceId;
        set => SetPropertyValue(nameof(InvoiceId), ref _invoiceId, value);
    }

    [XafDisplayName("Número de Serie")]
    public string NumeroSerie
    {
        get => _numeroSerie;
        set => SetPropertyValue(nameof(NumeroSerie), ref _numeroSerie, value);
    }

    [XafDisplayName("NIF del Emisor")]
    public string NifEmisor
    {
        get => _nifEmisor;
        set => SetPropertyValue(nameof(NifEmisor), ref _nifEmisor, value);
    }

    [XafDisplayName("Estado del Envío")]
    public string EstadoEnvio
    {
        get => _estadoEnvio;
        set => SetPropertyValue(nameof(EstadoEnvio), ref _estadoEnvio, value);
    }

    [XafDisplayName("ID de Lote (Batch/Transaction)")]
    public string BatchId
    {
        get => _batchId;
        set => SetPropertyValue(nameof(BatchId), ref _batchId, value);
    }

    [XafDisplayName("Fecha de Envío")]
    public DateTime FechaEnvio
    {
        get => _fechaEnvio;
        set => SetPropertyValue(nameof(FechaEnvio), ref _fechaEnvio, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        FechaEnvio = DateTime.Now;
        EstadoEnvio = "Encolada";
    }
}
