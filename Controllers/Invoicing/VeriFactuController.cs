using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Invoicing;
using erp.Module.BusinessObjects.Settings;
using VeriFactu.Config;

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

    protected override void OnActivated()
    {
        base.OnActivated();
        
        var companyInfo = ObjectSpace.FindObject<CompanyInfo>(null);
        
        if (companyInfo == null) return;

        if (string.IsNullOrEmpty(companyInfo.VeriFactuConfigFileName)) return;
          
        Settings.SetConfigFileName(companyInfo.VeriFactuConfigFileName);
        Settings.Current.CertificateSerial = companyInfo.VeriFactuCertificateSerial;
        Settings.Current.VeriFactuEndPointPrefix = companyInfo.VeriFactuEndPointPrefix;
        Settings.Current.SistemaInformatico.NombreSistemaInformatico = companyInfo.VeriFactuSystemName;
        Settings.Current.SistemaInformatico.Version = companyInfo.VeriFactuSystemVersion;
        Settings.Current.SistemaInformatico.NombreRazon = companyInfo.VeriFactuSystemAdministratorName;
        Settings.Current.SistemaInformatico.NIF = companyInfo.VeriFactuSystemAdministratorFiscalNumber;
        Settings.Save();
    }

    protected override void OnDeactivated()
    {
        //View.ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
        base.OnDeactivated();
    }
}