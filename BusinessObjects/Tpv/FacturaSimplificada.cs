using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Helpers.Contactos;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Tpv;

[DefaultClassOptions]
[NavigationItem("Tpv")]
[ImageName("BO_Invoice")] // Podría cambiarse a algo más específico si existe
[DefaultProperty(nameof(Secuencia))]
public class FacturaSimplificada(Session session) : FacturaBase(session)
{
    [Persistent(nameof(VentaTpv))]
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
}