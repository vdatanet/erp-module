using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.ExpressApp;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Tpv;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.Helpers.Contactos;
using erp.Module.Services.Contabilidad;
using erp.Module.Services.Facturacion;
using erp.Module.Services.Tesoreria;
using Microsoft.Extensions.Logging;

namespace erp.Module.Services.Ventas;

public class FacturaOrchestrator(ILogger<FacturaOrchestrator>? logger = null)
{
    public record BatchResult(int Total, int Success, string LastErrorMessage = "", List<string>? ErrorMessages = null)
    {
        public bool AllSuccessful => Total == Success;
    }

    public void Validar(FacturaBase factura)
    {
        if (factura == null) return;

        if (!factura.PuedeValidar)
        {
            throw new UserFriendlyException("La factura no está en estado borrador.");
        }

        var validationResult = factura.ValidarParaEmision();
        if (!validationResult.IsValid)
        {
            throw new UserFriendlyException($"No se puede validar la factura:\n{validationResult.ErrorMessage}");
        }

        factura.StateMachine.CambiarA(EstadoFactura.Validada);
    }

    public BatchResult ValidarLote(IEnumerable<FacturaBase> facturas)
    {
        var list = facturas.ToList();
        int total = 0;
        int success = 0;
        string lastError = "";

        foreach (var factura in list)
        {
            if (factura.PuedeValidar)
            {
                total++;
                try
                {
                    Validar(factura);
                    success++;
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                }
            }
        }

        return new BatchResult(total, success, lastError);
    }

    public void Emitir(FacturaBase factura)
    {
        if (factura == null) return;

        if (!factura.PuedeEmitir)
        {
            throw new UserFriendlyException("La factura debe estar validada antes de emitirse.");
        }

        var validationResult = factura.ValidarParaEmision();
        if (!validationResult.IsValid)
        {
            throw new UserFriendlyException($"No se puede emitir la factura:\n{validationResult.ErrorMessage}");
        }

        // Asignar número definitivo si no tiene uno
        if (string.IsNullOrEmpty(factura.Secuencia))
        {
            factura.AsignarNumero();
        }

        // Asignar fecha de emisión si no tiene
        var localTime = InformacionEmpresaHelper.GetLocalTime(factura.Session);
        factura.Fecha = localTime.Date;
        factura.Hora = localTime.TimeOfDay;
        
        // Generar efectos si es una factura de venta
        if (factura is FacturaVenta facturaVenta)
        {
            TesoreriaService.GenerarEfectosVenta(facturaVenta);
            facturaVenta.ActualizarEstadoCobro();
        }

        factura.StateMachine.CambiarA(EstadoFactura.Emitida);
    }

    public BatchResult EmitirLote(IEnumerable<FacturaBase> facturas)
    {
        var list = facturas.ToList();
        int total = 0;
        int success = 0;
        string lastError = "";

        foreach (var factura in list)
        {
            if (factura.PuedeEmitir)
            {
                total++;
                try
                {
                    Emitir(factura);
                    success++;
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                }
            }
        }

        return new BatchResult(total, success, lastError);
    }

    public async Task<VeriFactuService.SendResult> EnviarAVerifactuAsync(IObjectSpace objectSpace, FacturaBase factura, VeriFactuService veriFactuService)
    {
        if (factura == null) return new VeriFactuService.SendResult(false, "La factura es nula.");
        if (veriFactuService == null) return new VeriFactuService.SendResult(false, "El servicio VeriFactu no está disponible.");

        if (!factura.PuedeEnviarVerifactu)
        {
            return new VeriFactuService.SendResult(false, "La factura no está en un estado elegible para envío a VeriFactu.");
        }

        var validationResult = factura.ValidarParaEmision();
        if (!validationResult.IsValid)
        {
            throw new UserFriendlyException($"No se puede enviar a VeriFactu:\n{validationResult.ErrorMessage}");
        }

        // Si el importe es cero, no se envía a VeriFactu
        if (factura.ImporteTotal == 0)
        {
            factura.StateMachine.CambiarA(EstadoFactura.VeriFactuNoNecesario);
            factura.EstadoVeriFactu = EstadoVeriFactu.NoNecesario;
            return new VeriFactuService.SendResult(true, "Factura con importe cero: no requiere envío a VeriFactu.");
        }

        var result = await veriFactuService.SendFacturaAsync(objectSpace, factura);

        return result;
    }

