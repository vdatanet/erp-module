using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Invoicing;
using erp.Module.Helpers.Contacts;

namespace erp.Module.Controllers.Invoicing;

public class VeriFactuController : ViewController
{
    public VeriFactuController()
    {
        TargetObjectType = typeof(Invoice);
        TargetViewType = ViewType.Any;

        SimpleAction sendVeriFactuInvoice = new SimpleAction(this, "SendVeriFactuInvoice", PredefinedCategory.View)
        {
            //Specify the Action's button caption.
            Caption = "Send VeriFactu",
            //Specify the confirmation message that pops up when a user executes an Action.
            ConfirmationMessage = "Are you sure you want to send the Invoice to the Tax Agency?",
            //Specify the icon of the Action's button in the interface.
            ImageName = "Actions_Send",
            TargetViewType = ViewType.DetailView
        };
    }

    // [ActivatorUtilitiesConstructor]
    // public SalesLineController(
    //     ISalesLineService lineService,
    //     ISalesDocumentService documentService) : this()
    // {
    //     _lineService = lineService;
    //     _documentService = documentService;
    // }
    //
    // private readonly ISalesLineService _lineService;
    // private readonly ISalesDocumentService _documentService;

    protected override void OnActivated()
    {
        base.OnActivated();

        //var companyInfo = CompanyInfoHelper.GetCompanyInfo(Session);
        //if (companyInfo == null) return;

        // if (configuracion != null && (!string.IsNullOrEmpty(configuracion.ConfigFileName)))
        // {
        //     Settings.SetConfigFileName(configuracion.ConfigFileName);
        //
        //     if (!string.IsNullOrEmpty(configuracion.NumeroSerieCertificado)) Settings.Current.CertificateSerial = configuracion.NumeroSerieCertificado;
        //
        //     if (!string.IsNullOrEmpty(configuracion.VeriFactuSOAP)) Settings.Current.VeriFactuEndPointPrefix = configuracion.VeriFactuSOAP;
        //     if (!string.IsNullOrEmpty(configuracion.VeriFactuValidarQR)) Settings.Current.VeriFactuEndPointValidatePrefix = configuracion.VeriFactuValidarQR;
        //
        //     if (!string.IsNullOrEmpty(configuracion.NombreSistemaInformatico)) Settings.Current.SistemaInformatico.NombreSistemaInformatico = configuracion.NombreSistemaInformatico;
        //     if (!string.IsNullOrEmpty(configuracion.VersionSistemaInformatico)) Settings.Current.SistemaInformatico.Version = configuracion.VersionSistemaInformatico;
        //     if (!string.IsNullOrEmpty(configuracion.NombreResponsableSistemaInformatico)) Settings.Current.SistemaInformatico.NombreRazon = configuracion.NombreResponsableSistemaInformatico;
        //     if (!string.IsNullOrEmpty(configuracion.NifResponsableSistemaInformatico)) Settings.Current.SistemaInformatico.NIF = configuracion.NifResponsableSistemaInformatico;
        //
        //     Settings.Save();
        // }

    }

    protected override void OnDeactivated()
    {
        //View.ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
        base.OnDeactivated();
    }
}