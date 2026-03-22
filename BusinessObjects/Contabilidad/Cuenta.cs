using System.ComponentModel;
using System.Text;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Contabilidad;

[DefaultClassOptions]
[NavigationItem("Contabilidad")]
[ImageName("CustomerContactDirectory")]
[DefaultProperty(nameof(Codigo))]
public class Cuenta(Session session) : EntidadBase(session)
{
    public enum NaturalezaCuenta
    {
        Deudora,
        Acreedora
    }

    public enum TipoCuenta
    {
        Activo,
        Pasivo,
        PatrimonioNeto,
        Ingresos,
        Gastos,
        Resultados
    }

    private string? _codigo;
    private Cuenta? _cuentaPadre;
    private bool _esAsentable;
    private bool _estaActiva;
    private NaturalezaCuenta _naturaleza;
    private string? _nombre;
    private string? _notas;
    private TipoCuenta _tipo;

    [RuleRequiredField]
    [RuleUniqueValue]
    [XafDisplayName("Código")]
    public string? Codigo
    {
        get => _codigo;
        set
        {
            if (value != null && value.Contains('.'))
            {
                var partes = value.Split('.');
                if (partes.Length == 2)
                {
                    string prefijo = partes[0];
                    string sufijo = partes[1];
                    int cerosNecesarios = 10 - prefijo.Length - sufijo.Length;
                    if (cerosNecesarios > 0)
                    {
                        value = prefijo + new string('0', cerosNecesarios) + sufijo;
                    }
                }
            }

            if (SetPropertyValue(nameof(Codigo), ref _codigo, value))
            {
                if (!IsLoading && !IsSaving && !string.IsNullOrEmpty(value))
                {
                    ActualizarPadre();
                }
            }
        }
    }

    private void ActualizarPadre()
    {
        if (string.IsNullOrEmpty(Codigo))
        {
            CuentaPadre = null;
            return;
        }

        string? codigoPadre = null;
        int longitud = Codigo.Length;

        if (longitud == 2) // Nivel 2 (10), padre Nivel 1 (1)
        {
            codigoPadre = Codigo.Substring(0, 1);
        }
        else if (longitud == 3) // Nivel 3 (100), padre Nivel 2 (10)
        {
            codigoPadre = Codigo.Substring(0, 2);
        }
        else if (longitud == 4 || longitud == 5) // Nivel 4 (1000 o 10000), padre Nivel 3 (100)
        {
            codigoPadre = Codigo.Substring(0, 3);
        }
        else if (longitud > 5) // Nivel 5 (ej. 10 dígitos), padre Nivel 4 (5 dígitos)
        {
            if (longitud >= 5)
            {
                codigoPadre = Codigo.Substring(0, 5);
            }
            if (longitud == 10)
            {
                EsAsentable = true;
            }
        }

        if (codigoPadre != null)
        {
            CuentaPadre = Session.FindObject<Cuenta>(PersistentCriteriaEvaluationBehavior.InTransaction, new BinaryOperator(nameof(Codigo), codigoPadre));
        }
        else
        {
            CuentaPadre = null;
        }
    }

    [PersistentAlias("Iif(Len(Codigo) == 1, 1, Iif(Len(Codigo) == 2, 2, Iif(Len(Codigo) == 3, 3, Iif(Len(Codigo) == 4 || Len(Codigo) == 5, 4, Iif(Len(Codigo) > 5, 5, 0)))))")]
    [XafDisplayName("Nivel")]
    public int Nivel => (int)EvaluateAlias(nameof(Nivel));

    [Browsable(false)]
    public bool CodigoEsValido => true;

    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    [Association("CuentaPadre-CuentasHijas")]
    [XafDisplayName("Cuenta Padre")]
    public Cuenta? CuentaPadre
    {
        get => _cuentaPadre;
        set => SetPropertyValue(nameof(CuentaPadre), ref _cuentaPadre, value);
    }

    [XafDisplayName("Ruta Completa")]
    public string RutaCompleta
    {
        get
        {
            var sb = new StringBuilder();
            var current = this;
            while (current != null)
            {
                if (sb.Length > 0)
                    sb.Insert(0, " > ");
                sb.Insert(0, current.Codigo);
                current = current.CuentaPadre;
            }

            return sb.ToString();
        }
    }

    [XafDisplayName("Activa")]
    public bool EstaActiva
    {
        get => _estaActiva;
        set => SetPropertyValue(nameof(EstaActiva), ref _estaActiva, value);
    }

    [XafDisplayName("Asentable")]
    public bool EsAsentable
    {
        get => _esAsentable;
        set => SetPropertyValue(nameof(EsAsentable), ref _esAsentable, value);
    }

    [XafDisplayName("Tipo")]
    public TipoCuenta Tipo
    {
        get => _tipo;
        set => SetPropertyValue(nameof(Tipo), ref _tipo, value);
    }

    [XafDisplayName("Naturaleza")]
    public NaturalezaCuenta Naturaleza
    {
        get => _naturaleza;
        set => SetPropertyValue(nameof(Naturaleza), ref _naturaleza, value);
    }

    [Association("CuentaPadre-CuentasHijas")]
    [XafDisplayName("Cuentas Hijas")]
    public XPCollection<Cuenta> CuentasHijas => GetCollection<Cuenta>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        EstaActiva = true;
        EsAsentable = false;
        Tipo = TipoCuenta.Activo;
        Naturaleza = NaturalezaCuenta.Deudora;
    }
}