    public async Task<BatchResult> EnviarAVerifactuLoteAsync(IObjectSpace objectSpace, IEnumerable<FacturaBase> facturas, VeriFactuService veriFactuService)
    {
        var list = facturas.ToList();
        int total = 0;
        int success = 0;
        string lastError = "";

        foreach (var factura in list)
        {
            if (factura.PuedeEnviarVerifactu)
            {
                total++;
                var result = await EnviarAVerifactuAsync(objectSpace, factura, veriFactuService);
                if (result.Success)
                {
                    success++;
                }
                else
                {
                    lastError = result.Message;
                }
            }
        }

        return new BatchResult(total, success, lastError);
    }

    public void Contabilizar(FacturaBase factura)
    {
        if (factura == null) return;

        if (!factura.PuedeContabilizar)
        {
            throw new UserFriendlyException("La factura no cumple los requisitos para ser contabilizada.");
        }

        // Ejecutar la acción de contabilización existente
        ContabilidadService.ContabilizarFactura(factura);

        // Cambiar el estado a Contabilizada
        factura.StateMachine.CambiarA(EstadoFactura.Contabilizada);
    }

    public BatchResult ContabilizarLote(IEnumerable<FacturaBase> facturas)
    {
        var list = facturas.ToList();
        int total = 0;
        int success = 0;
        var errorMessages = new List<string>();

        foreach (var factura in list)
        {
            if (factura.PuedeContabilizar)
            {
                total++;
                try
                {
                    Contabilizar(factura);
                    success++;
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"{factura.Secuencia}: {ex.Message}");
                }
            }
        }

