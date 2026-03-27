using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Alquileres;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.Helpers.Contactos;
using erp.Module.Services.Contabilidad;
using erp.Module.Services.Tesoreria;
using erp.Module.Services.Ventas;
using erp.Module.Services.Ventas.StateMachines;
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
    [Action(Caption = "Validar", ConfirmationMessage = "¿Desea validar esta factura?", ImageName = "Action_Validate", TargetObjectsCriteria = "Estado = 'Borrador'")]
    public void ValidarAction()
    {
        var orchestrator = new FacturaVentaOrchestrator();
        orchestrator.Validar(this);
    }

    [Action(Caption = "Enviar a VeriFactu", ConfirmationMessage = "¿Desea enviar esta factura a VeriFactu?", ImageName = "Action_Send", TargetObjectsCriteria = "Estado = 'Validada'")]
    public void EnviarVerifactuAction()
    {
        var orchestrator = new FacturaVentaOrchestrator();
        orchestrator.EnviarAVerifactu(this);
    }

    [Action(Caption = "Contabilizar", ConfirmationMessage = "¿Desea generar el asiento contable para esta factura?", ImageName = "Action_LinkUnlink_Link", TargetObjectsCriteria = "Estado = 'EnviadaVerifactu'")]
    public void Contabilizar()
    {
        var orchestrator = new FacturaVentaOrchestrator();
        orchestrator.Contabilizar(this);
        OnChanged(nameof(AsientoContable));
        OnChanged(nameof(ApuntesContables));
    }

    [XafDisplayName("Efectos de Cobro")]
    [Association("FacturaVenta-EfectosCobro")]
    [DevExpress.Xpo.Aggregated]
    public XPCollection<EfectoCobro> EfectosCobro => GetCollection<EfectoCobro>();

    public void ActualizarEstadoCobro()
    {
        if (IsLoading || IsSaving) return;

        decimal cobrado = EfectosCobro.Where(e => e.Estado == EstadoEfecto.Cobrado).Sum(e => e.Importe);
        
        if (cobrado >= ImporteTotal && ImporteTotal > 0)
        {
            EstadoCobro = EstadoCobroFactura.Pagada;
        }
        else if (cobrado > 0)
        {
            EstadoCobro = EstadoCobroFactura.PagoParcial;
        }
        else
        {
            EstadoCobro = EstadoCobroFactura.Pendiente;
        }
    }

    [XafDisplayName("Apuntes Contables")]
    public IEnumerable<Apunte> ApuntesContables => AsientoContable?.Apuntes ?? Enumerable.Empty<Apunte>();

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (IsLoading || IsSaving) return;
        if (propertyName is nameof(CondicionPago) or nameof(ImporteTotal))
        {
            TesoreriaService.GenerarEfectosVenta(this);
            ActualizarEstadoCobro();
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
        EstadoCobro = EstadoCobroFactura.Pendiente;
    }

    public override bool EsValida()
    {
        return EstadoVeriFactu != EstadoVeriFactu.Enviado
               && Cliente != null
               && !string.IsNullOrEmpty(Cliente.Nombre)
               && !string.IsNullOrEmpty(Cliente.Nif)
                && !string.IsNullOrEmpty(Texto)
               && Impuestos.Count > 0;
    }

    protected override IDocumentoVentaStateMachine CreateStateMachine() => new FacturaVentaStateMachine(this);
}