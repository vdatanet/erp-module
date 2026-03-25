using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Base.Ventas;
using System.Security.Cryptography.X509Certificates;
using FACe.Xml.Facturae.Bies;

namespace erp.Module.Services.Facturacion;

public class FacturaeService : IFacturaeService
{
    public string GenerateFacturaeXml(IObjectSpace objectSpace, FacturaBase invoice)
    {
        var companyInfo = objectSpace.FindObject<InformacionEmpresa>(null);
        if (companyInfo == null) throw new UserFriendlyException("No se ha configurado la información de la empresa.");

        var f = MapToFacturae(invoice, companyInfo);
        var manager = new FACe.Xml.Facturae.FacturaeManager(f);
        return manager.GetUTF8XmlText();
    }

    public byte[] GenerateSignedFacturae(IObjectSpace objectSpace, FacturaBase invoice)
    {
        var companyInfo = objectSpace.FindObject<InformacionEmpresa>(null);
        if (companyInfo == null) throw new UserFriendlyException("No se ha configurado la información de la empresa.");

        if (string.IsNullOrEmpty(companyInfo.SerieCertificadoFacturae))
            throw new UserFriendlyException("No se ha configurado el certificado para Facturae.");

        var f = MapToFacturae(invoice, companyInfo);

        X509Certificate2? cert = LoadCertificate(companyInfo.SerieCertificadoFacturae);
        if (cert == null)
            throw new UserFriendlyException($"No se ha encontrado el certificado con serie {companyInfo.SerieCertificadoFacturae}");

        var manager = new FACe.Xml.Facturae.FacturaeManager(f);
        var xmlSigned = manager.GetXmlTextSigned(cert);
        return System.Text.Encoding.UTF8.GetBytes(xmlSigned);
    }