        return new BatchResult(total, success, errorMessages.FirstOrDefault() ?? "", errorMessages);
    }

    public void RevertirABorrador(FacturaBase factura)
    {
        if (factura == null) return;

        if (!factura.PuedeRevertirABorrador)
        {
            throw new UserFriendlyException("Solo se pueden revertir a borrador las facturas validadas.");
        }

        factura.StateMachine.CambiarA(EstadoFactura.Borrador);
    }

    public BatchResult RevertirABorradorLote(IEnumerable<FacturaBase> facturas)
    {
        var list = facturas.ToList();
        int total = 0;
        int success = 0;
        string lastError = "";

        foreach (var factura in list)
        {
            if (factura.PuedeRevertirABorrador)
            {
                total++;
                try
                {
                    RevertirABorrador(factura);
                    success++;
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                }
            }
        }

        return new BatchResult(total, success, lastError);
    }

    public async Task<VeriFactuService.SendResult> ProcesarHastaContabilizadaAsync(IObjectSpace objectSpace, FacturaBase factura, VeriFactuService veriFactuService)
    {
        if (factura == null) return new VeriFactuService.SendResult(false, "La factura es nula.");
        if (veriFactuService == null) return new VeriFactuService.SendResult(false, "El servicio VeriFactu no está disponible.");

        logger?.LogInformation("FacturaOrchestrator: Iniciando procesamiento para factura {Secuencia}. Estado actual: {Estado}", 
            factura.Secuencia, factura.EstadoFactura);

        try
        {
            // 1. Validar si está en Borrador
            if (factura.EstadoFactura == EstadoFactura.Borrador)
            {
                logger?.LogInformation("FacturaOrchestrator: Validando factura {Secuencia}", factura.Secuencia);
                Validar(factura);
            }

            // 2. Emitir si está Validada
            if (factura.EstadoFactura == EstadoFactura.Validada)
            {
                logger?.LogInformation("FacturaOrchestrator: Emitiendo factura {Secuencia}", factura.Secuencia);
                Emitir(factura);
            }

            // 3. Enviar a VeriFactu si está Emitida y VeriFactu está activo
            if (factura.EstadoFactura == EstadoFactura.Emitida)
            {
                var infoEmpresa = InformacionEmpresaHelper.GetInformacionEmpresa(factura.Session);
                
                // Prioridad: 1. Configuración del TPV (si existe), 2. Configuración de la empresa
                bool activarVeriFactu = infoEmpresa?.ActivarVeriFactu ?? false;
                var tpv = factura.Tpv ?? factura.SesionTpv?.Tpv;
                if (factura is FacturaSimplificada fs && tpv == null)
                {
                    tpv = fs.VentaTpv?.SesionTpv?.Tpv;
                }
                
                if (tpv != null)
                {
                    activarVeriFactu = tpv.ActivarVeriFactu;
                    logger?.LogInformation("FacturaOrchestrator: Usando configuración de VeriFactu del TPV {Tpv}: {Activo}", tpv.Codigo, activarVeriFactu);
                }
                else
                {
                    logger?.LogInformation("FacturaOrchestrator: No se encontró TPV asociado, usando configuración de empresa para VeriFactu: {Activo}", activarVeriFactu);
                }
                
                var importeCero = factura.ImporteTotal == 0;
                if (importeCero) logger?.LogInformation("FacturaOrchestrator: La factura tiene importe cero, no se enviará a VeriFactu.");

                if (activarVeriFactu && !importeCero)
                {
                    if (factura.EstadoVeriFactu != EstadoVeriFactu.Correcto && 
                        factura.EstadoVeriFactu != EstadoVeriFactu.EnviadaVeriFactu)
                    {
                        logger?.LogInformation("FacturaOrchestrator: Llamando a EnviarAVerifactuAsync para factura {Secuencia}", factura.Secuencia);
                        var sendResult = await EnviarAVerifactuAsync(objectSpace, factura, veriFactuService);
                        logger?.LogInformation("FacturaOrchestrator: Resultado envío VeriFactu para {Secuencia}: {Success}, Mensaje: {Message}, Estado: {EstadoVeriFactu}", 
                            factura.Secuencia, sendResult.Success, sendResult.Message, factura.EstadoVeriFactu);

                        // Consideramos éxito si el resultado es Success O si la factura ha quedado en estado EnviadaVeriFactu (contingencia)
                        bool esExito = sendResult.Success || factura.EstadoVeriFactu == EstadoVeriFactu.EnviadaVeriFactu;

                        if (!esExito)
                        {
                            // Guardamos cambios para asegurar que el estado Emitida persista
                            objectSpace.CommitChanges();
                            return sendResult;
                        }
                    }
                    else
                    {
                        logger?.LogInformation("FacturaOrchestrator: La factura {Secuencia} ya tiene un estado de envío finalizado: {EstadoVeriFactu}", 
                            factura.Secuencia, factura.EstadoVeriFactu);
                    }
                }
                
                // Si llegamos aquí y no estamos en Enviada, es que VeriFactu no es necesario o se ha marcado como tal
                if (factura.EstadoFactura == EstadoFactura.Emitida)
                {
                    logger?.LogInformation("FacturaOrchestrator: Cambiando factura {Secuencia} a VeriFactuNoNecesario", factura.Secuencia);
                    factura.StateMachine.CambiarA(EstadoFactura.VeriFactuNoNecesario);
                    if (factura.EstadoVeriFactu == EstadoVeriFactu.Borrador)
                    {
                        factura.EstadoVeriFactu = EstadoVeriFactu.NoNecesario;
                    }
                }
            }

            // 4. Detener el proceso después del envío a VeriFactu. No contabilizar automáticamente.
            if (factura.EstadoFactura == EstadoFactura.Enviada || 
                factura.EstadoFactura == EstadoFactura.VeriFactuNoNecesario)
            {
                logger?.LogInformation("FacturaOrchestrator: Factura {Secuencia} enviada/procesada con éxito. Se detiene el proceso antes de contabilizar.", factura.Secuencia);
                return new VeriFactuService.SendResult(true, "Factura enviada correctamente.");
            }

            logger?.LogWarning("FacturaOrchestrator: El proceso para {Secuencia} se detuvo en estado {Estado}", factura.Secuencia, factura.EstadoFactura);
            return new VeriFactuService.SendResult(false, $"El proceso se detuvo en el estado {factura.EstadoFactura}.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "FacturaOrchestrator: Error inesperado procesando factura {Secuencia}", factura.Secuencia);
            return new VeriFactuService.SendResult(false, $"Error durante el proceso: {ex.Message}");
        }
    }

    public async Task<BatchResult> ProcesarHastaContabilizadaLoteAsync(IObjectSpace objectSpace, IEnumerable<FacturaBase> facturas, VeriFactuService veriFactuService)
    {
        var list = facturas.ToList();
        int total = 0;
        int success = 0;
        var errorMessages = new List<string>();

        foreach (var factura in list)
        {
            total++;
            try
            {
                var result = await ProcesarHastaContabilizadaAsync(objectSpace, factura, veriFactuService);
                if (result.Success)
                {
                    success++;
                }
                else
                {
                    errorMessages.Add($"{factura.Secuencia}: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                errorMessages.Add($"{factura.Secuencia}: {ex.Message}");
            }
        }

        return new BatchResult(total, success, errorMessages.FirstOrDefault() ?? "", errorMessages);
    }
}
