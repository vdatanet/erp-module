using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.BusinessObjects.Compras;
using erp.Module.BusinessObjects.Auxiliares;

namespace erp.Module.Services.Tesoreria;

public static class TesoreriaService
{
    public static void GenerarEfectosVenta(FacturaVenta factura)
    {
        if (factura.EfectosCobro.Any(e => e.Estado != EstadoEfecto.Pendiente))
            return;

        // Borrar efectos pendientes existentes
        var efectosABorrar = factura.EfectosCobro.ToList();
        foreach (var efecto in efectosABorrar)
        {
            efecto.Delete();
        }

        var condicion = factura.CondicionPago;
        if (condicion == null || condicion.NumeroPlazos <= 0)
        {
            var soloEfecto = new EfectoCobro(factura.Session)
            {
                Factura = factura,
                Importe = factura.ImporteTotal,
                FechaVencimiento = factura.Fecha,
                Estado = EstadoEfecto.Pendiente
            };
            return;
        }

        decimal importeRestante = factura.ImporteTotal;
        decimal importePlazo = Math.Round(factura.ImporteTotal / condicion.NumeroPlazos, 2);

        for (int i = 0; i < condicion.NumeroPlazos; i++)
        {
            decimal importeActual = (i == condicion.NumeroPlazos - 1) ? importeRestante : importePlazo;
            DateTime fechaVencimiento = factura.Fecha.AddDays(condicion.PlazoPrimerPago + (i * condicion.DiasEntrePlazos));

            var efecto = new EfectoCobro(factura.Session)
            {
                Factura = factura,
                Importe = importeActual,
                FechaVencimiento = fechaVencimiento,
                Estado = EstadoEfecto.Pendiente
            };
            
            importeRestante -= importeActual;
        }
    }

    public static void GenerarEfectosCompra(FacturaCompra factura)
    {
        if (factura.EfectosPago.Any(e => e.Estado != EstadoEfecto.Pendiente))
            return;

        var efectosABorrar = factura.EfectosPago.ToList();
        foreach (var efecto in efectosABorrar)
        {
            efecto.Delete();
        }

        var condicion = factura.CondicionPago;
        if (condicion == null || condicion.NumeroPlazos <= 0)
        {
            var soloEfecto = new EfectoPago(factura.Session)
            {
                FacturaCompra = factura,
                Importe = factura.ImporteTotal,
                FechaVencimiento = factura.Fecha,
                Estado = EstadoEfecto.Pendiente
            };
            return;
        }

        decimal importeRestante = factura.ImporteTotal;
        decimal importePlazo = Math.Round(factura.ImporteTotal / condicion.NumeroPlazos, 2);

        for (int i = 0; i < condicion.NumeroPlazos; i++)
        {
            decimal importeActual = (i == condicion.NumeroPlazos - 1) ? importeRestante : importePlazo;
            DateTime fechaVencimiento = factura.Fecha.AddDays(condicion.PlazoPrimerPago + (i * condicion.DiasEntrePlazos));

            var efecto = new EfectoPago(factura.Session)
            {
                FacturaCompra = factura,
                Importe = importeActual,
                FechaVencimiento = fechaVencimiento,
                Estado = EstadoEfecto.Pendiente
            };
            
            importeRestante -= importeActual;
        }
    }
}
