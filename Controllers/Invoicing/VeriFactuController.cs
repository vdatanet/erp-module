using System.Text;
using System.Xml.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using erp.Module.BusinessObjects.Settings;
using erp.Module.Helpers.Common;
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

        var validateInvoice = new SimpleAction(this, "ValidateInvoice", PredefinedCategory.View)
        {
            //Specify the Action's button caption.
            Caption = "Validate",
            //Specify the confirmation message that pops up when a user executes an Action.
            //ConfirmationMessage = "Are you sure you want to send the Invoice to the Tax Agency?",
            //Specify the icon of the Action's button in the interface.
            ImageName = "Action_Validation_Validate",
            TargetViewType = ViewType.DetailView
        };
        validateInvoice.Execute += ValidateInvoice_Execute;

        //var cancelVeriFactuInvoice = new SimpleAction(this, "CancelVeriFactuInvoice", PredefinedCategory.View)
        //{
        //    //Specify the Action's button caption.
        //    Caption = "Cancel VeriFactu",
        //    //Specify the confirmation message that pops up when a user executes an Action.
        //    ConfirmationMessage = "Are you sure you want to cancel the Invoice to the Tax Agency?",
        //    //Specify the icon of the Action's button in the interface.
        //    ImageName = "Cancel",
        //    TargetViewType = ViewType.DetailView
        //};
        //cancelVeriFactuInvoice.Execute += CancelVeriFactuInvoice_Execute;
    }

    private void CancelVeriFactuInvoice_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        //ObjectSpace.CommitChanges();

        //if (View.CurrentObject is not Invoice invoice) return;

        //if (invoice.VeriFactuStatus == Invoice.VeriFactuStatusValues.Send)
        //    CancelInvoice(invoice);
    }

    private void CancelInvoice(Invoice invoice)
    {
        // var companyInfo = ObjectSpace.FindObject<CompanyInfo>(null);
        //
        // var veriFactuInvoice =
        //     new VeriFactu.Business.Invoice(invoice.InvoiceNumber, invoice.InvoiceDate, companyInfo.VatNumber)
        //     {
        //         SellerName = companyInfo.Name
        //     };
        // var invoiceCancellation = new InvoiceCancellation(veriFactuInvoice);
        // invoiceCancellation.Save();
        // invoice.TaxAgencyResponse = invoiceCancellation.Response;
        // invoice.VeriFactuStatus = Invoice.VeriFactuStatusValues.Draft;
        // invoice.InvoiceEntryStatus = invoiceCancellation.Status;
        // invoice.Csv = null;
        // invoice.ValidationUrl = null;
        // invoice.Qr = null;
        // ObjectSpace.CommitChanges();
    }

    private void ValidateInvoice_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (View.CurrentObject is not Invoice invoice) return;
        if (!invoice.IsValid()) return;

        if (invoice.InvoiceDate == DateTime.MinValue) invoice.InvoiceDate = DateTime.Now.Date;
        if (string.IsNullOrEmpty(invoice.InvoiceNumber)) invoice.GetInvoiceNumber();
        
        ObjectSpace.CommitChanges();
        SendInvoice(invoice);
    }

    private void SendInvoice(Invoice invoice)
    {
        var companyInfo = ObjectSpace.FindObject<CompanyInfo>(null);

        return;

        var veriFactuInvoice =
            new VeriFactu.Business.Invoice(invoice.InvoiceNumber, invoice.InvoiceDate, companyInfo.VatNumber)
            {
                InvoiceType = invoice.InvoiceType,
                SellerName = companyInfo.Name,
                BuyerID = invoice.Customer.VatNumber,
                BuyerName = invoice.Customer.Name,
                Text = invoice.Text,
                TaxItems = []
            };

        foreach (var tax in invoice.Taxes)
        {
            if (tax.Tax == null) continue;

            var taxItem = new TaxItem
            {
                TaxRate = tax.Rate,
                TaxBase = tax.TaxableAmount,
                TaxAmount = tax.TaxAmount,
                Tax = tax.Tax ?? default,
                TaxType = tax.TaxType ?? default,
                TaxScheme = tax.TaxScheme ?? default,
                TaxException = tax.TaxException ?? default
            };

            veriFactuInvoice.TaxItems.Add(taxItem);
        }

        var invoiceEntry = new InvoiceEntry(veriFactuInvoice);
        invoiceEntry.Save();

        if (invoiceEntry.Status != "Correcto")
        {
            invoice.InvoiceEntryStatus = invoiceEntry.Status;
            invoice.TaxAgencyResponse = invoiceEntry.Response;
            invoice.InvoiceEntryErrorCode = invoiceEntry.ErrorCode;
            ObjectSpace.CommitChanges();
            return;
        }

        invoice.InvoiceEntryStatus = invoiceEntry.Status;

        var newRecord = veriFactuInvoice.GetRegistroAlta();
        var validationUrl = newRecord.GetUrlValidate();
        var qr = newRecord.GetValidateQr();

        try
        {
            if (!string.IsNullOrEmpty(invoiceEntry.Response))
            {
                var response = XDocument.Parse(invoiceEntry.Response);
                invoice.TaxAgencyResponse = response.ToString();
            }
            else
            {
                invoice.TaxAgencyResponse = "No response data available";
            }

            if (invoiceEntry.Xml is { Length: > 0 })
            {
                var xmlString = Encoding.UTF8.GetString(invoiceEntry.Xml);
                var xml = XDocument.Parse(xmlString);
                invoice.TaxAgencyXml = xml.ToString();
            }
            else
            {
                invoice.TaxAgencyXml = "No XML data available";
            }
        }
        catch (System.Xml.XmlException ex)
        {
            invoice.TaxAgencyResponse = $"Error parsing response XML: {ex.Message}";
            invoice.TaxAgencyXml = "XML parsing failed";
        }
        catch (ArgumentException ex) when (ex.ParamName == "bytes")
        {
            invoice.TaxAgencyXml = $"Error decoding XML data: {ex.Message}";
        }
        catch (Exception ex)
        {
            invoice.TaxAgencyResponse = $"Unexpected error processing response: {ex.Message}";
            invoice.TaxAgencyXml = "Processing failed";
        }

        invoice.VeriFactuStatus = Invoice.VeriFactuStatusValues.Send;
        invoice.Csv = invoiceEntry.CSV;
        invoice.ValidationUrl = validationUrl;

        var qrMedia = ObjectSpace.CreateObject<MediaDataObject>();
        qrMedia.MediaData = qr;
        invoice.Qr = qrMedia;

        ObjectSpace.CommitChanges();
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

        Settings.Current.SistemaInformatico.NumeroInstalacion = MachineIdentifier.GetMachineId();

        Settings.Save();
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
    }
}