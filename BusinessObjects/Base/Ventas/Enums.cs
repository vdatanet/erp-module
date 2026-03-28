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

public enum EstadoOferta
{
    [XafDisplayName("Borrador")] Borrador,
    [XafDisplayName("Enviada")] Enviada,
    [XafDisplayName("Aceptada")] Aceptada,
    [XafDisplayName("Rechazada")] Rechazada,
    [XafDisplayName("Anulada")] Anulada,
    [XafDisplayName("Vencida")] Vencida
}

public enum EstadoPedido
{
    [XafDisplayName("Borrador")] Borrador,
    [XafDisplayName("Confirmado")] Confirmado,
    [XafDisplayName("En Preparación")] EnPreparacion,
    [XafDisplayName("Preparado")] Preparado,
    [XafDisplayName("Enviado")] Enviado,
    [XafDisplayName("Entregado")] Entregado,
    [XafDisplayName("Anulado")] Anulado
}

public enum EstadoAlbaran
{
    [XafDisplayName("Borrador")] Borrador,
    [XafDisplayName("Emitido")] Emitido,
    [XafDisplayName("Enviado")] Enviado,
    [XafDisplayName("Entregado")] Entregado,
    [XafDisplayName("Facturado")] Facturado,
    [XafDisplayName("Anulado")] Anulado
}

public enum EstadoFactura
{
    [XafDisplayName("Borrador")] Borrador,
    [XafDisplayName("Validada")] Validada,
    [XafDisplayName("Enviada a VeriFactu")] EnviadaVerifactu,
    [XafDisplayName("Contabilizada")] Contabilizada
}

public enum EstadoCobroFactura
{
    [XafDisplayName("Pendiente")] Pendiente,
    [XafDisplayName("Pago Parcial")] PagoParcial,
    [XafDisplayName("Pagada")] Pagada
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
