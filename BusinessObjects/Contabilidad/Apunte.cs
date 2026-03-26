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
[RuleCriteria("Apunte_Cuenta_Activa_Asentable", DefaultContexts.Save, "CuentaContable is null || (CuentaContable.EstaActiva && CuentaContable.EsAsentable)", 
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
    private CuentaContable? _cuenta;
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

    protected bool SetPropertyValueWithValidation<T>(string propertyName, ref T propertyValueHolder, T newValue)
    {
        if (Equals(propertyValueHolder, newValue)) return false;
        EnsureAsientoNotPublished();
        return SetPropertyValue(propertyName, ref propertyValueHolder, newValue);
    }

    [XafDisplayName("Asiento")]
    [Association("Asiento-Apuntes")]
    [RuleRequiredField]
    public Asiento? Asiento
    {
        get => _asiento;
        set => SetPropertyValueWithValidation(nameof(Asiento), ref _asiento, value);
    }
    
    private string? _cuentaBusqueda;
    [NonPersistent]
    [XafDisplayName("Buscar Cuenta Contable")]
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

    [XafDisplayName("Cuenta Contable")]
    [RuleRequiredField]
    [DataSourceCriteria("EstaActiva = True and EsAsentable = True")]
    public CuentaContable? CuentaContable
    {
        get => _cuenta;
        set => SetPropertyValueWithValidation(nameof(CuentaContable), ref _cuenta, value);
    }

    [XafDisplayName("Tercero")]
    [DataSourceCriteria("Activo = True and (IsInstanceOfType(this, 'erp.Module.BusinessObjects.Contactos.Cliente') or IsInstanceOfType(this, 'erp.Module.BusinessObjects.Contactos.Proveedor') or IsInstanceOfType(this, 'erp.Module.BusinessObjects.Contactos.Acreedor'))")]
    public Tercero? Tercero
    {
        get => _tercero;
        set => SetPropertyValueWithValidation(nameof(Tercero), ref _tercero, value);
    }
    
    [XafDisplayName("Concepto")]
    [Size(255)]
    [RuleRequiredField]
    public string? Concepto
    {
        get => _concepto;
        set => SetPropertyValueWithValidation(nameof(Concepto), ref _concepto, value);
    }

    [XafDisplayName("Debe")]
    [ModelDefault("DisplayFormat", "N2")]
    public decimal Debe
    {
        get => _debe;
        set
        {
            if (SetPropertyValueWithValidation(nameof(Debe), ref _debe, value))
            {
                UpdateAsientoSums();
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
            if (SetPropertyValueWithValidation(nameof(Haber), ref _haber, value))
            {
                UpdateAsientoSums();
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
        set => SetPropertyValueWithValidation(nameof(Notas), ref _notas, value);
    }
    
    private void UpdateAsientoSums()
    {
        if (!IsLoading && !IsSaving)
        {
            Asiento?.UpdateSums();
        }
    }

    private string NormalizeCuentaCode(string pattern)
    {
        if (!pattern.Contains('.')) return pattern;

        var partes = pattern.Split('.');
        if (partes.Length != 2) return pattern;

        string prefijo = partes[0];
        string sufijo = partes[1];
        int cerosNecesarios = 10 - prefijo.Length - sufijo.Length;
        
        return cerosNecesarios > 0 
            ? prefijo + new string('0', cerosNecesarios) + sufijo 
            : prefijo + sufijo;
    }

    private void BuscarCuenta(string pattern)
    {
        string searchCode = NormalizeCuentaCode(pattern);

        var cuentaEncontrada = Session.Query<CuentaContable>()
            .FirstOrDefault(c => (c.Codigo == searchCode || c.Codigo!.StartsWith(searchCode)) && c.EstaActiva && c.EsAsentable);
        
        if (cuentaEncontrada != null)
        {
            CuentaContable = cuentaEncontrada;
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
