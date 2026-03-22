using System.ComponentModel;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.Helpers.Contactos;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace erp.Module.BusinessObjects.Contabilidad;

[DefaultClassOptions]
[NavigationItem("Contabilidad")]
[XafDisplayName("Asiento")]
[DefaultProperty(nameof(Codigo))]
[ImageName("BO_List")]
[RuleCriteria("Asiento_NoEliminablePublicado", DefaultContexts.Delete, "Estado != 'Publicado'", "Un asiento publicado no puede ser eliminado.", SkipNullOrEmptyValues = false, TargetContextIDs = "Delete")]
[Appearance("Asiento_Publicado_Deshabilitado", AppearanceItemType = "ViewItem", TargetItems = "*", Criteria = "Estado = 'Publicado'", Enabled = false)]
public class Asiento(Session session) : EntidadBase(session)
{
    private Diario? _diario;
    private DateTime _fecha;
    private Ejercicio? _ejercicio;
    private string? _serie;
    private int _numero;
    private string? _codigo;
    private int _orden;
    private string? _concepto;
    private string? _notas;
    private EstadoAsiento _estado;

    private void EnsureNotPublished()
    {
        if (!IsLoading && !IsSaving && Estado == EstadoAsiento.Publicado)
        {
            throw new UserFriendlyException("No se puede modificar un asiento publicado.");
        }
    }

    [XafDisplayName("Diario")]
    [RuleRequiredField]
    [DataSourceCriteria("EstaActivo = True")]
    public Diario? Diario
    {
        get => _diario;
        set
        {
            if (value != _diario)
            {
                EnsureNotPublished();
                SetPropertyValue(nameof(Diario), ref _diario, value);
            }
        }
    }

    [XafDisplayName("Fecha")]
    [RuleRequiredField]
    [RuleRange("Asiento_FechaEnEjercicio", DefaultContexts.Save, "Ejercicio.FechaInicio", "Ejercicio.FechaFin", ParametersMode.Expression, TargetContextIDs = "Save")]
    public DateTime Fecha
    {
        get => _fecha;
        set
        {
            if (value != _fecha)
            {
                EnsureNotPublished();
                SetPropertyValue(nameof(Fecha), ref _fecha, value);
            }
        }
    }

    [XafDisplayName("Ejercicio")]
    [RuleRequiredField]
    [Association("Ejercicio-Asientos")]
    public Ejercicio? Ejercicio
    {
        get => _ejercicio;
        set
        {
            if (value != _ejercicio)
            {
                EnsureNotPublished();
                SetPropertyValue(nameof(Ejercicio), ref _ejercicio, value);
            }
        }
    }

    [XafDisplayName("Serie")]
    [RuleRequiredField]
    [Size(50)]
    [NonCloneable]
    public string? Serie
    {
        get => _serie;
        set
        {
            if (value != _serie)
            {
                EnsureNotPublished();
                SetPropertyValue(nameof(Serie), ref _serie, value);
            }
        }
    }

    [XafDisplayName("Número")]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public int Numero
    {
        get => _numero;
        set => SetPropertyValue(nameof(Numero), ref _numero, value);
    }

    [XafDisplayName("Código")]
    [RuleUniqueValue]
    [Size(100)]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [XafDisplayName("Orden")]
    [ModelDefault("AllowEdit", "False")]
    [NonCloneable]
    public int Orden
    {
        get => _orden;
        set => SetPropertyValue(nameof(Orden), ref _orden, value);
    }

    [XafDisplayName("Concepto")]
    [Size(255)]
    [RuleRequiredField]
    public string? Concepto
    {
        get => _concepto;
        set
        {
            if (value != _concepto)
            {
                EnsureNotPublished();
                SetPropertyValue(nameof(Concepto), ref _concepto, value);
            }
        }
    }

