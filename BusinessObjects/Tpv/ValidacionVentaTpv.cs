using DevExpress.ExpressApp.Security;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;

namespace erp.Module.BusinessObjects.Tpv;

public class ValidacionVentaTpv
{
    public bool EsValida(VentaTpv venta, out string mensaje)
    {
        mensaje = string.Empty;

        if (venta == null)
        {
            mensaje = "La venta es nula.";
            return false;
        }

        if (venta.Lineas.Count == 0)
        {
            mensaje = "La venta no tiene líneas.";
            return false;
        }

        if (venta.TotalFinal <= 0)
        {
            mensaje = "El total de la venta debe ser mayor que cero.";
            return false;
        }

        if (venta.SesionTpv == null)
        {
            mensaje = "La venta debe estar asociada a una sesión TPV.";
            return false;
        }

        if (venta.SesionTpv.Estado != EstadoSesionTpv.Abierta)
        {
            mensaje = "La sesión TPV asociada está cerrada.";
            return false;
        }

        return true;
    }

    public bool PuedeFinalizar(VentaTpv venta, out string mensaje)
    {
        if (!EsValida(venta, out mensaje)) return false;

        if (venta.TotalPagado < venta.TotalFinal)
        {
            mensaje = "El importe pagado es insuficiente.";
            return false;
        }

        return true;
    }
}
