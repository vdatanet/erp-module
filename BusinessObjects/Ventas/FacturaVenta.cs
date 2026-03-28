using System.ComponentModel;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Facturacion;
using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.BusinessObjects.Contabilidad;
using erp.Module.BusinessObjects.Tesoreria;
using erp.Module.Helpers.Contactos;
using erp.Module.Services.Tesoreria;
using erp.Module.Services.Ventas;
using erp.Module.Services.Ventas.StateMachines;

namespace erp.Module.BusinessObjects.Ventas;

[DefaultClassOptions]
[XafDisplayName("Factura de Venta")]
[NavigationItem("Ventas")]
[ImageName("BO_Invoice")]
[DefaultProperty(nameof(Secuencia))]
[Appearance("BlockEditingFacturaNoBorrador", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria =
        "EstadoFactura = 'Validada' OR EstadoFactura = 1 OR EstadoFactura = 'EnviadaVerifactu' OR EstadoFactura = 2 OR EstadoFactura = 'Contabilizada' OR EstadoFactura = 3",
    Context = "Any", Enabled = false)]
[Appearance("BlockDeletionFacturaNoBorrador", AppearanceItemType = "Action", TargetItems = "Delete",
    Criteria = "EstadoFactura != 'Borrador' AND EstadoFactura != 0", Context = "Any", Enabled = false)]
[RuleCriteria("Factura_SoloBorradorModificable", DefaultContexts.Save,
    "EstadoFactura = 'Borrador' OR EstadoFactura = 0 OR EstadoFactura = 'Validada' OR EstadoFactura = 1 OR EstadoFactura = 'EnviadaVerifactu' OR EstadoFactura = 2 OR EstadoFactura = 'Contabilizada' OR EstadoFactura = 3",
    "Una factura solo se puede modificar si está en borrador.", SkipNullOrEmptyValues = false)]
[RuleCriteria("Factura_SoloBorradorEliminable", DefaultContexts.Delete,
    "EstadoFactura = 'Borrador' OR EstadoFactura = 0",
    "Una factura solo se puede eliminar si está en borrador.")]
[RuleCriteria("Factura_SumaEfectosCoherente", DefaultContexts.Save, "EfectosCobro.Sum(Importe) = ImporteTotal",
    "La suma de los importes de los efectos debe coincidir con el total de la factura.")]
public class FacturaVenta(Session session) : FacturaBase(session)
{
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
        DiarioVentas ??= companyInfo.DiarioVentasPorDefecto;
        EsFactura = true;
        TipoDocumento = TipoDocumentoVenta.Factura;
        EstadoCobro = EstadoCobroFactura.Pendiente;
    }

    public override bool EsValida() => ValidarParaEmision().IsValid;

    public override ValidationResult ValidarParaEmision()
    {
        var result = base.ValidarParaEmision();
        if (Cliente == null)
            result.AddError("La factura requiere un cliente.");
        else
        {
            if (string.IsNullOrEmpty(NombreCliente))
                result.AddError("El cliente debe tener un nombre.");
            if (string.IsNullOrEmpty(DocumentoIdentificacionCliente))
                result.AddError("El cliente debe tener un NIF válido.");
        }

        return result;
    }

    protected override IFacturaStateMachine GetStateMachine() => new FacturaVentaStateMachine(this);
}