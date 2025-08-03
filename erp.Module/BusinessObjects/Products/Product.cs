using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Common;
using erp.Module.BusinessObjects.Helpers;
using Task = erp.Module.BusinessObjects.Common.Task;

namespace erp.Module.BusinessObjects.Products;

[DefaultClassOptions]
[NavigationItem("Products")]
[ImageName("BO_Product")]
public class Product(Session session) : BaseEntity(session)
{
    private string _code;
    private string _barcode;
    private string _name;
    private string _description;
    private Category _category;
    private decimal _standardCost;
    private decimal _priceList;
    private bool _isActive;
    private bool _isAvailableInPos;
    private string _notes;
    private MediaDataObject _picture;
    
    [RuleUniqueValue]
    public string Code
    {
        get => _code;
        set => SetPropertyValue(nameof(Code), ref _code, value);
    }

    [RuleUniqueValue]
    public string Barcode
    {
        get => _barcode;
        set => SetPropertyValue(nameof(Barcode), ref _barcode, value);
    }

    [RuleUniqueValue]
    [RuleRequiredField]
    [Size(255)]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    [Size(1000)]
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }

    [Association("Category-Products")]
    public Category Category
    {
        get => _category;
        set => SetPropertyValue(nameof(Category), ref _category, value);
    }

    public decimal StandardCost
    {
        get => _standardCost;
        set => SetPropertyValue(nameof(StandardCost), ref _standardCost, value);
    }

    public decimal PriceList
    {
        get => _priceList;
        set => SetPropertyValue(nameof(PriceList), ref _priceList, value);
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

    public MediaDataObject Picture
    {
        get => _picture;
        set => SetPropertyValue(nameof(Picture), ref _picture, value);
    }
    
    [Aggregated]
    [Association("Product-Tasks")]
    public XPCollection<Task> Tasks => GetCollection<Task>(nameof(Tasks)); 
    
    [Aggregated]
    [Association("Product-Pictures")]
    public XPCollection<Picture> Pictures => GetCollection<Picture>(nameof(Pictures));
    
    [Aggregated]
    [Association("Product-Attachments")]
    public XPCollection<Attachment> Attachments => GetCollection<Attachment>(nameof(Attachments));
    
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
        if (companyInfo?.Name != null) Name = companyInfo.Name;
    }
}