    [Persistent(nameof(SumaDebe))]
    private decimal _persistentSumaDebe;
    [XafDisplayName("Suma Debe")]
    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "N2")]
    [PersistentAlias(nameof(_persistentSumaDebe))]
    public decimal SumaDebe => _persistentSumaDebe;

    [Persistent(nameof(SumaHaber))]
    private decimal _persistentSumaHaber;
    [XafDisplayName("Suma Haber")]
    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "N2")]
    [PersistentAlias(nameof(_persistentSumaHaber))]
    public decimal SumaHaber => _persistentSumaHaber;

    [XafDisplayName("Saldo")]
    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "N2")]
    [RuleFromBoolProperty("Asiento_SaldoCeroParaPublicar", DefaultContexts.Save, "Solo los asientos en Borrador pueden tener un saldo diferente a cero.", TargetContextIDs = "Save")]
    public bool IsSaldoValido => Estado == EstadoAsiento.Borrador || Saldo == 0;

    public decimal Saldo => SumaDebe - SumaHaber;

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set
        {
            if (value != _notas)
            {
                EnsureNotPublished();
                SetPropertyValue(nameof(Notas), ref _notas, value);
            }
        }
    }

    [XafDisplayName("Estado")]
    [NonCloneable]
    public EstadoAsiento Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    public void TogglePublicado()
    {
        if (Estado == EstadoAsiento.Borrador)
        {
            if (Saldo != 0)
            {
                throw new UserFriendlyException("No se puede publicar un asiento descuadrado.");
            }
            Estado = EstadoAsiento.Publicado;
        }
        else if (Estado == EstadoAsiento.Publicado)
        {
            Estado = EstadoAsiento.Borrador;
        }
    }

    [Browsable(false)]
    [RuleFromBoolProperty("Asiento_FechaNoBloqueada", DefaultContexts.Save, "La fecha del asiento se encuentra en un periodo bloqueado del ejercicio.", UsedProperties = nameof(Fecha))]
    public bool IsFechaNoBloqueada
    {
        get
        {
            if (Ejercicio == null) return true;
            if (Ejercicio.Estado == EstadoEjercicio.Bloqueado) return false;
            
            return !Ejercicio.PeriodosBloqueados.Any(p => Fecha >= p.FechaInicio && Fecha <= p.FechaFin);
        }
    }

    [Aggregated, Association("Asiento-Apuntes")]
    [XafDisplayName("Apuntes")]
    public XPCollection<Apunte> Apuntes => GetCollection<Apunte>(nameof(Apuntes));

    public void UpdateSums()
    {
        decimal oldSumaDebe = _persistentSumaDebe;
        decimal oldSumaHaber = _persistentSumaHaber;

        decimal tempSumaDebe = 0;
        decimal tempSumaHaber = 0;
        foreach (var apunte in Apuntes)
        {
            tempSumaDebe += apunte.Debe;
            tempSumaHaber += apunte.Haber;
        }

        if (_persistentSumaDebe != tempSumaDebe)
            SetPropertyValue(nameof(SumaDebe), ref _persistentSumaDebe, tempSumaDebe);
        if (_persistentSumaHaber != tempSumaHaber)
            SetPropertyValue(nameof(SumaHaber), ref _persistentSumaHaber, tempSumaHaber);

        if (oldSumaDebe != _persistentSumaDebe || oldSumaHaber != _persistentSumaHaber)
        {
            OnChanged(nameof(Saldo));
        }
    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(this))
        {
            if (Numero == 0 && Ejercicio != null)
            {
                var maxNumero = Session.Query<Asiento>()
                    .Where(a => a.Ejercicio == Ejercicio && a.Serie == Serie)
                    .Max(a => (int?)a.Numero) ?? 0;
                Numero = maxNumero + 1;
            }

            if (Orden == 0 && Ejercicio != null)
            {
                var maxOrden = Session.Query<Asiento>()
                    .Where(a => a.Ejercicio == Ejercicio)
                    .Max(a => (int?)a.Orden) ?? 0;
                Orden = maxOrden + 1;
            }
        }
        
        if (string.IsNullOrEmpty(Codigo) && !string.IsNullOrEmpty(Serie) && Numero > 0)
        {
            var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session) ?? throw new UserFriendlyException("No se ha podido obtener la configuración de la empresa.");
            int padding = companyInfo.PaddingNumero;
            Codigo = $"{Serie}/{Numero.ToString().PadLeft(padding, '0')}";
        }

        base.OnSaving();
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        Estado = EstadoAsiento.Borrador;
        Fecha = DateTime.Today;
        
        Ejercicio = Session.Query<Ejercicio>().FirstOrDefault(e => e.Estado == EstadoEjercicio.Abierto && e.FechaInicio <= Fecha && e.FechaFin >= Fecha);
        
        var companyInfo = InformacionEmpresaHelper.GetInformacionEmpresa(Session);
        if (companyInfo != null)
        {
            string? prefijo = companyInfo.PrefijoAsientosPorDefecto;
            if (!string.IsNullOrEmpty(prefijo))
            {
                Serie = $"{prefijo}/{Fecha.Year}";
            }
            Diario = companyInfo.DiarioVentasPorDefecto;
        }
    }
}

public enum EstadoAsiento
{
    Borrador,
    Publicado
}
