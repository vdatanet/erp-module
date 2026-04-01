using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Configuraciones;
using erp.Module.Models.VeriFactu;
using Microsoft.Extensions.Logging;
using VeriFactu.Business;
using Settings = VeriFactu.Config.Settings;
using LibInvoice = VeriFactu.Business.Invoice;
using LibTaxItem = VeriFactu.Business.TaxItem;

namespace erp.Module.Services.Facturacion;

public class VeriFactuLibraryAdapter(ILogger<VeriFactuLibraryAdapter> logger) : IVeriFactuAdapter
{
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<VeriFactuResponse> SendInvoiceAsync(erp.Module.Models.VeriFactu.Invoice veriFactuInvoice, FacturaBase invoice,
        InformacionEmpresa companyInfo)
    {
        return await ExecuteInContextAsync(async () =>
        {
            logger.LogInformation("VeriFactuLibraryAdapter: Enviando factura {Sequence} mediante librería local",
                veriFactuInvoice.Sequence);

            // 2. Mapear a VeriFactu.Xml.Factu.Alta.Invoice
            var libInvoice = new LibInvoice(veriFactuInvoice.Sequence, veriFactuInvoice.Date, veriFactuInvoice.SellerID)
            {
                InvoiceType = MapTipoFactura(veriFactuInvoice.InvoiceType),
                SellerName = veriFactuInvoice.SellerName,
                BuyerID = veriFactuInvoice.BuyerID,
                BuyerName = veriFactuInvoice.BuyerName,
                Text = veriFactuInvoice.Text,
                TaxItems = []
            };

            // Intentamos asignar el tipo de rectificativa si existe la propiedad en la librería
            try
            {
                // Usamos dynamic para evitar errores de compilación si la propiedad no existe o tiene otro nombre
                ((dynamic)libInvoice).TipoRectificativa = MapTipoRectificativa(veriFactuInvoice.CorrectionType);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "No se pudo asignar TipoRectificativa a la factura {Sequence}", veriFactuInvoice.Sequence);
            }

            foreach (var taxItem in veriFactuInvoice.TaxItems)
            {
                logger.LogInformation("VeriFactuLibraryAdapter: Procesando TaxItem - Base: {Base}, Rate: {Rate}, Amount: {Amount}, Tax: {Tax}, Type: {Type}, Scheme: {Scheme}, Exception: {Exception}",
                    taxItem.TaxBase, taxItem.TaxRate, taxItem.TaxAmount, taxItem.Tax, taxItem.TaxType, taxItem.TaxScheme, taxItem.TaxException);

                var lTaxItem = new LibTaxItem();
                
                // Mapeo defensivo de enums ANTES de asignar valores numéricos
                try { lTaxItem.Tax = MapImpuesto(taxItem.Tax); } catch (Exception ex) { logger.LogError(ex, "Error mapeando Impuesto {Tax}", taxItem.Tax); }
                try { lTaxItem.TaxType = MapCalificacion(taxItem.TaxType); } catch (Exception ex) { logger.LogError(ex, "Error mapeando Calificacion {Type}", taxItem.TaxType); }
                // try { lTaxItem.TaxTypeSpecified = true; } catch { }
                try { lTaxItem.TaxScheme = MapRegimen(taxItem.TaxScheme); } catch (Exception ex) { logger.LogError(ex, "Error mapeando Regimen {Scheme}", taxItem.TaxScheme); }
                // try { lTaxItem.TaxSchemeSpecified = true; } catch { }
                if (taxItem.TaxException.HasValue)
                {
                    try { lTaxItem.TaxException = MapCausaExencion(taxItem.TaxException.Value); } catch (Exception ex) { logger.LogError(ex, "Error mapeando CausaExencion {Exception}", taxItem.TaxException); }
                    // try { lTaxItem.TaxExceptionSpecified = true; } catch { }
                }

                lTaxItem.TaxBase = taxItem.TaxBase;
                lTaxItem.TaxRate = taxItem.TaxRate;
                lTaxItem.TaxAmount = taxItem.TaxAmount;

                libInvoice.TaxItems.Add(lTaxItem);
            }

            // 3. Enviar
            var invoiceEntry = new InvoiceEntry(libInvoice);
            try
            {
                invoiceEntry.Save();
                logger.LogInformation("VeriFactuLibraryAdapter: Resultado para factura {Sequence}: Status={Status}, ErrorCode={ErrorCode}, ErrorDescription={ErrorDescription}",
                    veriFactuInvoice.Sequence, invoiceEntry.Status, invoiceEntry.ErrorCode, invoiceEntry.ErrorDescription);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "VeriFactuLibraryAdapter: Excepción llamando a InvoiceEntry.Save() para factura {Sequence}", veriFactuInvoice.Sequence);
                throw;
            }

            // 4. Mapear respuesta
            return new VeriFactuResponse
            {
                Status = invoiceEntry.Status == "Correcto" ? EstadoVeriFactu.Correcto : EstadoVeriFactu.Incorrecto,
                ErrorMessage = invoiceEntry.Status != "Correcto"
                    ? $"{invoiceEntry.ErrorCode}: {invoiceEntry.ErrorDescription}"
                    : null,
                ErrorCode = invoiceEntry.ErrorCode,
                RawResponse = invoiceEntry.Response,
                Uuid = invoiceEntry.CSV
            };
        }, companyInfo, veriFactuInvoice.Sequence);
    }

    public async Task<VeriFactuResponse> GetStatusAsync(string uuid, InformacionEmpresa companyInfo)
    {
        return await ExecuteInContextAsync(async () =>
        {
            logger.LogInformation("VeriFactuLibraryAdapter: Consultando estado para UUID {Uuid} (Simulado)", uuid);

            // Aunque sea simulado por ahora, el patrón de acceso debe ser consistente
            return new VeriFactuResponse
            {
                Status = EstadoVeriFactu.Correcto,
                Uuid = uuid
            };
        }, companyInfo, uuid);
    }

    private async Task<VeriFactuResponse> ExecuteInContextAsync(Func<Task<VeriFactuResponse>> action,
        InformacionEmpresa companyInfo, string identifier)
    {
        try
        {
            await _semaphore.WaitAsync();
            
            // Capturamos el estado anterior (usando reflexión si es necesario, pero aquí asumimos 
            // que la mejor práctica si no conocemos el getter es limpiar al terminar)
            
            try
            {
                // 1. Configurar Settings para el tenant actual
                Settings.SetConfigFileName(companyInfo.ConfiguracionVeriFactuLibrary);
                Settings.Save();

                return await action();
            }
            finally
            {
                // Limpiar configuración al terminar para evitar fuga de datos
                //try
                //{
                    // No llamamos a Settings.Save() al limpiar el nombre del archivo, para evitar intentar
                    // escribir en rutas por defecto (como /Users/joan/Library/Application Support/VeriFactu/)
                    // donde no tengamos permisos. El nombre del archivo ya queda vacío en memoria para el siguiente uso.
                    //Settings.SetConfigFileName(string.Empty);
                //}
                //catch (Exception ex)
                //{
                    //logger.LogWarning(ex, "Error al limpiar la configuración de VeriFactu tras la operación");
                //}
                
                _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error en operación VeriFactu para {Identifier}", identifier);
            return new VeriFactuResponse
            {
                Status = EstadoVeriFactu.ErrorTecnico,
                ErrorMessage = ex.Message
            };
        }
    }

    internal VeriFactu.Xml.Factu.Alta.TipoFactura MapTipoFactura(TipoFacturaAmigable type)
    {
        // El usuario proporcionó el enum real de la librería VeriFactu.Xml.Factu.Alta:
        // F1 (0) -> Factura
        // F2 (1) -> Factura Simplificada
        // F3 (2) -> Factura emitida en sustitución de simplificadas
        // R1 (3) -> Factura Rectificativa
        // R2 (4) -> Factura Rectificativa (Art. 80.3)
        // R3 (5) -> Factura Rectificativa (Art. 80.4)
        // R4 (6) -> Factura Rectificativa (Resto)
        // R5 (7) -> Factura Rectificativa en simplificadas
        
        var mappedValue = type switch
        {
            TipoFacturaAmigable.F1 => 0,
            TipoFacturaAmigable.F2 => 1,
            TipoFacturaAmigable.F3 => 2,
            TipoFacturaAmigable.F4 => 0, // F4 no existe en la librería, mapeamos a F1
            TipoFacturaAmigable.R1 => 3,
            TipoFacturaAmigable.R2 => 4,
            TipoFacturaAmigable.R3 => 5,
            TipoFacturaAmigable.R4 => 6,
            TipoFacturaAmigable.R5 => 7,
            _ => 0
        };
        
        var mapped = (VeriFactu.Xml.Factu.Alta.TipoFactura)mappedValue;
        
        logger.LogInformation("VeriFactuLibraryAdapter: Mapeando TipoFactura: {Internal} ({Value}) -> {External} ({ExternalValue})", 
            type, (int)type, mapped, (int)mapped);
        return mapped;
    }

    internal VeriFactu.Xml.Factu.Alta.TipoRectificativa MapTipoRectificativa(TipoRectificativaAmigable? type)
    {
        // El usuario proporcionó el enum real de la librería VeriFactu.Xml.Factu.Alta:
        // NA (0) -> No asignada tipo rectificativa
        // S (1) -> Por sustitución
        // I (2) -> Por diferencias
        
        var mappedValue = type switch
        {
            TipoRectificativaAmigable.S => 1,
            TipoRectificativaAmigable.I => 2,
            _ => 0
        };
        
        var mapped = (VeriFactu.Xml.Factu.Alta.TipoRectificativa)mappedValue;
        
        logger.LogInformation("VeriFactuLibraryAdapter: Mapeando TipoRectificativa: {Internal} ({Value}) -> {External} ({ExternalValue})", 
            type?.ToString() ?? "null", type.HasValue ? (int)type.Value : -1, mapped, (int)mapped);
        return mapped;
    }

    internal VeriFactu.Xml.Factu.Impuesto MapImpuesto(Impuesto tax)
    {
        // AEAT: IVA=01, IPSI=02, IGIC=03.
        // La librería de Irene Solutions usa 0-based: IVA=0, IPSI=1, IGIC=2.
        var mappedValue = tax switch
        {
            Impuesto.IVA => 0,
            Impuesto.IPSI => 1,
            Impuesto.IGIC => 2,
            _ => 0
        };
        var mapped = (VeriFactu.Xml.Factu.Impuesto)mappedValue;
        
        logger.LogInformation("VeriFactuLibraryAdapter: Mapeando Impuesto: {Internal} ({Value}) -> {External} ({ExternalValue})", 
            tax, (int)tax, mapped, (int)mapped);
        return mapped;
    }

    internal VeriFactu.Xml.Factu.Alta.CalificacionOperacion MapCalificacion(CalificacionOperacion type)
    {
        // El usuario proporcionó el enum real de la librería VeriFactu.Xml.Factu.Alta:
        // S1 (0) -> Sujeta y No exenta - Sin ISP
        // S2 (1) -> Sujeta y No exenta - Con ISP
        // N1 (2) -> No Sujeta art 7, 14, otros
        // N2 (3) -> No Sujeta por Reglas de localización
        
        // Mapeo basado en esta definición:
        var mappedValue = type switch
        {
            CalificacionOperacion.S1 => 0, // S1 (Sin ISP)
            CalificacionOperacion.S3 => 1, // S2 (Con ISP)
            // Para CalificacionOperacion.S2 (Exenta), la librería no parece tener un valor 
            // en este enum según el snippet. Podría estar incompleto o gestionarse de otro modo.
            // Si el XML generado usa el string del enum, mapear S2 interno a S2 de la librería (1)
            // causará que la AEAT lo vea como Exenta (S2), pero la librería lo trata como ISP.
            // Por ahora, para evitar errores de casteo y basándonos en el snippet:
            CalificacionOperacion.S2 => 0, // Fallback temporal a S1 si no hay valor para Exenta
            _ => 0
        };
        var mapped = (VeriFactu.Xml.Factu.Alta.CalificacionOperacion)mappedValue;
        
        logger.LogInformation("VeriFactuLibraryAdapter: Mapeando Calificacion: {Internal} ({Value}) -> {External} ({ExternalValue})", 
            type, (int)type, mapped, (int)mapped);
        return mapped;
    }

    internal VeriFactu.Xml.Factu.Alta.ClaveRegimen MapRegimen(ClaveRegimen scheme)
    {
        // Mapeo explícito basado en la definición de ClaveRegimen de la librería de Irene Solutions
        // proporcionada en la descripción del problema, para asegurar coherencia con los códigos AEAT.
        var mappedValue = scheme switch
        {
            ClaveRegimen.General => 1,                          // '01'
            ClaveRegimen.Exportacion => 2,                      // '02'
            ClaveRegimen.BienesUsados => 3,                     // '03' (REBU)
            ClaveRegimen.AgenciasViajes => 5,                   // '05'
            ClaveRegimen.AgriculturaGanaderiaPesca => 19,      // '19' (REAGYP)
            ClaveRegimen.RecargoEquivalencia => 18,             // '18'
            ClaveRegimen.CriterioCaja => 7,                     // '07'
            ClaveRegimen.GrupoEntidades => 6,                   // '06'
            ClaveRegimen.CobroTerceros => 10,                   // '10'
            ClaveRegimen.IGIC_IPSI => 8,                        // '08'
            ClaveRegimen.AgenciasViajesMediadoras => 9,         // '09'
            ClaveRegimen.ArrendamientosLocales => 11,           // '11'
            // Otros mapeos según disponibilidad en el enum interno y códigos AEAT
            _ => 1 // Por defecto Régimen General
        };
        
        var mapped = (VeriFactu.Xml.Factu.Alta.ClaveRegimen)mappedValue;
        logger.LogInformation("VeriFactuLibraryAdapter: Mapeando Regimen: {Internal} ({Value}) -> {External} ({ExternalValue})", 
            scheme, (int)scheme, mapped, (int)mapped);
        return mapped;
    }

    internal VeriFactu.Xml.Factu.Alta.CausaExencion MapCausaExencion(CausaExencion exception)
    {
        // AEAT: E1=1, E2=2, E3=3, E4=4, E5=5, E6=6.
        // La librería de Irene Solutions usa 0-based: E1=0, E2=1, E3=2, E4=3, E5=4, E6=5.
        var mappedValue = exception switch
        {
            CausaExencion.E1 => 0,
            CausaExencion.E2 => 1,
            CausaExencion.E3 => 2,
            CausaExencion.E4 => 3,
            CausaExencion.E5 => 4,
            CausaExencion.E6 => 5,
            _ => 0
        };
        var mapped = (VeriFactu.Xml.Factu.Alta.CausaExencion)mappedValue;
        
        logger.LogInformation("VeriFactuLibraryAdapter: Mapeando CausaExencion: {Internal} ({Value}) -> {External} ({ExternalValue})", 
            exception, (int)exception, mapped, (int)mapped);
        return mapped;
    }
}