using System.Text;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Base.Common;
using erp.Module.BusinessObjects.Helpers.Contacts;

namespace erp.Module.BusinessObjects.Products;

[DefaultClassOptions]
[NavigationItem("Products")]
[ImageName("BO_Product_Group")]
public class Category(Session session) : BaseEntity(session)
{
    private string _name;
    private Category _parentCategory;
    private bool _isActive;
    private bool _isAvailableInPos;
    private string _notes;

    [Size(255)]
    [RuleRequiredField]
    [RuleUniqueValue]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    [Association("Category-Subcategories")]
    public Category ParentCategory
    {
        get => _parentCategory;
        set => SetPropertyValue(nameof(ParentCategory), ref _parentCategory, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetPropertyValue(nameof(IsActive), ref _isActive, value);
    }

    public bool IsAvailableInPos
    {
        get => _isAvailableInPos;
        set => SetPropertyValue(nameof(IsAvailableInPos), ref _isAvailableInPos, value);
    }

    [Size(1000)]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }

    public string FullPath
    {
        get
        {
            var sb = new StringBuilder();
            Category current = this;
            while (current != null)
            {
                if (sb.Length > 0)
                    sb.Insert(0, " > ");
                sb.Insert(0, current.Name);
                current = current.ParentCategory;
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
        IsActive = true;
        IsAvailableInPos = false;
        var companyInfo = CompanyInfoHelper.GetCompanyInfo(Session);
        if (companyInfo == null) return;
    }
}