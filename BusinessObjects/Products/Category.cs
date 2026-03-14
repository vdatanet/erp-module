using System.Text;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.Helpers.Contacts;

namespace erp.Module.BusinessObjects.Products;

[DefaultClassOptions]
[NavigationItem("Products")]
[ImageName("BO_Product_Group")]
public class Category(Session session) : BaseEntity(session)
{
    private string _nombre;
    private Category _categoriaPadre;
    private bool _estaActivo;
    private bool _disponibleEnTpV;
    private string _notas;

    [Size(255)]
    [RuleRequiredField]
    [RuleUniqueValue]
    public string Nombre
    {
        get => _nombre;
        set => SetPropertyValue(nameof(Nombre), ref _nombre, value);
    }

    [Association("Category-Subcategories")]
    public Category CategoriaPadre
    {
        get => _categoriaPadre;
        set => SetPropertyValue(nameof(CategoriaPadre), ref _categoriaPadre, value);
    }

    public bool EstaActivo
    {
        get => _estaActivo;
        set => SetPropertyValue(nameof(EstaActivo), ref _estaActivo, value);
    }

    public bool DisponibleEnTpv
    {
        get => _disponibleEnTpV;
        set => SetPropertyValue(nameof(DisponibleEnTpv), ref _disponibleEnTpV, value);
    }

    [Size(1000)]
    public string Notas
    {
        get => _notas;
        set => SetPropertyValue(nameof(Notas), ref _notas, value);
    }

    public string RutaCompleta
    {
        get
        {
            var sb = new StringBuilder();
            Category current = this;
            while (current != null)
            {
                if (sb.Length > 0)
                    sb.Insert(0, " > ");
                sb.Insert(0, current.Nombre);
                current = current.CategoriaPadre;
            }

            return sb.ToString();
        }
    }

    [Association("Category-Subcategories")]
    public XPCollection<Category> Subcategories => GetCollection<Category>();

    [Association("Category-Products")] public XPCollection<Product> Products => GetCollection<Product>();

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        EstaActivo = true;
        DisponibleEnTpv = false;
        var companyInfo = CompanyInfoHelper.GetCompanyInfo(Session);
        if (companyInfo == null) return;
    }
}