using System.Text;
using System.Xml;
using System.Xml.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using erp.Module.BusinessObjects.Configuracion;
using erp.Module.Helpers.Comun;
using VeriFactu.Business;
using VeriFactu.Config;
using FacturaBase = erp.Module.BusinessObjects.Facturacion.FacturaBase;

namespace erp.Module.Controllers.Invoicing;

public class VeriFactuController : ViewController
{
    public VeriFactuController()
    {
        TargetObjectType = typeof(FacturaBase);
        TargetViewType = ViewType.Any;

        var validateFactura = new SimpleAction(this, "ValidateFactura", PredefinedCategory.View)
        {
            Caption = "Validar",
            ImageName = "Action_Validation_Validate",
            TargetViewType = ViewType.DetailView
        };
        validateFactura.Execute += ValidateFactura_Execute;
    }

    private void ValidateFactura_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        if (View.CurrentObject is not FacturaBase invoice) return;

        if (!invoice.EsValida())
        {
            throw new UserFriendlyException("La factura no es válida para el envío a VeriFactu. Revise que tenga Cliente, Texto e Impuestos.");
        }

        if (invoice.Fecha == DateTime.MinValue) invoice.Fecha = DateTime.Now.Date;
        if (string.IsNullOrEmpty(invoice.Numero)) invoice.AsignarNumero();

        ObjectSpace.CommitChanges();
        SendFactura(invoice);
    }

    private void SendFactura(FacturaBase invoice)
    {
        var companyInfo = ObjectSpace.FindObject<InformacionEmpresa>(null);

        if (companyInfo == null || string.IsNullOrEmpty(companyInfo.Nombre) || string.IsNullOrEmpty(companyInfo.Nif))
        {
            throw new UserFriendlyException("La información de la empresa (Nombre/NIF) es incompleta.");
        }

        var veriFactuInvoice = MapToVeriFactuInvoice(invoice, companyInfo);
        var invoiceEntry = new InvoiceEntry(veriFactuInvoice);
        invoiceEntry.Save();

        UpdateInvoiceFromEntry(invoice, invoiceEntry, veriFactuInvoice);
        
        ObjectSpace.CommitChanges();

        if (invoiceEntry.Status == "Correcto")
        {
            MessageOptions options = new MessageOptions
            {
                Duration = 2000,
                Message = "Factura enviada correctamente a VeriFactu",
                Type = InformationType.Success,
                Web = { Position = InformationPosition.Right }
            };
            Application.ShowViewStrategy.ShowMessage(options);
        }
        else
        {
            throw new UserFriendlyException($"Error al enviar a VeriFactu: {invoiceEntry.Status} - {invoiceEntry.ErrorCode}");
        }
    }

    private Invoice MapToVeriFactuInvoice(FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        var veriFactuFactura = new Invoice(invoice.Numero, invoice.Fecha, companyInfo.Nif)
        {
            InvoiceType = invoice.TipoFactura,
            SellerName = companyInfo.Nombre,
            BuyerID = invoice.Cliente?.Nif,
            BuyerName = invoice.Cliente?.Nombre,
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

            veriFactuFactura.TaxItems.Add(taxItem);
        }

        return veriFactuFactura;
    }

    private void UpdateInvoiceFromEntry(FacturaBase invoice, InvoiceEntry invoiceEntry, Invoice veriFactuFactura)
    {
        invoice.EstadoEntradaFactura = invoiceEntry.Status;
        invoice.RespuestaAgenciaTributaria = invoiceEntry.Response;
        invoice.CodigoErrorEntradaFactura = invoiceEntry.ErrorCode;

        if (invoiceEntry.Status != "Correcto") return;

        var newRecord = veriFactuFactura.GetRegistroAlta();
        invoice.UrlValidacion = newRecord.GetUrlValidate();
        invoice.Csv = invoiceEntry.CSV;

        var qr = newRecord.GetValidateQr();
        var qrMedia = ObjectSpace.CreateObject<MediaDataObject>();
        qrMedia.MediaData = qr;
        invoice.Qr = qrMedia;

        invoice.EstadoVeriFactu = FacturaBase.ValoresEstadoVeriFactu.Enviado;

        try
        {
            if (!string.IsNullOrEmpty(invoiceEntry.Response))
            {
                var response = XDocument.Parse(invoiceEntry.Response);
                invoice.RespuestaAgenciaTributaria = response.ToString();
            }

            if (invoiceEntry.Xml is { Length: > 0 })
            {
                var xmlString = Encoding.UTF8.GetString(invoiceEntry.Xml);
                var xml = XDocument.Parse(xmlString);
                invoice.XmlAgenciaTributaria = xml.ToString();
            }
        }
        catch (Exception ex)
        {
            invoice.RespuestaAgenciaTributaria += $"\nError processing XML detail: {ex.Message}";
        }
    }

    protected override void OnActivated()
    {
        base.OnActivated();

        var companyInfo = ObjectSpace.FindObject<InformacionEmpresa>(null);

        if (companyInfo == null || string.IsNullOrEmpty(companyInfo.NombreArchivoConfigVeriFactu)) return;

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