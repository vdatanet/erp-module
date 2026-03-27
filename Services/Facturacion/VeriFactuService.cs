using System.Text;
using System.Xml.Linq;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Helpers.Comun;
using erp.Module.Helpers.Contactos;
using VeriFactu.Business;
using VeriFactu.Config;

namespace erp.Module.Services.Facturacion;

public class VeriFactuService
{
    private static readonly object Lock = new();

    public void SendFactura(IObjectSpace objectSpace, FacturaBase invoice)
    {
        ArgumentNullException.ThrowIfNull(objectSpace);
        ArgumentNullException.ThrowIfNull(invoice);

        lock (Lock)
        {
            var companyInfo = objectSpace.FindObject<InformacionEmpresa>(null);
            if (companyInfo == null) return;

            ConfigureVeriFactu(companyInfo);

            if (!invoice.EsValida())
                throw new UserFriendlyException(
                    "La factura no es válida para el envío a VeriFactu. Revise que tenga Cliente, Texto e Impuestos.");

            invoice.Fecha = invoice.Fecha == DateTime.MinValue
                ? InformacionEmpresaHelper.GetLocalTime(objectSpace).Date
                : invoice.Fecha;
            if (string.IsNullOrEmpty(invoice.Secuencia)) invoice.AsignarNumero();

            if (string.IsNullOrEmpty(companyInfo.Nombre) || string.IsNullOrEmpty(companyInfo.Nif))
                throw new UserFriendlyException("La información de la empresa (Nombre/NIF) es incompleta.");

            var veriFactuInvoice = MapToVeriFactuInvoice(invoice, companyInfo);
            var invoiceEntry = new InvoiceEntry(veriFactuInvoice);
            invoiceEntry.Save();

            UpdateInvoiceFromEntry(objectSpace, invoice, invoiceEntry, veriFactuInvoice);

            objectSpace.CommitChanges();

            if (invoiceEntry.Status != "Correcto")
                throw new UserFriendlyException(
                    $"Error al enviar a VeriFactu: {invoiceEntry.Status} - {invoiceEntry.ErrorCode}");
        }
    }

    private void ConfigureVeriFactu(InformacionEmpresa companyInfo)
    {
        if (string.IsNullOrEmpty(companyInfo.NombreArchivoConfigVeriFactu)) return;

        Settings.SetConfigFileName(companyInfo.NombreArchivoConfigVeriFactu);

        Settings.Current.CertificateSerial = companyInfo.SerieCertificadoVeriFactu ?? Settings.Current.CertificateSerial;
        Settings.Current.VeriFactuEndPointPrefix = companyInfo.PrefijoUrlVeriFactu ?? Settings.Current.VeriFactuEndPointPrefix;

        if (Settings.Current.SistemaInformatico != null)
        {
            Settings.Current.SistemaInformatico.NombreSistemaInformatico = companyInfo.NombreSistemaVeriFactu ?? Settings.Current.SistemaInformatico.NombreSistemaInformatico;
            Settings.Current.SistemaInformatico.Version = companyInfo.VersionSistemaVeriFactu ?? Settings.Current.SistemaInformatico.Version;
            Settings.Current.SistemaInformatico.NombreRazon = companyInfo.NombreAdministradorSistemaVeriFactu ?? Settings.Current.SistemaInformatico.NombreRazon;
            Settings.Current.SistemaInformatico.NIF = companyInfo.NifAdministradorSistemaVeriFactu ?? Settings.Current.SistemaInformatico.NIF;
            Settings.Current.SistemaInformatico.NumeroInstalacion = MachineIdentifier.GetMachineId();
        }

        Settings.Save();
    }

    private Invoice MapToVeriFactuInvoice(FacturaBase invoice, InformacionEmpresa companyInfo)
    {
        var veriFactuFactura = new Invoice(invoice.Secuencia, invoice.Fecha, companyInfo.Nif)
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
            if (tax.TipoImpuesto == null) continue;

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

    private void UpdateInvoiceFromEntry(IObjectSpace objectSpace, FacturaBase invoice, InvoiceEntry invoiceEntry,
        Invoice veriFactuFactura)
    {
        invoice.EstadoEntradaFactura = invoiceEntry.Status;
        invoice.RespuestaAgenciaTributaria = invoiceEntry.Response;
        invoice.CodigoErrorEntradaFactura = invoiceEntry.ErrorCode;

        if (invoiceEntry.Status != "Correcto") return;

        var newRecord = veriFactuFactura.GetRegistroAlta();
        invoice.UrlValidacion = newRecord.GetUrlValidate();
        invoice.Csv = invoiceEntry.CSV;

        var qr = newRecord.GetValidateQr();
        var qrMedia = objectSpace.CreateObject<MediaDataObject>();
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
}