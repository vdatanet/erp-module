using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.Helpers.Contactos;
using VeriFactu.Xml.Factu.Alta;

namespace erp.Module.BusinessObjects.Tpv;

[DefaultClassOptions]
[NavigationItem("Tpv")]
[ImageName("BO_Invoice")] // Podría cambiarse a algo más específico si existe
[DefaultProperty(nameof(Secuencia))]
public class FacturaSimplificada(Session session) : FacturaBase(session)
{
    private SesionTpv? _sesionTpv;
    private Tpv? _tpv;

    [XafDisplayName("TPV")]
    [Association("Tpv-FacturasSimplificadas")]
    public Tpv? Tpv
    {
        get => _tpv;
        set => SetPropertyValue(nameof(Tpv), ref _tpv, value);
    }

    [XafDisplayName("Sesión TPV")]
    [Association("SesionTpv-FacturasSimplificadas")]
    public SesionTpv? SesionTpv
    {
        get => _sesionTpv;
        set => SetPropertyValue(nameof(SesionTpv), ref _sesionTpv, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        Serie ??= companyInfo.PrefijoFacturasSimplificadasPorDefecto;
        TipoFactura = TipoFactura.F2;
    }

    public override bool EsValida()
    {
        return EstadoVeriFactu != ValoresEstadoVeriFactu.Enviado
               && !string.IsNullOrEmpty(Texto)
               && Impuestos.Count > 0;
    }
}