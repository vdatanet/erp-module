using System.ComponentModel;
using System.Text;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;

namespace erp.Module.BusinessObjects.Contabilidad;

[DefaultClassOptions]
[NavigationItem("Contabilidad")]
[ImageName("ClienteContactoDirectory")]
[DefaultProperty(nameof(Codigo))]
public class Cuenta(Session session): EntidadBase(session)
{
    
    private string _codigo;
    private string _nombre;
    private string _notas;
    private Cuenta _cuentaPadre;
    private bool _estaActiva;
    private bool _esAsentable;
    private TipoCuenta _tipo;
    private NaturalezaCuenta _naturaleza;
    
    [RuleRequiredField]
    [RuleUniqueValue]
    [XafDisplayName("Código")]
    public string Codigo
    {
        get => _codigo;
        set => SetPropertyValue(nameof(Codigo), ref _codigo, value);
    }
    
    [Size(255)]
    [RuleRequiredField]
    [XafDisplayName("Nombre")]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);    
    }
    
    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }
        
    [Association("CuentaPadre-CuentasHijas")]
    [XafDisplayName("Cuenta Padre")]
    public Cuenta CuentaPadre
    {
        get => _cuentaPadre;
        set => SetPropertyValue(nameof(CuentaPadre), ref _cuentaPadre, value);
    }
    
    [XafDisplayName("Ruta Completa")]
    public string RutaCompleta {
        get {
            var sb = new StringBuilder();
            Cuenta current = this;
            while (current != null) {
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
    public XPCollection<Cuenta> CuentasHijas => GetCollection<Cuenta>(nameof(CuentasHijas));
    
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