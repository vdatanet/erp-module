using System.ComponentModel;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Comun;
using erp.Module.BusinessObjects.Contactos;

namespace erp.Module.BusinessObjects.Auxiliares;

[DefaultClassOptions]
[NavigationItem("Auxiliares")]
[ImageName("Business_Bank")]
[XafDisplayName("Banco")]
[DefaultProperty(nameof(Nombre))]
public class Banco(Session session) : EntidadBase(session)
{
    private string? _bic;
    private Cliente? _cliente;
    private string? _iban;
    private string? _nombre;
    private string? _notas;

    [RuleRequiredField]
    [Size(255)]
    [XafDisplayName("Nombre")]
    public string? Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [XafDisplayName("IBAN")]
    public string? Iban
    {
        get => _iban;
        set => SetPropertyValue(nameof(Iban), ref _iban, value);
    }

    [XafDisplayName("BIC")]
    public string? Bic
    {
        get => _bic;
        set => SetPropertyValue(nameof(Bic), ref _bic, value);
    }

    [Association("Cliente-Bancos")]
    [XafDisplayName("Cliente")]
    [DataSourceCriteria("Activo = true")]
    public Cliente? Cliente
    {
        get => _cliente;
        set => SetPropertyValue(nameof(Cliente), ref _cliente, value);
    }

    [Size(SizeAttribute.Unlimited)]
    [XafDisplayName("Notas")]
    public string? Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }
}