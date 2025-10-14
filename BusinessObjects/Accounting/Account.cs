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
[DefaultProperty(nameof(Code))]
public class Account(Session session): BaseEntity(session)
{
    
    private string _code;
    private string _name;
    private string _notes;
    private Account _parentAccount;
    private bool _isActive;
    private bool _isPostable;
    private AccountType _type;
    private AccountNature _nature;
    
    [RuleRequiredField]
    [RuleUniqueValue]
    public string Code
    {
        get => _code;
        set => SetPropertyValue(nameof(Code), ref _code, value);
    }
    
    [Size(255)]
    [RuleRequiredField]

    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);    
    }
    
    [Size(SizeAttribute.Unlimited)]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }
        
    [Association("Parent-ChildrenAccounts")]
    public Account ParentAccount
    {
        get => _parentAccount;
        set => SetPropertyValue(nameof(ParentAccount), ref _parentAccount, value);
    }
    
    public string FullPath {
        get {
            var sb = new StringBuilder();
            Account current = this;
            while (current != null) {
                if (sb.Length > 0)
                    sb.Insert(0, " > ");
                sb.Insert(0, current.Code);
                current = current.ParentAccount;
            }
            return sb.ToString();
        }
    }
    
    public bool IsActive
    {
        get => _isActive;
        set => SetPropertyValue(nameof(IsActive), ref _isActive, value);
    }
    
    public bool IsPostable
    {
        get => _isPostable;
        set => SetPropertyValue(nameof(IsPostable), ref _isPostable, value);
    }
    
    public AccountType Type
    {
        get => _type;
        set => SetPropertyValue(nameof(Type), ref _type, value);
    }
    
    public AccountNature Nature
    {
        get => _nature;
        set => SetPropertyValue(nameof(Nature), ref _nature, value);
    }
    
    [Association("Parent-ChildrenAccounts")]
    public XPCollection<Account> ChildrenAccounts => GetCollection<Account>();
    
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        InitValues();
    }

    private void InitValues()
    {
        IsActive = true;
        IsPostable = false;
        Type = AccountType.Asset;
        Nature = AccountNature.Debit;
    }
    
    public enum AccountType
    {
        Asset,
        Liability,
        Equity,
        Revenue,
        Expense,
        Result
    }

    public enum AccountNature
    {
        Debit,
        Credit
    }
}