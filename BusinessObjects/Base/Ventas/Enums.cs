using DevExpress.ExpressApp.DC;

namespace erp.Module.BusinessObjects.Base.Ventas;

public enum TipoDocumentoVenta
{
    [XafDisplayName("Presupuesto")] Presupuesto,
    [XafDisplayName("Oferta")] Oferta,
    [XafDisplayName("Pedido")] Pedido,
    [XafDisplayName("Albarán")] Albaran,
    [XafDisplayName("Factura")] Factura,
    [XafDisplayName("Factura Simplificada")] FacturaSimplificada,
    [XafDisplayName("Ticket")] Ticket,
    [XafDisplayName("Devolución")] Devolucion
}

public enum EstadoDocumentoVenta
{
    [XafDisplayName("Borrador")] Borrador,
    [XafDisplayName("Confirmado")] Confirmado,
    [XafDisplayName("Emitido")] Emitido,
    [XafDisplayName("Impreso")] Impreso,
    [XafDisplayName("Cobrado")] Cobrado,
    [XafDisplayName("Anulado")] Anulado,
    [XafDisplayName("Bloqueado")] Bloqueado,
    [XafDisplayName("Sincronizado")] Sincronizado
}

public enum OrigenDocumentoVenta
{
    [XafDisplayName("Manual")] Manual,
    [XafDisplayName("TPV")] Tpv,
    [XafDisplayName("Web")] Web,
    [XafDisplayName("API")] Api,
    [XafDisplayName("Externo")] Externo
}

public enum MetodoCobroVenta
{
    [XafDisplayName("Efectivo")] Efectivo,
    [XafDisplayName("Tarjeta")] Tarjeta,
    [XafDisplayName("Transferencia")] Transferencia,
    [XafDisplayName("Giro Bancario")] GiroBancario,
    [XafDisplayName("Otros")] Otros
}
