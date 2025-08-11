using System.ComponentModel;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Common;

namespace erp.Module.BusinessObjects.Invoicing;

[ImageName("Top10Percent")]
[DefaultProperty(nameof(TaxType))]
public class InvoiceLineTax(Session session): BaseEntity(session)
{
    private InvoiceLine _invoiceLine;
    private TaxType _taxType;
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

    public TaxType TaxType
    {
        get => _taxType;
        set
        {
            if (SetPropertyValue(nameof(TaxType), ref _taxType, value)) 
                ApplyTaxTypeSnapshot(value);
        }
    }

    private void ApplyTaxTypeSnapshot(TaxType t)
    {
        if (IsLoading || IsSaving)
        {
            return;
        }

        if (t == null)
        {
            Name = null;
            Notes = null;
            Account = null;
            Rate = 0m;
            IsCompound = false;
            IsWithHolding = false;
            return;
        }

        Name = t.Name;
        Notes = t.Notes;
        Account = t.Account;
        Rate = t.Rate;
        IsCompound = t.IsCompound;
        IsWithHolding = t.IsWithHolding;
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
        set => SetPropertyValue(nameof(TaxBase), ref _taxBase, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal Amount
    {
        get => _amount;
        set => SetPropertyValue(nameof(Amount), ref _amount, value);   
    }
    
    public override void AfterConstruction()
    {
        base.AfterConstruction();
    }
}