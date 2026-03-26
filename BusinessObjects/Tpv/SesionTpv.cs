using System.ComponentModel;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Security;
using Microsoft.Extensions.DependencyInjection;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.Factories;
using erp.Module.Helpers.Contactos;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Configuraciones;

using erp.Module.BusinessObjects.Base.Ventas;
using erp.Module.Helpers.Contactos;

using erp.Module.Services.Tpv;

namespace erp.Module.BusinessObjects.Tpv;

public enum EstadoSesionTpv
{
    Abierta,
    Cerrada
}

[DefaultClassOptions]
[NavigationItem("Tpv")]
[XafDisplayName("Sesión TPV")]
[Persistent("SesionTpv")]
[DefaultProperty(nameof(Apertura))]
[RuleCriteria("UnicaSesionAbiertaPorTpv", DefaultContexts.Save, "Estado != 'Abierta' || [Tpv.Sesiones][Estado = 'Abierta' && Oid != ^.Oid].Count() == 0", 
    CustomMessageTemplate = "Ya existe otra sesión abierta para este TPV.", SkipNullOrEmptyValues = false, ResultType = ValidationResultType.Information)]
public class SesionTpv(Session session) : EntidadBase(session)
{
    private DateTime _apertura;
    private DateTime? _cierre;
    private EstadoSesionTpv _estado;
    private decimal _importeApertura;
    private decimal _importeCierre;
    private decimal _importeEsperado;
    private decimal _diferenciaArqueo;
    private string? _observaciones;
    private Tpv? _tpv;
    private Guid? _estadoAbiertoUnico;
    private ApplicationUser? _usuario;
    private ApplicationUser? _abiertaPor;
    private ApplicationUser? _cerradaPor;
    private ApplicationUser? _reabiertaPor;
    private string? _motivoReapertura;
    private int _numeroReaperturas;
    private DateTime? _fechaUltimaModificacion;
    private int _numero;
    private string? _secuencia;

    [XafDisplayName("Número")]
    [Browsable(false)]
    public int Numero
    {
        get => _numero;
        set => SetPropertyValue(nameof(Numero), ref _numero, value);
    }

    [XafDisplayName("Secuencia")]
    [ModelDefault("AllowEdit", "False")]
    public string? Secuencia
    {
        get => _secuencia;
        set => SetPropertyValue(nameof(Secuencia), ref _secuencia, value);
    }

    [XafDisplayName("Estado Abierto Único")]
    [Browsable(false)]
    [Indexed("Tpv", Unique = true)]
    public Guid? EstadoAbiertoUnico
    {
        get => _estadoAbiertoUnico;
        set => SetPropertyValue(nameof(EstadoAbiertoUnico), ref _estadoAbiertoUnico, value);
    }

    [XafDisplayName("TPV")]
    [RuleRequiredField("RuleRequiredField_SesionTpv_Tpv", DefaultContexts.Save, CustomMessageTemplate = "El TPV de la Sesión es obligatorio")]
    [Association("Tpv-Sesiones")]
    public Tpv? Tpv
    {
        get => _tpv;
        set => SetPropertyValue(nameof(Tpv), ref _tpv, value);
    }

    [XafDisplayName("Usuario")]
    [RuleRequiredField("RuleRequiredField_SesionTpv_Usuario", DefaultContexts.Save, CustomMessageTemplate = "El Usuario de la Sesión es obligatorio")]
    public ApplicationUser? Usuario
    {
        get => _usuario;
        set => SetPropertyValue(nameof(Usuario), ref _usuario, value);
    }

    [XafDisplayName("Abierta por")]
    public ApplicationUser? AbiertaPor
    {
        get => _abiertaPor;
        set => SetPropertyValue(nameof(AbiertaPor), ref _abiertaPor, value);
    }

    [XafDisplayName("Cerrada por")]
    public ApplicationUser? CerradaPor
    {
        get => _cerradaPor;
        set => SetPropertyValue(nameof(CerradaPor), ref _cerradaPor, value);
    }

    [XafDisplayName("Reabierta por")]
    public ApplicationUser? ReabiertaPor
    {
        get => _reabiertaPor;
        set => SetPropertyValue(nameof(ReabiertaPor), ref _reabiertaPor, value);
    }

    [XafDisplayName("Motivo Reapertura")]
    public string? MotivoReapertura
    {
        get => _motivoReapertura;
        set => SetPropertyValue(nameof(MotivoReapertura), ref _motivoReapertura, value);
    }

    [XafDisplayName("Número de Reaperturas")]
    public int NumeroReaperturas
    {
        get => _numeroReaperturas;
        set => SetPropertyValue(nameof(NumeroReaperturas), ref _numeroReaperturas, value);
    }

    [XafDisplayName("Última Modificación")]
    public DateTime? FechaUltimaModificacion
    {
        get => _fechaUltimaModificacion;
        set => SetPropertyValue(nameof(FechaUltimaModificacion), ref _fechaUltimaModificacion, value);
    }

