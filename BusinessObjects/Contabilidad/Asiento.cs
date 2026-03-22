using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
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
    private decimal _sumaDebe;
    private decimal _sumaHaber;
    private string? _notas;
    private EstadoAsiento _estado;

    [XafDisplayName("Diario")]
    [RuleRequiredField]
    [DataSourceCriteria("EstaActivo = True")]
    public Diario? Diario
    {
        get => _diario;
        set => SetPropertyValue(nameof(Diario), ref _diario, value);
    }

    [XafDisplayName("Fecha")]
    [RuleRequiredField]
    [RuleRange("Asiento_FechaEnEjercicio", DefaultContexts.Save, "Ejercicio.FechaInicio", "Ejercicio.FechaFin", ParametersMode.Expression, TargetContextIDs = "Save")]
    public DateTime Fecha
    {
        get => _fecha;
        set => SetPropertyValue(nameof(Fecha), ref _fecha, value);
    }

    [XafDisplayName("Ejercicio")]
    [RuleRequiredField]
    [Association("Ejercicio-Asientos")]
    public Ejercicio? Ejercicio
    {
        get => _ejercicio;
        set => SetPropertyValue(nameof(Ejercicio), ref _ejercicio, value);
    }

    [XafDisplayName("Serie")]
    [RuleRequiredField]
    [Size(50)]
    public string? Serie
    {
        get => _serie;
        set => SetPropertyValue(nameof(Serie), ref _serie, value);
    }

    [XafDisplayName("Número")]
    [ModelDefault("AllowEdit", "False")]
    public int Numero
    {
        get => _numero;
        set => SetPropertyValue(nameof(Numero), ref _numero, value);
    }

    [XafDisplayName("Código")]
    [RuleUniqueValue]
    [Size(100)]
    [ModelDefault("AllowEdit", "False")]
    public string? Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }

    [XafDisplayName("Orden")]
    [ModelDefault("AllowEdit", "False")]
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
        set => SetPropertyValue(nameof(Concepto), ref _concepto, value);
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
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [XafDisplayName("Estado")]
    public EstadoAsiento Estado
    {
        get => _estado;
        set => SetPropertyValue(nameof(Estado), ref _estado, value);
    }

    [Action(Caption = "Publicar Asiento", ConfirmationMessage = "El asiento pasará a estar Publicado y no podrá ser modificado. ¿Desea continuar?", ImageName = "Action_LinkUnlink_Link", TargetObjectsCriteria = "Estado = 'Borrador'")]
    public void PublicarAsiento()
    {
        if (Saldo != 0)
        {
            throw new UserFriendlyException("No se puede publicar un asiento descuadrado.");
        }
        Estado = EstadoAsiento.Publicado;
    }

    [Action(Caption = "Volver a Borrador", ImageName = "Action_Reset", TargetObjectsCriteria = "Estado = 'Publicado'")]
    public void ResetBorrador()
    {
        Estado = EstadoAsiento.Borrador;
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
            Codigo = $"{Serie}/{Numero:D5}";
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
            // Supongamos que hay un campo de prefijo por defecto en InformacionEmpresa que usaremos para Serie
            // Si no existe, usaremos uno genérico AS/Anio
            Serie = $"AS/{Fecha.Year}";
            Diario = companyInfo.DiarioVentasPorDefecto;
        }
        else
        {
            Serie = $"AS/{Fecha.Year}";
        }
    }
}

public enum EstadoAsiento
{
    Borrador,
    Publicado
}
