using System.ComponentModel;
using System.Text;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Accounting;

[DefaultClassOptions]
[NavigationItem("Accounting")]
[ImageName("CustomerContactDirectory")]
[DefaultProperty(nameof(Codigo))]
public class Account(Session session): BaseEntity(session)
{
    
    private string _codigo;
    private string _nombre;
    private string _notas;
    private Account _cuentaPadre;
    private bool _estaActiva;
    private bool _esAsentable;
    private TipoCuenta _tipo;
    private NaturalezaCuenta _naturaleza;
    
    [RuleRequiredField]
    [RuleUniqueValue]
    public string Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }
    
    [Size(255)]
    [RuleRequiredField]

    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);    
    }
    
    [Size(SizeAttribute.Unlimited)]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }
        
    [Association("Parent-ChildrenAccounts")]
    public Account CuentaPadre
    {
        get => _cuentaPadre;
        set => SetPropertyValue(nameof(CuentaPadre), ref _cuentaPadre, value);
    }
    
    public string RutaCompleta {
        get {
            var sb = new StringBuilder();
            Account current = this;
            while (current != null) {
                if (sb.Length > 0)
                    sb.Insert(0, " > ");
                sb.Insert(0, current.Codigo);
                current = current.CuentaPadre;
            }
            return sb.ToString();
        }
    }
    
    public bool EstaActiva
    {
        get => _estaActiva;
        set => SetPropertyValue(nameof(EstaActiva), ref _estaActiva, value);
    }
    
    public bool EsAsentable
    {
        get => _esAsentable;
        set => SetPropertyValue(nameof(EsAsentable), ref _esAsentable, value);
    }
    
    public TipoCuenta Tipo
    {
        get => _tipo;
        set => SetPropertyValue(nameof(Tipo), ref _tipo, value);
    }
    
    public NaturalezaCuenta Naturaleza
    {
        get => _naturaleza;
        set => SetPropertyValue(nameof(Naturaleza), ref _naturaleza, value);
    }
    
    [Association("Parent-ChildrenAccounts")]
    public XPCollection<Account> CuentasHijas => GetCollection<Account>();
    
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
    
    public enum TipoCuenta
    {
        Activo,
        Pasivo,
        PatrimonioNeto,
        Ingresos,
        Gastos,
        Resultados
    }

    public enum NaturalezaCuenta
    {
        Deudora,
        Acreedora
    }
}