using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Alquileres;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Base.Ventas;

using erp.Module.Helpers.Contactos;
using erp.Module.Services.Contabilidad;
using erp.Module.Services.Tesoreria;
using DevExpress.Persistent.Validation;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[XafDisplayName("Factura de Venta")]
[NavigationItem("Ventas")]
[ImageName("BO_Invoice")]
[DefaultProperty(nameof(Secuencia))]
[RuleCriteria("Factura_SumaEfectosCoherente", DefaultContexts.Save, "EfectosCobro.Sum(Importe) = ImporteTotal", "La suma de los importes de los efectos debe coincidir con el total de la factura.")]
public class FacturaVenta(Session session) : FacturaBase(session)
{
    [Action(Caption = "Contabilizar", ConfirmationMessage = "¿Desea generar el asiento contable para esta factura?", ImageName = "Action_LinkUnlink_Link", TargetObjectsCriteria = "AsientoContable is null")]
    public void Contabilizar()
    {
        ContabilidadService.ContabilizarFactura(this);
    }

    [XafDisplayName("Efectos de Cobro")]
    [Association("FacturaVenta-EfectosCobro")]
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
        if (companyInfo == null) return;
        Serie ??= companyInfo.PrefijoFacturasVentaPorDefecto;
        EsFactura = true;
        TipoDocumento = TipoDocumentoVenta.Factura;
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