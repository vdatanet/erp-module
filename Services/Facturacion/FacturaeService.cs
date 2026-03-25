using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.BusinessObjects.Base.Ventas;
using System.Security.Cryptography.X509Certificates;
using FACe;

namespace erp.Module.Services.Facturacion;

public class FacturaeService : IFacturaeService
{
    public string GenerateFacturaeXml(IObjectSpace objectSpace, FacturaBase invoice)
    {
        var companyInfo = objectSpace.FindObject<InformacionEmpresa>(null);
        if (companyInfo == null) throw new UserFriendlyException("No se ha configurado la información de la empresa.");

        var f = MapToFacturae(invoice, companyInfo);
        return f.GetXML();
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

        return f.Sign(cert);
    }

    private Facturae MapToFacturae(FacturaBase invoice, InformacionEmpresa company)
    {
        var f = new Facturae();

        // 1. Cabecera
        f.FileHeader.SchemaVersion = SchemaVersionType.Item322;
        f.FileHeader.Modality = ModalityType.I; // Individual
        f.FileHeader.InvoiceIssuerType = InvoiceIssuerTypeType.EM; // Emisor

        // 2. Emisor
        f.Parties.SellerParty.TaxIdentification.TaxIdentificationNumber = company.Nif;
        f.Parties.SellerParty.TaxIdentification.PersonTypeCode = company.Nif?.Length == 9 && char.IsLetter(company.Nif[0]) ? PersonTypeCodeType.J : PersonTypeCodeType.F;
        
        f.Parties.SellerParty.LegalEntity.CorporateName = company.Nombre;
        f.Parties.SellerParty.LegalEntity.AddressInSpain.Address = company.Direccion;
        f.Parties.SellerParty.LegalEntity.AddressInSpain.PostCode = company.CodigoPostal;
        f.Parties.SellerParty.LegalEntity.AddressInSpain.Town = company.Poblacion?.Nombre;
        f.Parties.SellerParty.LegalEntity.AddressInSpain.Province = company.Provincia?.Nombre;
        f.Parties.SellerParty.LegalEntity.AddressInSpain.CountryCode = CountryCodeType.ESP;

        // 3. Receptor
        var cliente = invoice.Cliente;
        if (cliente == null) throw new UserFriendlyException("La factura no tiene cliente asignado.");

        f.Parties.BuyerParty.TaxIdentification.TaxIdentificationNumber = cliente.Nif;
        f.Parties.BuyerParty.TaxIdentification.PersonTypeCode = cliente.Nif?.Length == 9 && char.IsLetter(cliente.Nif[0]) ? PersonTypeCodeType.J : PersonTypeCodeType.F;

        f.Parties.BuyerParty.LegalEntity.CorporateName = cliente.Nombre;
        f.Parties.BuyerParty.LegalEntity.AddressInSpain.Address = cliente.Direccion;
        f.Parties.BuyerParty.LegalEntity.AddressInSpain.PostCode = cliente.CodigoPostal;
        f.Parties.BuyerParty.LegalEntity.AddressInSpain.Town = cliente.Poblacion?.Nombre;
        f.Parties.BuyerParty.LegalEntity.AddressInSpain.Province = cliente.Provincia?.Nombre;
        f.Parties.BuyerParty.LegalEntity.AddressInSpain.CountryCode = CountryCodeType.ESP;

        // Unidades Orgánicas (FACe)
        if (!string.IsNullOrEmpty(company.UnidadOrganicaOficinaContable))
        {
            f.Parties.BuyerParty.AdministrativeCentres.Add(new AdministrativeCentreType
            {
                CentreCode = company.UnidadOrganicaOficinaContable,
                RoleTypeCode = RoleTypeCodeType.Item01 // Oficina Contable
            });
        }
        if (!string.IsNullOrEmpty(company.UnidadOrganicaOrganoGestor))
        {
            f.Parties.BuyerParty.AdministrativeCentres.Add(new AdministrativeCentreType
            {
                CentreCode = company.UnidadOrganicaOrganoGestor,
                RoleTypeCode = RoleTypeCodeType.Item02 // Órgano Gestor
            });
        }
        if (!string.IsNullOrEmpty(company.UnidadOrganicaUnidadTramitadora))
        {
            f.Parties.BuyerParty.AdministrativeCentres.Add(new AdministrativeCentreType
            {
                CentreCode = company.UnidadOrganicaUnidadTramitadora,
                RoleTypeCode = RoleTypeCodeType.Item03 // Unidad Tramitadora
            });
        }

        // 4. Detalle de factura
        var inv = new InvoiceType();
        inv.InvoiceHeader.InvoiceNumber = invoice.Secuencia ?? invoice.Numero.ToString();
        inv.InvoiceHeader.InvoiceSeriesCode = invoice.Serie;
        inv.InvoiceHeader.InvoiceDocumentType = InvoiceDocumentTypeType.FE; // Factura Electrónica
        inv.InvoiceHeader.InvoiceClass = InvoiceClassType.OO; // Original

        inv.InvoiceIssueData.IssueDate = invoice.Fecha;
        inv.InvoiceIssueData.InvoiceCurrencyCode = CurrencyCodeType.EUR;
        inv.InvoiceIssueData.TaxCurrencyCode = CurrencyCodeType.EUR;
        inv.InvoiceIssueData.LanguageName = LanguageCodeType.es;

        // Totales
        inv.InvoiceTotals.TotalGrossAmount = (double)invoice.TotalBruto;
        inv.InvoiceTotals.TotalTaxOutputs = (double)invoice.ImporteImpuestos;
        inv.InvoiceTotals.TotalGrossAmountBeforeTaxes = (double)invoice.BaseImponible;
        inv.InvoiceTotals.InvoiceTotalAmount = (double)invoice.ImporteTotal;
        inv.InvoiceTotals.TotalOutstandingAmount = (double)invoice.ImporteTotal;
        inv.InvoiceTotals.TotalExecutableAmount = (double)invoice.ImporteTotal;

        // Impuestos
        foreach (var taxGroup in invoice.Impuestos.GroupBy(t => t.Tipo))
        {
            var tax = new TaxOutputType();
            tax.TaxTypeCode = TaxTypeCodeType.Item01; // IVA
            tax.TaxRate = (double)taxGroup.Key;
            tax.TaxableBase.TotalAmount = (double)taxGroup.Sum(t => t.BaseImponible);
            tax.TaxAmount.TotalAmount = (double)taxGroup.Sum(t => t.ImporteImpuestos);
            inv.TaxesOutputs.Add(tax);
        }

        // Líneas
        foreach (var line in invoice.Lineas)
        {
            var l = new InvoiceLineType();
            l.ItemDescription = line.NombreProducto;
            l.Quantity = (double)line.Cantidad;
            l.UnitOfMeasure = UnitOfMeasureType.Item01; // Unidades
            l.UnitPriceWithoutTax = (double)line.PrecioUnitario;
            l.TotalCost = (double)line.BaseImponible;
            l.GrossAmount = (double)(line.Cantidad * line.PrecioUnitario);
            
            if (line.PorcentajeDescuento > 0)
            {
                var disc = new DiscountType();
                disc.DiscountReason = "Descuento comercial";
                disc.DiscountRate = (double)line.PorcentajeDescuento;
                disc.DiscountAmount = (double)(line.Cantidad * line.PrecioUnitario * (line.PorcentajeDescuento / 100.0m));
                l.DiscountsAndRebates.Add(disc);
            }

            foreach (var tax in line.TiposImpuestoVenta)
            {
                var lt = new TaxType();
                lt.TaxTypeCode = TaxTypeCodeType.Item01; // IVA
                lt.TaxRate = (double)tax.Tipo;
                l.TaxesOutputs.Add(lt);
            }

            inv.Items.Add(l);
        }

        f.Invoices.Add(inv);

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
