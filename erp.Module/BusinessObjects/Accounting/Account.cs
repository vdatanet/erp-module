using DevExpress.Xpo;
using erp.Module.BusinessObjects.Common;

namespace erp.Module.BusinessObjects.Accounting;

public class Account(Session session): BaseEntity(session)
{
    
    private string _code;
    private string _name;
    private string _description;
    private int _level;
    private Account _parentAccount;
    private bool _isActive;
    private bool _isPostable;
    private AccountType _type;
    private AccountNature _nature;
    
    public string Code
    {
        get => _code;
        set => SetPropertyValue(nameof(Code), ref _code, value);
    }
    
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);    
    }
    
    public string Description
    {
        get => _description;
        set => SetPropertyValue(nameof(Description), ref _description, value);
    }
    
    public int Level
    {
        get => _level;
        set => SetPropertyValue(nameof(Level), ref _level, value);  
    }
    
    [Association("Account-Subaccounts")]
    public Account ParentAccount
    {
        get => _parentAccount;
        set => SetPropertyValue(nameof(ParentAccount), ref _parentAccount, value);
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
    
    [Association("Account-Subaccounts")]
    public XPCollection<Account> Subaccounts => GetCollection<Account>(nameof(Subaccounts));
    
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