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

namespace erp.Module.Services.Ventas;

public class FacturaOrchestrator
{
    public void Validar(FacturaBase factura)
    {
        if (factura == null) return;

        var validationResult = factura.ValidarParaEmision();
        if (!validationResult.IsValid)
        {
            throw new UserFriendlyException($"No se puede validar la factura:\n{validationResult.ErrorMessage}");
        }

        factura.StateMachine.CambiarA(EstadoFactura.Validada);
    }

    public void Emitir(FacturaBase factura)
    {
        if (factura == null) return;

        if (factura.EstadoFactura == EstadoFactura.Borrador)
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

    public async Task<VeriFactuService.SendResult> EnviarAVerifactuAsync(IObjectSpace objectSpace, FacturaBase factura, VeriFactuService veriFactuService)
    {
        if (factura == null) return new VeriFactuService.SendResult(false, "La factura es nula.");
        if (veriFactuService == null) return new VeriFactuService.SendResult(false, "El servicio VeriFactu no está disponible.");

        if (factura.EstadoFactura == EstadoFactura.Borrador)
        {
            throw new UserFriendlyException("La factura debe estar validada antes de enviarse a VeriFactu.");
        }

        var validationResult = factura.ValidarParaEmision();
        if (!validationResult.IsValid)
        {
            throw new UserFriendlyException($"No se puede enviar a VeriFactu:\n{validationResult.ErrorMessage}");
        }

        var result = await veriFactuService.SendFacturaAsync(objectSpace, factura);

        return result;
    }

    public void Contabilizar(FacturaBase factura)
    {
        if (factura == null) return;

        // Ejecutar la acción de contabilización existente
        ContabilidadService.ContabilizarFactura(factura);

        // Cambiar el estado a Contabilizada
        factura.StateMachine.CambiarA(EstadoFactura.Contabilizada);
    }

    public void RevertirABorrador(FacturaBase factura)
    {
        if (factura == null) return;

        if (factura.EstadoFactura != EstadoFactura.Validada)
        {
            throw new UserFriendlyException("Solo se pueden revertir a borrador las facturas validadas.");
        }

        factura.StateMachine.CambiarA(EstadoFactura.Borrador);
    }

    public async Task<VeriFactuService.SendResult> ProcesarHastaContabilizadaAsync(IObjectSpace objectSpace, FacturaBase factura, VeriFactuService veriFactuService)
    {
        if (factura == null) return new VeriFactuService.SendResult(false, "La factura es nula.");
        if (veriFactuService == null) return new VeriFactuService.SendResult(false, "El servicio VeriFactu no está disponible.");

        try
        {
            // 1. Validar si está en Borrador
            if (factura.EstadoFactura == EstadoFactura.Borrador)
            {
                Validar(factura);
            }

            // 2. Emitir si está Validada
            if (factura.EstadoFactura == EstadoFactura.Validada)
            {
                Emitir(factura);
            }

            // 3. Enviar a VeriFactu si está Emitida
            if (factura.EstadoFactura == EstadoFactura.Emitida &&
                factura.EstadoVeriFactu != EstadoVeriFactu.AceptadaVeriFactu &&
                factura.EstadoVeriFactu != EstadoVeriFactu.EnviadaVeriFactu)
            {
                var sendResult = await EnviarAVerifactuAsync(objectSpace, factura, veriFactuService);
                if (!sendResult.Success)
                {
                    return sendResult;
                }
            }

            // 4. Contabilizar si está Enviada/Aceptada/Pendiente
            if (factura.EstadoFactura != EstadoFactura.Contabilizada &&
                (factura.EstadoFactura == EstadoFactura.Enviada ||
                 factura.EstadoVeriFactu == EstadoVeriFactu.AceptadaVeriFactu ||
                 factura.EstadoVeriFactu == EstadoVeriFactu.EnviadaVeriFactu ||
                 factura.EstadoVeriFactu == EstadoVeriFactu.PendienteVeriFactu))
            {
                Contabilizar(factura);
            }

            if (factura.EstadoFactura == EstadoFactura.Contabilizada)
            {
                return new VeriFactuService.SendResult(true, "Factura procesada y contabilizada correctamente.");
            }

            return new VeriFactuService.SendResult(false, $"El proceso se detuvo en el estado {factura.EstadoFactura}.");
        }
        catch (Exception ex)
        {
            return new VeriFactuService.SendResult(false, $"Error durante el proceso: {ex.Message}");
        }
    }
}
