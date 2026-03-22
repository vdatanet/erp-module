using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;

namespace erp.Module.BusinessObjects.Contabilidad;

[DefaultClassOptions]
[NavigationItem("Contabilidad")]
[XafDisplayName("Apunte")]
[ImageName("BO_Lead")]
[RuleCriteria("Apunte_Cuenta_Activa_Asentable", DefaultContexts.Save, "Cuenta is null || (Cuenta.EstaActiva && Cuenta.EsAsentable)", 
    "La cuenta debe ser activa y asentable.")]
[RuleCriteria("Apunte_Tercero_Activo", DefaultContexts.Save, "Tercero is null || Tercero.Activo", 
    "El tercero debe estar activo.")]
[RuleCriteria("Apunte_Tercero_Tipo_Valido", DefaultContexts.Save, "Tercero is null || IsInstanceOfType(Tercero, 'erp.Module.BusinessObjects.Contactos.Cliente') || IsInstanceOfType(Tercero, 'erp.Module.BusinessObjects.Contactos.Proveedor') || IsInstanceOfType(Tercero, 'erp.Module.BusinessObjects.Contactos.Acreedor')", 
    "El tercero debe ser Cliente, Proveedor o Acreedor.")]
[RuleCriteria("Apunte_NoEliminableAsientoPublicado", DefaultContexts.Delete, "Asiento is null || Asiento.Estado != 'Publicado'", "No se puede eliminar un apunte de un asiento publicado.", SkipNullOrEmptyValues = false, TargetContextIDs = "Delete")]
[Appearance("Apunte_AsientoPublicado_Deshabilitado", AppearanceItemType = "ViewItem", TargetItems = "*", Criteria = "Asiento.Estado = 'Publicado'", Enabled = false)]
public class Apunte(Session session) : EntidadBase(session)
{
    private Asiento? _asiento;
    private Cuenta? _cuenta;
    private Tercero? _tercero;
    private string? _concepto;
    private decimal _debe;
    private decimal _haber;
    private string? _notas;

    private void EnsureAsientoNotPublished()
    {
        if (!IsLoading && !IsSaving && Asiento?.Estado == EstadoAsiento.Publicado)
        {
            throw new UserFriendlyException("No se puede modificar un apunte de un asiento publicado.");
        }
    }

    [XafDisplayName("Asiento")]
    [Association("Asiento-Apuntes")]
    [RuleRequiredField]
    public Asiento? Asiento
    {
        get => _asiento;
        set
        {
            if (value != _asiento)
            {
                EnsureAsientoNotPublished();
                SetPropertyValue(nameof(Asiento), ref _asiento, value);
            }
        }
    }
    
    private string? _cuentaBusqueda;
    [NonPersistent]
    [XafDisplayName("Buscar Cuenta")]
    [ToolTip("Permite buscar por código (ej. 430.1 para 4300000001)")]
    public string? CuentaBusqueda
    {
        get => _cuentaBusqueda;
        set
        {
            if (SetPropertyValue(nameof(CuentaBusqueda), ref _cuentaBusqueda, value) && !string.IsNullOrEmpty(value))
            {
                BuscarCuenta(value);
            }
        }
    }

    [XafDisplayName("Cuenta")]
    [RuleRequiredField]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public Cuenta? Cuenta
    {
        get => _cuenta;
        set
        {
            if (value != _cuenta)
            {
                EnsureAsientoNotPublished();
                SetPropertyValue(nameof(Cuenta), ref _cuenta, value);
            }
        }
    }

    [XafDisplayName("Tercero")]
    [DataSourceCriteria("Activo = True and (IsInstanceOfType(this, 'erp.Module.BusinessObjects.Contactos.Cliente') or IsInstanceOfType(this, 'erp.Module.BusinessObjects.Contactos.Proveedor') or IsInstanceOfType(this, 'erp.Module.BusinessObjects.Contactos.Acreedor'))")]
    public Tercero? Tercero
    {
        get => _tercero;
        set
        {
            if (value != _tercero)
            {
                EnsureAsientoNotPublished();
                SetPropertyValue(nameof(Tercero), ref _tercero, value);
            }
        }
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
                EnsureAsientoNotPublished();
                SetPropertyValue(nameof(Concepto), ref _concepto, value);
            }
        }
    }

    [XafDisplayName("Debe")]
    [ModelDefault("DisplayFormat", "N2")]
    public decimal Debe
    {
        get => _debe;
        set
        {
            if (value != _debe)
            {
                EnsureAsientoNotPublished();
                if (SetPropertyValue(nameof(Debe), ref _debe, value))
                {
                    if (!IsLoading && !IsSaving && Asiento != null)
                    {
                        Asiento.UpdateSums();
                    }
                }
            }
        }
    }

    [XafDisplayName("Haber")]
    [ModelDefault("DisplayFormat", "N2")]
    public decimal Haber
    {
        get => _haber;
        set
        {
            if (value != _haber)
            {
                EnsureAsientoNotPublished();
                if (SetPropertyValue(nameof(Haber), ref _haber, value))
                {
                    if (!IsLoading && !IsSaving && Asiento != null)
                    {
                        Asiento.UpdateSums();
                    }
                }
            }
        }
    }

    [XafDisplayName("Saldo")]
    [ModelDefault("AllowEdit", "False")]
    [ModelDefault("DisplayFormat", "N2")]
    public decimal Saldo => Debe - Haber;

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set
        {
            if (value != _notas)
            {
                EnsureAsientoNotPublished();
                SetPropertyValue(nameof(Notas), ref _notas, value);
            }
        }
    }
    
    private void BuscarCuenta(string pattern)
    {
        // Lógica: si contiene un punto, expandimos el primer dígito después del punto con ceros
        // Ejemplo: 430.1 -> 4300000001 (si la longitud es 10)
        string searchCode = pattern;
        if (pattern.Contains('.'))
        {
            var partes = pattern.Split('.');
            if (partes.Length == 2)
            {
                string prefijo = partes[0];
                string sufijo = partes[1];
                int cerosNecesarios = 10 - prefijo.Length - sufijo.Length;
                if (cerosNecesarios > 0)
                {
                    searchCode = prefijo + new string('0', cerosNecesarios) + sufijo;
                }
                else
                {
                    searchCode = prefijo + sufijo;
                }
            }
        }
        else
        {
            searchCode = pattern;
        }

        var cuentaEncontrada = Session.Query<Cuenta>()
            .FirstOrDefault(c => (c.Codigo == searchCode || c.Codigo!.StartsWith(searchCode)) && c.EstaActiva && c.EsAsentable);
        
        if (cuentaEncontrada != null)
        {
            Cuenta = cuentaEncontrada;
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Asiento != null)
        {
            Concepto = Asiento.Concepto;
        }
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (propertyName == nameof(Asiento))
        {
            if (oldValue is Asiento oldAsiento)
            {
                oldAsiento.UpdateSums();
            }
            if (newValue is Asiento newAsiento)
            {
                newAsiento.UpdateSums();
                if (string.IsNullOrEmpty(Concepto))
                {
                    Concepto = newAsiento.Concepto;
                }
            }
        }
    }
}
