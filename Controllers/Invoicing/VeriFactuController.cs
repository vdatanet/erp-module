using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Settings;
using VeriFactu.Business;
using VeriFactu.Config;
using Invoice = erp.Module.BusinessObjects.Invoicing.Invoice;

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
        sendVeriFactuInvoice.Execute += SendVeriFactuInvoice_Execute;
    }

    private void SendVeriFactuInvoice_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var invoice = View.CurrentObject as Invoice;
        if (invoice == null)
        {
            return;
        }

        SendInvoice(invoice);
    }

    private void SendInvoice(Invoice invoice)
    {
        var companyInfo = ObjectSpace.FindObject<CompanyInfo>(null);
        
        var veriFactuInvoice =
            new VeriFactu.Business.Invoice(invoice.InvoiceNumber, invoice.InvoiceDate, companyInfo.VatNumber)
            {
                InvoiceType = invoice.InvoiceType,
                SellerName = companyInfo.Name,
                BuyerID = invoice.Customer.VatNumber,
                BuyerName = invoice.Customer.Name,
                Text = invoice.Text,
                TaxItems =
                [
                    new TaxItem()
                    {
                        TaxRate = 4,
                        TaxBase = 10,
                        TaxAmount = 0.4m
                    },

                    new TaxItem()
                    {
                        TaxRate = 21,
                        TaxBase = 100,
                        TaxAmount = 21
                    }
                ]
            };
        
        var invoiceEntry = new InvoiceEntry(veriFactuInvoice);
        invoiceEntry.Save();

        if (invoiceEntry.Status == "Correcto")
        {
            invoice.Csv = invoiceEntry.CSV;
            invoice.Save();
            ObjectSpace.CommitChanges();
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        var companyInfo = ObjectSpace.FindObject<CompanyInfo>(null);

        if (companyInfo == null) return;

        if (string.IsNullOrEmpty(companyInfo.VeriFactuConfigFileName)) return;

        Settings.SetConfigFileName(companyInfo.VeriFactuConfigFileName);
        
        if (!string.IsNullOrEmpty(companyInfo.VeriFactuCertificateSerial))
            Settings.Current.CertificateSerial = companyInfo.VeriFactuCertificateSerial;
            
        if (!string.IsNullOrEmpty(companyInfo.VeriFactuEndPointPrefix))
            Settings.Current.VeriFactuEndPointPrefix = companyInfo.VeriFactuEndPointPrefix;
            
        if (!string.IsNullOrEmpty(companyInfo.VeriFactuSystemName))
            Settings.Current.SistemaInformatico.NombreSistemaInformatico = companyInfo.VeriFactuSystemName;
            
        if (!string.IsNullOrEmpty(companyInfo.VeriFactuSystemVersion))
            Settings.Current.SistemaInformatico.Version = companyInfo.VeriFactuSystemVersion;
            
        if (!string.IsNullOrEmpty(companyInfo.VeriFactuSystemAdministratorName))
            Settings.Current.SistemaInformatico.NombreRazon = companyInfo.VeriFactuSystemAdministratorName;
            
        if (!string.IsNullOrEmpty(companyInfo.VeriFactuSystemAdministratorFiscalNumber))
            Settings.Current.SistemaInformatico.NIF = companyInfo.VeriFactuSystemAdministratorFiscalNumber;
            
        Settings.Save();
    }

    protected override void OnDeactivated()
    {
        //View.ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
        base.OnDeactivated();
    }
}