    [XafDisplayName("Apertura")]
    [RuleRequiredField("RuleRequiredField_SesionTpv_Apertura", DefaultContexts.Save, CustomMessageTemplate = "La Fecha de Apertura de la Sesión es obligatoria")]
    public DateTime Apertura
    {
        get => _apertura;
        set => SetPropertyValue(nameof(Apertura), ref _apertura, value);
    }

    [XafDisplayName("Cierre")]
    [Appearance("CierreVisibleWhenClosed", Visibility = ViewItemVisibility.Hide, Criteria = "Estado = 'Abierta'")]
    public DateTime? Cierre
    {
        get => _cierre;
        set => SetPropertyValue(nameof(Cierre), ref _cierre, value);
    }

    [XafDisplayName("Importe Apertura")]
    [ModelDefault("DisplayFormat", "{0:n2} €")]
    [ModelDefault("EditMask", "n2")]
    public decimal ImporteApertura
    {
        get => _importeApertura;
        set => SetPropertyValue(nameof(ImporteApertura), ref _importeApertura, value);
    }

    [XafDisplayName("Importe Contado")]
    [ModelDefault("DisplayFormat", "{0:n2} €")]
    [ModelDefault("EditMask", "n2")]
    [Appearance("ImporteCierreVisibleWhenClosed", Visibility = ViewItemVisibility.Hide,
        Criteria = "Estado = 'Abierta'")]
    public decimal ImporteCierre
    {
        get => _importeCierre;
        set => SetPropertyValue(nameof(ImporteCierre), ref _importeCierre, value);
    }