    private Facturae MapToFacturae(FacturaBase invoice, InformacionEmpresa company)
    {
        var f = new Facturae();

        // 1. Cabecera
        f.FileHeader.SchemaVersion = SchemaVersion.Ver322;
        f.FileHeader.Modality = Modality.I; // Individual
        f.FileHeader.InvoiceIssuerType = InvoiceIssuerType.EM; // Emisor

        // 2. Emisor
        f.Parties.SellerParty.TaxIdentification.TaxIdentificationNumber = company.Nif;
        f.Parties.SellerParty.TaxIdentification.PersonTypeCode = company.Nif?.Length == 9 && char.IsLetter(company.Nif[0]) ? PersonTypeCode.J : PersonTypeCode.F;

        var sellerLegalEntity = new LegalEntity();
        sellerLegalEntity.CorporateName = company.Nombre;
        var sellerAddress = new Address();
        sellerAddress.AddressText = company.Direccion;
        sellerAddress.PostCode = company.CodigoPostal;
        sellerAddress.Town = company.Poblacion?.Nombre;
        sellerAddress.Province = company.Provincia?.Nombre;
        sellerAddress.CountryCode = Country.ESP;
        sellerLegalEntity.Address = sellerAddress;
        f.Parties.SellerParty.Party = sellerLegalEntity;

        // 3. Receptor
        var cliente = invoice.Cliente;
        if (cliente == null) throw new UserFriendlyException("La factura no tiene cliente asignado.");

        f.Parties.BuyerParty.TaxIdentification.TaxIdentificationNumber = cliente.Nif;
        f.Parties.BuyerParty.TaxIdentification.PersonTypeCode = cliente.Nif?.Length == 9 && char.IsLetter(cliente.Nif[0]) ? PersonTypeCode.J : PersonTypeCode.F;

        var buyerLegalEntity = new LegalEntity();
        buyerLegalEntity.CorporateName = cliente.Nombre;
        var buyerAddress = new Address();
        buyerAddress.AddressText = cliente.Direccion;
        buyerAddress.PostCode = cliente.CodigoPostal;
        buyerAddress.Town = cliente.Poblacion?.Nombre;
        buyerAddress.Province = cliente.Provincia?.Nombre;
        buyerAddress.CountryCode = Country.ESP;
        buyerLegalEntity.Address = buyerAddress;
        f.Parties.BuyerParty.Party = buyerLegalEntity;

        // Unidades Orgánicas (FACe)
        var centres = new System.Collections.Generic.List<AdministrativeCentre>();
        if (!string.IsNullOrEmpty(company.UnidadOrganicaOficinaContable))
        {
            centres.Add(new AdministrativeCentre
            {
                CentreCode = company.UnidadOrganicaOficinaContable,
                RoleTypeCode = RoleTypeCode.Fiscal // Oficina Contable
            });
        }
        if (!string.IsNullOrEmpty(company.UnidadOrganicaOrganoGestor))
        {
            centres.Add(new AdministrativeCentre
            {
                CentreCode = company.UnidadOrganicaOrganoGestor,
                RoleTypeCode = RoleTypeCode.Receiver // Órgano Gestor
            });
        }
        if (!string.IsNullOrEmpty(company.UnidadOrganicaUnidadTramitadora))
        {
            centres.Add(new AdministrativeCentre
            {
                CentreCode = company.UnidadOrganicaUnidadTramitadora,
                RoleTypeCode = RoleTypeCode.Payer // Unidad Tramitadora
            });
        }
        if (centres.Count > 0)
            f.Parties.BuyerParty.AdministrativeCentres = centres.ToArray();

        // 4. Detalle de factura
        var inv = new Invoice();
        inv.InvoiceHeader.InvoiceNumber = invoice.Secuencia ?? invoice.Numero.ToString();
        inv.InvoiceHeader.InvoiceSeriesCode = invoice.Serie;
        inv.InvoiceHeader.InvoiceDocumentType = InvoiceDocumentType.FC; // Factura Completa
        inv.InvoiceHeader.InvoiceClass = InvoiceClass.OO; // Original

        inv.InvoiceIssueData.IssueDate = invoice.Fecha;
        inv.InvoiceIssueData.InvoiceCurrencyCode = CurrencyCode.EUR;
        inv.InvoiceIssueData.TaxCurrencyCode = CurrencyCode.EUR;
        inv.InvoiceIssueData.LanguageName = LanguageCode.es;

        // Totales
        inv.InvoiceTotals.TotalGrossAmount = (decimal)invoice.TotalBruto;
        inv.InvoiceTotals.TotalGeneralDiscounts = 0m;
        inv.InvoiceTotals.TotalGeneralSurcharges = 0m;
        inv.InvoiceTotals.TotalGrossAmountBeforeTaxes = (decimal)invoice.BaseImponible;
        inv.InvoiceTotals.TotalTaxOutputs = (decimal)invoice.ImporteImpuestos;
        inv.InvoiceTotals.TotalTaxesWithheld = 0m;
        inv.InvoiceTotals.InvoiceTotal = (decimal)invoice.ImporteTotal;
        inv.InvoiceTotals.TotalOutstandingAmount = (decimal)invoice.ImporteTotal;
        inv.InvoiceTotals.TotalExecutableAmount = (decimal)invoice.ImporteTotal;

        // Impuestos
        var taxOutputs = new System.Collections.Generic.List<TaxOutput>();
        foreach (var taxGroup in invoice.Impuestos.GroupBy(t => t.Tipo))
        {
            var tax = new TaxOutput();
            tax.TaxTypeCode = TaxTypeCode.IVA; // IVA
            tax.TaxRate = (decimal)taxGroup.Key;
            tax.TaxableBase = new Amount { TotalAmount = (decimal)taxGroup.Sum(t => t.BaseImponible) };
            tax.TaxAmount = new Amount { TotalAmount = (decimal)taxGroup.Sum(t => t.ImporteImpuestos) };
            taxOutputs.Add(tax);
        }
        inv.TaxesOutputs = taxOutputs.ToArray();

        // Líneas
        var items = new System.Collections.Generic.List<InvoiceLine>();
        foreach (var line in invoice.Lineas)
        {
            var l = new InvoiceLine();
            l.ItemDescription = line.NombreProducto;
            l.Quantity = (decimal)line.Cantidad;
            l.UnitOfMeasure = UnitOfMeasure.Units; // Unidades
            l.UnitPriceWithoutTax = (decimal)line.PrecioUnitario;
            l.TotalCost = (decimal)line.BaseImponible;
            l.GrossAmount = (decimal)(line.Cantidad * line.PrecioUnitario);

            if (line.PorcentajeDescuento > 0)
            {
                var disc = new Discount();
                disc.DiscountReason = "Descuento comercial";
                disc.DiscountRate = (decimal)line.PorcentajeDescuento;
                disc.DiscountAmount = (decimal)(line.Cantidad * line.PrecioUnitario * (line.PorcentajeDescuento / 100.0m));
                l.DiscountsAndRebates = new[] { disc };
            }

            var lineTaxes = new System.Collections.Generic.List<Tax>();
            foreach (var tax in line.TiposImpuestoVenta)
            {
                var lt = new Tax();
                lt.TaxTypeCode = TaxTypeCode.IVA; // IVA
                lt.TaxRate = (decimal)tax.Tipo;
                lineTaxes.Add(lt);
            }
            l.TaxesOutputs = lineTaxes.ToArray();

            items.Add(l);
        }

        inv.Items = items.ToArray();
        f.Invoices = new[] { inv };

        return f;
    }

    private X509Certificate2? LoadCertificate(string serialNumber)
    {
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);
        var certs = store.Certificates.Find(X509FindType.FindBySerialNumber, serialNumber, false);
        return certs.Count > 0 ? certs[0] : null;
    }
}
