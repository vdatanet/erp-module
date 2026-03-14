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
        if (!invoice.EsValida()) return;

        if (invoice.FechaFactura == DateTime.MinValue) invoice.FechaFactura = DateTime.Now.Date;
        if (string.IsNullOrEmpty(invoice.NumeroFactura)) invoice.ObtenerNumeroFactura();
        
        ObjectSpace.CommitChanges();
        SendInvoice(invoice);
    }

    private void SendInvoice(Invoice invoice)
    {
        var companyInfo = ObjectSpace.FindObject<CompanyInfo>(null);
        
        if (companyInfo == null) return;
        if (string.IsNullOrEmpty(companyInfo.Nombre)) return;
        if (string.IsNullOrEmpty(companyInfo.Nif)) return;
        
        var veriFactuInvoice =
            new VeriFactu.Business.Invoice(invoice.NumeroFactura, invoice.FechaFactura, companyInfo.Nif)
            {
                InvoiceType = invoice.TipoFactura,
                SellerName = companyInfo.Nombre,
                BuyerID = invoice.Cliente.Nif,
                BuyerName = invoice.Cliente.Nombre,
                Text = invoice.Texto,
                TaxItems = []
            };

        foreach (var tax in invoice.Impuestos)
        {
            if (tax.Impuesto == null) continue;

            var taxItem = new TaxItem
            {
                TaxRate = tax.Tipo,
                TaxBase = tax.BaseImponible,
                TaxAmount = tax.ImporteImpuestos,
                Tax = tax.Impuesto ?? default,
                TaxType = tax.TipoOperacion ?? default,
                TaxScheme = tax.RegimenFiscal ?? default,
                TaxException = tax.CausaExencion ?? default
            };

            veriFactuInvoice.TaxItems.Add(taxItem);
        }

        var invoiceEntry = new InvoiceEntry(veriFactuInvoice);
        invoiceEntry.Save();

        if (invoiceEntry.Status != "Correcto")
        {
            invoice.EstadoEntradaFactura = invoiceEntry.Status;
            invoice.RespuestaAgenciaTributaria = invoiceEntry.Response;
            invoice.CodigoErrorEntradaFactura = invoiceEntry.ErrorCode;
            ObjectSpace.CommitChanges();
            return;
        }

        invoice.EstadoEntradaFactura = invoiceEntry.Status;

        var newRecord = veriFactuInvoice.GetRegistroAlta();
        var validationUrl = newRecord.GetUrlValidate();
        var qr = newRecord.GetValidateQr();

        try
        {
            if (!string.IsNullOrEmpty(invoiceEntry.Response))
            {
                var response = XDocument.Parse(invoiceEntry.Response);
                invoice.RespuestaAgenciaTributaria = response.ToString();
            }
            else
            {
                invoice.RespuestaAgenciaTributaria = "No response data available";
            }

            if (invoiceEntry.Xml is { Length: > 0 })
            {
                var xmlString = Encoding.UTF8.GetString(invoiceEntry.Xml);
                var xml = XDocument.Parse(xmlString);
                invoice.XmlAgenciaTributaria = xml.ToString();
            }
            else
            {
                invoice.XmlAgenciaTributaria = "No XML data available";
            }
        }
        catch (System.Xml.XmlException ex)
        {
            invoice.RespuestaAgenciaTributaria = $"Error parsing response XML: {ex.Message}";
            invoice.XmlAgenciaTributaria = "XML parsing failed";
        }
        catch (ArgumentException ex) when (ex.ParamName == "bytes")
        {
            invoice.XmlAgenciaTributaria = $"Error decoding XML data: {ex.Message}";
        }
        catch (Exception ex)
        {
            invoice.RespuestaAgenciaTributaria = $"Unexpected error processing response: {ex.Message}";
            invoice.XmlAgenciaTributaria = "Processing failed";
        }

        invoice.EstadoVeriFactu = Invoice.ValoresEstadoVeriFactu.Enviado;
        invoice.Csv = invoiceEntry.CSV;
        invoice.UrlValidacion = validationUrl;

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

        if (string.IsNullOrEmpty(companyInfo.NombreArchivoConfigVeriFactu)) return;

        Settings.SetConfigFileName(companyInfo.NombreArchivoConfigVeriFactu);

        if (!string.IsNullOrEmpty(companyInfo.SerieCertificadoVeriFactu))
            Settings.Current.CertificateSerial = companyInfo.SerieCertificadoVeriFactu;

        if (!string.IsNullOrEmpty(companyInfo.PrefijoUrlVeriFactu))
            Settings.Current.VeriFactuEndPointPrefix = companyInfo.PrefijoUrlVeriFactu;

        if (!string.IsNullOrEmpty(companyInfo.NombreSistemaVeriFactu))
            Settings.Current.SistemaInformatico.NombreSistemaInformatico = companyInfo.NombreSistemaVeriFactu;

        if (!string.IsNullOrEmpty(companyInfo.VersionSistemaVeriFactu))
            Settings.Current.SistemaInformatico.Version = companyInfo.VersionSistemaVeriFactu;

        if (!string.IsNullOrEmpty(companyInfo.NombreAdministradorSistemaVeriFactu))
            Settings.Current.SistemaInformatico.NombreRazon = companyInfo.NombreAdministradorSistemaVeriFactu;

        if (!string.IsNullOrEmpty(companyInfo.NifAdministradorSistemaVeriFactu))
            Settings.Current.SistemaInformatico.NIF = companyInfo.NifAdministradorSistemaVeriFactu;

        Settings.Current.SistemaInformatico.NumeroInstalacion = MachineIdentifier.GetMachineId();

        Settings.Save();
    }

    protected override void OnDeactivated()
    {
        base.OnDeactivated();
    }
}