    [XafDisplayName("Importe Esperado")]
    [ModelDefault("DisplayFormat", "{0:n2} €")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal ImporteEsperado
    {
        get => _importeEsperado;
        set => SetPropertyValue(nameof(ImporteEsperado), ref _importeEsperado, value);
    }

    [XafDisplayName("Diferencia Arqueo")]
    [ModelDefault("DisplayFormat", "{0:n2} €")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal DiferenciaArqueo
    {
        get => _diferenciaArqueo;
        set => SetPropertyValue(nameof(DiferenciaArqueo), ref _diferenciaArqueo, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Observaciones")]
    public string? Observaciones
    {
        get => _observaciones;
        set => SetPropertyValue(nameof(Observaciones), ref _observaciones, value);
    }

    [XafDisplayName("Estado")]
    [ModelDefault("AllowEdit", "False")]
    public EstadoSesionTpv Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [XafDisplayName("Facturas Simplificadas")]
    public XPCollection<FacturaSimplificada> FacturasSimplificadas => new XPCollection<FacturaSimplificada>(Session, CriteriaOperator.Parse("SesionTpv.Oid = ? AND TipoDocumento = ?", Oid, TipoDocumentoVenta.FacturaSimplificada));

    [Association("SesionTpv-DocumentosVenta")]
    [XafDisplayName("Documentos de Venta")]
    public XPCollection<DocumentoVenta> DocumentosVenta => GetCollection<DocumentoVenta>();

    [Association("SesionTpv-VentasTpv")]
    [XafDisplayName("Ventas TPV")]
    public XPCollection<VentaTpv> VentasTpv => GetCollection<VentaTpv>();

    [Association("SesionTpv-Eventos")]
    [XafDisplayName("Historial de Eventos")]
    public XPCollection<SesionTpvEvento> Eventos => GetCollection<SesionTpvEvento>();

    public void RegistrarEvento(TipoEventoSesionTpv tipo, string? descripcion = null, decimal importeAnterior = 0, decimal importeNuevo = 0, string? estadoAnterior = null, string? estadoNuevo = null)
    {
        var service = Session.ServiceProvider?.GetService<ISesionTpvService>();
        var user = (service as SesionTpvService)?.InvokeGetUsuarioActual(Session);

        var evento = new SesionTpvEvento(Session);
        evento.Sesion = this;
        evento.TipoEvento = tipo;
        evento.Usuario = user;
        evento.FechaHora = Tpv?.GetLocalTime() ?? InformacionEmpresaHelper.GetLocalTime(Session);
        evento.Descripcion = descripcion;
        evento.ImporteAnterior = importeAnterior;
        evento.ImporteNuevo = importeNuevo;
        evento.EstadoAnterior = estadoAnterior;
        evento.EstadoNuevo = estadoNuevo;
    }

    [Browsable(false)]
    public bool IsBeingSaved => Session.IsObjectToSave(this);

    [Association("SesionTpv-Movimientos")]
    [XafDisplayName("Movimientos de Caja")]
    public XPCollection<MovimientoCajaTpv> Movimientos => GetCollection<MovimientoCajaTpv>();

    [Browsable(false)]
    public bool EstaAbierta => Estado == EstadoSesionTpv.Abierta;

    [Browsable(false)]
    public bool EstaCerrada => Estado == EstadoSesionTpv.Cerrada;

    [Browsable(false)]
    // Removido temporalmente RuleFromBoolProperty para evitar conflictos con la validación de OnSaving y el servicio de dominio.
    public bool UnaSolaSesionAbierta
    {
        get
        {
            if (Tpv == null || Estado != EstadoSesionTpv.Abierta) return true;
            // Usar InTransaction para que el validador vea los cambios pendientes
            var sesionAbierta = Session.FindObject<SesionTpv>(PersistentCriteriaEvaluationBehavior.InTransaction,
                CriteriaOperator.Parse("Tpv.Oid = ? AND Estado = ? AND Oid != ?", Tpv.Oid, EstadoSesionTpv.Abierta, Oid));
            return sesionAbierta == null;
        }
    }

    [Browsable(false)]
    public ISesionTpvService? SesionTpvService => Session.ServiceProvider?.GetService<ISesionTpvService>();

    private void CambiarEstado(EstadoSesionTpv nuevoEstado)
    {
        if (Estado == nuevoEstado) return;
        
        Estado = nuevoEstado;
        
        if (nuevoEstado == EstadoSesionTpv.Abierta)
        {
            Cierre = null;
            EstadoAbiertoUnico = Guid.NewGuid();
        }
        else
        {
            EstadoAbiertoUnico = null;
        }
    }

    public void AbrirSesion(Tpv tpv, ApplicationUser usuario, decimal importeApertura = 0)
    {
        var service = SesionTpvService;
        if (service == null)
            throw new InvalidOperationException("El servicio de Sesión TPV no está disponible.");

        service.InicializarSesion(this, tpv, usuario, importeApertura);
    }

    public void CerrarSesionAction(string? observaciones = null)
    {
        SesionTpvService?.CerrarSesion(this, null, observaciones);
    }

    public void CerrarSesion(decimal? importeCierreManual = null, string? observaciones = null)
    {
        var service = SesionTpvService;
        if (service == null)
            throw new InvalidOperationException("El servicio de Sesión TPV no está disponible.");

        service.CerrarSesion(this, importeCierreManual, observaciones);
    }

    public void RegistrarMovimiento(TipoMovimientoCajaTpv tipo, decimal importe, string? motivo)
    {
        var service = SesionTpvService;
        if (service == null)
            throw new InvalidOperationException("El servicio de Sesión TPV no está disponible.");

        service.RegistrarMovimiento(this, tipo, importe, motivo);
    }

    public void RetirarEfectivoAction(decimal importe, string motivo)
    {
        RegistrarMovimiento(TipoMovimientoCajaTpv.Retirada, importe, motivo);
    }

    public void ValidarCierre()
    {
        if (SesionTpvService != null)
        {
            if (!SesionTpvService.PuedeCerrarSesion(this, out var error))
                throw new InvalidOperationException(error ?? "No se puede cerrar la sesión.");
        }
        else if (!PuedeCerrarSesion())
        {
            throw new InvalidOperationException("No se puede cerrar la sesión.");
        }
    }

    public bool PuedeCerrarSesion()
    {
        return EstaAbierta && Tpv != null && Usuario != null;
    }

    public decimal CalcularImporteEsperado()
    {
        return SesionTpvService?.CalcularImporteEsperado(this) ?? 0;
    }

    public void ReabrirSesion(string? motivo = null)
    {
        var service = SesionTpvService;
        if (service == null)
            throw new InvalidOperationException("El servicio de Sesión TPV no está disponible.");

        service.ReabrirSesion(this, motivo);
    }

    public void ReabrirSesionAction()
    {
        ReabrirSesion();
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        
        ProcesarNumeracion();

        // Solo validamos si el estado ACTUAL en memoria es Abierta.
        // Si estamos cambiando a Cerrada, no debería saltar esta validación.
        if (Tpv != null && Estado == EstadoSesionTpv.Abierta && !Session.IsObjectToDelete(this))
        {
            // Revalidación en el momento de guardar para evitar condiciones de carrera.
            // Se comprueba si existe OTRA sesión abierta diferente a la actual.
            var sesionAbierta = Session.FindObject<SesionTpv>(
                PersistentCriteriaEvaluationBehavior.InTransaction,
                CriteriaOperator.Parse("Tpv.Oid = ? AND Estado = ? AND Oid != ?", Tpv.Oid, EstadoSesionTpv.Abierta, Oid));
            
            if (sesionAbierta != null)
                throw new UserFriendlyException($"Ya existe otra sesión abierta para este TPV ({Tpv.Nombre}). No se pueden tener dos sesiones abiertas simultáneamente.");
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        // Se deja en estado neutro. La apertura real se gestiona en AbrirSesion.
    }

    private void ProcesarNumeracion()
    {
        if (string.IsNullOrEmpty(Secuencia) && Tpv != null)
            AsignarNumero();
    }

    public virtual void AsignarNumero()
    {
        if (Tpv != null)
        {
            var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
            if (companyInfo == null) return;

            int padding = companyInfo.PaddingNumero;
            string serie = companyInfo.PrefijoSesionTpvPorDefecto ?? "TS";
            int anio = Apertura != DateTime.MinValue ? Apertura.Year : InformacionEmpresaHelper.GetLocalTime(Session).Year;

            string sequenceName = $"{GetType().FullName}.{anio}.{Tpv.Codigo}";
            string prefix = $"{serie}/{anio}/{Tpv.Codigo}";

            Numero = SequenceFactory.GetNextSequence(Session, sequenceName, out var formattedSequence,
                prefix, padding);
            Secuencia = formattedSequence;
        }
    }
}