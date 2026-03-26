using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contactos;
using erp.Module.BusinessObjects.Ventas;
using erp.Module.Helpers.Contactos;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Tpv;

[DefaultClassOptions]
[NavigationItem("Tpv")]
[XafDisplayName("Factura Simplificada")]
[ImageName("BO_Invoice")] // Podría cambiarse a algo más específico si existe
[DefaultProperty(nameof(Secuencia))]
public class FacturaSimplificada(Session session) : FacturaBase(session)
{
    private VentaTpv? _ventaTpv;

    [XafDisplayName("Venta TPV")]
    public VentaTpv? VentaTpv
    {
        get => _ventaTpv;
        set => SetPropertyValue(nameof(VentaTpv), ref _ventaTpv, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        Serie ??= companyInfo.PrefijoFacturasSimplificadasPorDefecto;
        TipoFactura = TipoFactura.F2;
        EsFacturaSimplificada = true;
        TipoDocumento = TipoDocumentoVenta.FacturaSimplificada;
    }

    public override bool EsValida()
    {
        return EstadoVeriFactu != ValoresEstadoVeriFactu.Enviado
               && !string.IsNullOrEmpty(Texto)
               && Impuestos.Count > 0;
    }

    public (FacturaSimplificada Rectificativa, FacturaVenta Nominal) ConvertirAFacturaNominal(Tercero cliente)
    {
        if (cliente == null) throw new UserFriendlyException("Se requiere un cliente para generar una factura nominal.");
        if (string.IsNullOrEmpty(cliente.Nif)) throw new UserFriendlyException("El cliente seleccionado no tiene NIF.");

        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        // 1. Crear la Factura Simplificada Rectificativa (para anular la actual)
        var rectificativa = new FacturaSimplificada(Session)
        {
            VentaTpv = VentaTpv,
            Tpv = Tpv,
            SesionTpv = SesionTpv,
            Fecha = companyInfo?.GetLocalTime() ?? DateTime.Now,
            Cliente = Cliente,
            Notas = $"Anulación de factura {Secuencia} para emisión de factura nominal.",
            Usuario = Usuario,
            TipoFactura = TipoFactura.R4,
            TipoRectificativa = TipoRectificativa.S, // Sustitutiva
            EsFacturaSimplificada = true,
            TipoDocumento = TipoDocumentoVenta.FacturaSimplificada
        };
        rectificativa.Serie = Serie;

        // Copiar líneas en negativo
        foreach (var lineaOriginal in Lineas)
        {
            var lineaRect = new DocumentoVentaLinea(Session)
            {
                DocumentoVenta = rectificativa,
                Producto = lineaOriginal.Producto,
                NombreProducto = lineaOriginal.NombreProducto,
                Cantidad = -lineaOriginal.Cantidad,
                PrecioUnitario = lineaOriginal.PrecioUnitario,
                PorcentajeDescuento = lineaOriginal.PorcentajeDescuento
            };
            foreach (var impOriginal in lineaOriginal.Impuestos)
            {
                var impRect = new DocumentoVentaLineaImpuesto(Session)
                {
                    DocumentoVentaLinea = lineaRect,
                    TipoImpuesto = impOriginal.TipoImpuesto
                };
            }
            rectificativa.Lineas.Add(lineaRect);
        }
        rectificativa.RecalcularTotales();
        rectificativa.AsignarNumero();

        // 2. Crear la Factura de Venta Nominal
        var nominal = new FacturaVenta(Session)
        {
            Fecha = companyInfo?.GetLocalTime() ?? DateTime.Now,
            Cliente = cliente,
            Notas = $"Factura generada a partir de la simplificada {Secuencia}.",
            Usuario = Usuario,
            TipoFactura = TipoFactura.F1,
            EsFactura = true,
            TipoDocumento = TipoDocumentoVenta.Factura
        };
        
        nominal.Serie = companyInfo?.PrefijoFacturasVentaPorDefecto;

        // Copiar líneas en positivo
        foreach (var lineaOriginal in Lineas)
        {
            var lineaNom = new DocumentoVentaLinea(Session)
            {
                DocumentoVenta = nominal,
                Producto = lineaOriginal.Producto,
                NombreProducto = lineaOriginal.NombreProducto,
                Cantidad = lineaOriginal.Cantidad,
                PrecioUnitario = lineaOriginal.PrecioUnitario,
                PorcentajeDescuento = lineaOriginal.PorcentajeDescuento
            };
            foreach (var impOriginal in lineaOriginal.Impuestos)
            {
                var impNom = new DocumentoVentaLineaImpuesto(Session)
                {
                    DocumentoVentaLinea = lineaNom,
                    TipoImpuesto = impOriginal.TipoImpuesto
                };
            }
            nominal.Lineas.Add(lineaNom);
        }
        nominal.RecalcularTotales();
        nominal.AsignarNumero();

        return (rectificativa, nominal);
    }
}