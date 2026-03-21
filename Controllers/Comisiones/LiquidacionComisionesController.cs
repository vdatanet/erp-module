using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using erp.Module.BusinessObjects.Comisiones;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.BusinessObjects.Alquileres;
using System.Linq;

namespace erp.Module.Controllers.Comisiones;

public class LiquidacionComisionesController : ObjectViewController<DetailView, LiquidacionComision>
{
    public LiquidacionComisionesController()
    {
        var generarComisionesAction = new SimpleAction(this, "GenerarComisiones", PredefinedCategory.RecordEdit)
        {
            Caption = "Generar Comisiones",
            ConfirmationMessage = "¿Desea generar las comisiones para este mes/año y vendedor? Se borrarán las actuales de esta liquidación.",
            ImageName = "Action_Generate"
        };
        generarComisionesAction.Execute += GenerarComisionesAction_Execute;
    }

    private void GenerarComisionesAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var liquidacion = ViewCurrentObject;
        if (liquidacion == null || liquidacion.Vendedor == null) return;

        // Borrar comisiones actuales de esta liquidación
        var currentComisiones = liquidacion.Comisiones.ToList();
        foreach (var comision in currentComisiones)
        {
            comision.Delete();
        }

        // Buscar facturas que tengan pagos en el mes/año indicado y que sean del vendedor
        // Nota: El vendedor está en el DocumentoVenta (base de Factura)
        var mes = liquidacion.Mes;
        var anio = liquidacion.Anio;

        var pagos = ObjectSpace.GetObjectsQuery<Pago>()
            .Where(p => p.FechaPago.Month == mes && p.FechaPago.Year == anio && p.Factura != null && p.Factura.Vendedor == liquidacion.Vendedor)
            .ToList();

        // Agrupar por factura para no duplicar si hay varios pagos para la misma factura en el mismo mes
        var facturasIds = pagos.Select(p => p.Factura!.Oid).Distinct();

        foreach (var facturaId in facturasIds)
        {
            var factura = ObjectSpace.GetObjectByKey<Factura>(facturaId);
            if (factura == null) continue;

            foreach (var linea in factura.Lineas)
            {
                if (linea.ComisionCalculada > 0)
                {
                    var nuevaComision = ObjectSpace.CreateObject<Comision>();
                    nuevaComision.Liquidacion = liquidacion;
                    nuevaComision.LineaDocumentoVenta = linea;
                    nuevaComision.Importe = linea.ComisionCalculada;
                }
            }
        }

        if (ObjectSpace.IsModified)
        {
            ObjectSpace.CommitChanges();
        }
        
        View.Refresh();
    }
}
