using System.ComponentModel;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Common;

namespace erp.Module.BusinessObjects.Invoicing;

[ImageName("Top10Percent")]
[DefaultProperty(nameof(Code))]
public class InvoiceLineTax(Session session): BaseEntity(session)
{
    private InvoiceLine _invoiceLine;
    private string _code;
    private string _name;
    private string _notes;
    private Account _account;
    private decimal _rate;
    private bool _isCompound;
    private bool _isWithHolding;
    private decimal _taxBase;
    private decimal _amount;
    
    [Association("InvoiceLine-Taxes")]
    public InvoiceLine InvoiceLine
    {
        get => _invoiceLine;
        set => SetPropertyValue(nameof(InvoiceLine), ref _invoiceLine, value);
    }
    
    [RuleRequiredField]
    [RuleUniqueValue]
    public string Code
    {
        get => _code;
        set => SetPropertyValue(nameof(Code), ref _code, value);   
    }
    
    [Size(255)]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }

    [Size(1000)]
    public string Notes
    {
        get => _notes;
        set => SetPropertyValue(nameof(Notes), ref _notes, value);
    }
    
    public Account Account
    {
        get => _account;
        set => SetPropertyValue(nameof(Account), ref _account, value);
    }
    
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal Rate
    {
        get => _rate;
        set => SetPropertyValue(nameof(Rate), ref _rate, value);
    }
    
    public bool IsCompound
    {
        get => _isCompound;
        set => SetPropertyValue(nameof(IsCompound), ref _isCompound, value);
    }

    public bool IsWithHolding
    {
        get => _isWithHolding;
        set => SetPropertyValue(nameof(IsWithHolding), ref _isWithHolding, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal TaxBase
    {
        get => _taxBase;
        protected set => SetPropertyValue(nameof(TaxBase), ref _taxBase, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal Amount
    {
        get => _amount;
        protected set => SetPropertyValue(nameof(Amount), ref _amount, value);   
    }
    
    public override void AfterConstruction()
    {
        base.AfterConstruction();
    }
}