using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Alquileres;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.BusinessObjects.Base.Facturacion;

using erp.Module.Helpers.Contactos;
using erp.Module.Services.Tesoreria;
using DevExpress.Persistent.Validation;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[NavigationItem("Ventas")]
[ImageName("BO_Invoice")]
[DefaultProperty(nameof(Secuencia))]
[RuleCriteria("Factura_SumaEfectosCoherente", DefaultContexts.Save, "EfectosCobro.Sum(Importe) = ImporteTotal", "La suma de los importes de los efectos debe coincidir con el total de la factura.")]
public class Factura(Session session) : FacturaBase(session)
{
    [XafDisplayName("Pagos")]
    [Association("Factura-Pagos")]
    public XPCollection<Pago> Pagos => GetCollection<Pago>();

    [XafDisplayName("Efectos de Cobro")]
    [Association("Factura-EfectosCobro")]
    [DevExpress.Xpo.Aggregated]
    public XPCollection<EfectoCobro> EfectosCobro => GetCollection<EfectoCobro>();

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (IsLoading || IsSaving) return;
        if (propertyName is nameof(CondicionPago) or nameof(ImporteTotal))
        {
            TesoreriaService.GenerarEfectosVenta(this);
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        Serie ??= companyInfo?.PrefijoFacturasVentaPorDefecto;
    }

    public override bool EsValida()
    {
        return EstadoVeriFactu != ValoresEstadoVeriFactu.Enviado
               && Cliente != null
               && !string.IsNullOrEmpty(Cliente.Nombre)
               && !string.IsNullOrEmpty(Cliente.Nif)
               && !string.IsNullOrEmpty(Texto)
               && Impuestos.Count > 0;
    }
}