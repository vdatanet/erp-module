using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.BusinessObjects.Base.Compras;
using DevExpress.ExpressApp.DC;

using erp.Module.Helpers.Contactos;
using erp.Module.Services.Tesoreria;
using DevExpress.Persistent.Validation;

namespace erp.Module.BusinessObjects.Compras;

[DefaultClassOptions]
[NavigationItem("Compras")]
[ImageName("BO_Invoice")]
[DefaultProperty(nameof(Secuencia))]
[RuleCriteria("FacturaCompra_SumaEfectosCoherente", DefaultContexts.Save, "EfectosPago.Sum(Importe) = ImporteTotal", "La suma de los importes de los efectos debe coincidir con el total de la factura.")]
public class FacturaCompra(Session session) : DocumentoCompra(session)
{
    [XafDisplayName("Efectos de Pago")]
    [Association("FacturaCompra-EfectosPago")]
    [DevExpress.Xpo.Aggregated]
    public XPCollection<EfectoPago> EfectosPago => GetCollection<EfectoPago>();

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (IsLoading || IsSaving) return;
        if (propertyName is nameof(CondicionPago) or nameof(ImporteTotal))
        {
            TesoreriaService.GenerarEfectosCompra(this);
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo == null) return;
        Serie ??= companyInfo.PrefijoFacturasCompraPorDefecto;
    }
}