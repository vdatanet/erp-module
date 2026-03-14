using System.ComponentModel;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.Helpers.Contacts;

namespace erp.Module.BusinessObjects.Accounting;

[DefaultClassOptions]
[NavigationItem("Accounting")]
[ImageName("Actions_Book")]
[DefaultProperty(nameof(Nombre))]
public class Journal(Session session) : BaseEntity(session)
{
    private string _nombre;
    private string _notas;
    private bool _estaActivo;

    [RuleRequiredField]
    [RuleUniqueValue]
    [Size(255)]
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

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    private void InitValues()
    {
        EstaActivo = true;
        var companyInfo = CompanyInfoHelper.GetCompanyInfo(Session);
        if (companyInfo == null) return;
    }
}