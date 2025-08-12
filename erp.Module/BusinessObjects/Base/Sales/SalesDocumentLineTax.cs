using System.ComponentModel;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using erp.Module.BusinessObjects.Accounting;
using erp.Module.BusinessObjects.Base.Common;

namespace erp.Module.BusinessObjects.Base.Sales;

[ImageName("Top10Percent")]
[DefaultProperty(nameof(TaxKind))]
public class SalesDocumentLineTax(Session session) : BaseEntity(session)
{
    private SalesDocumentLine _salesDocumentLine;
    private TaxKind _taxKind;
    private string _name;
    private string _notes;
    private int _sequence;
    private Account _account;
    private decimal _rate;
    private bool _isCompound;
    private bool _isWithHolding;
    private decimal _taxableAmount;
    private decimal _taxAmount;

    [Association("SalesDocumentLine-Taxes")]
    public SalesDocumentLine SalesDocumentLine
    {
        get => _salesDocumentLine;
        set => SetPropertyValue(nameof(SalesDocumentLine), ref _salesDocumentLine, value);
    }

    public TaxKind TaxKind
    {
        get => _taxKind;
        set
        {
            if (SetPropertyValue(nameof(TaxKind), ref _taxKind, value))
                ApplyTaxTypeSnapshot(value);
        }
    }

    private void ApplyTaxTypeSnapshot(TaxKind t)
    {
        if (IsLoading || IsSaving) return;

        if (t == null)
        {
            Name = null;
            Notes = null;
            Sequence = 0;
            Account = null;
            Rate = 0m;
            IsCompound = false;
            IsWithHolding = false;
            return;
        }

        Name = t.Name;
        Notes = t.Notes;
        Sequence = t.Sequence;
        Account = t.Account;
        Rate = t.Rate;
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

    public int Sequence
    {
        get => _sequence;
        set => SetPropertyValue(nameof(Sequence), ref _sequence, value);
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
    public decimal TaxableAmount
    {
        get => _taxableAmount;
        set => SetPropertyValue(nameof(TaxableAmount), ref _taxableAmount, value);
    }

    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    [ModelDefault("AllowEdit", "False")]
    public decimal TaxAmount
    {
        get => _taxAmount;
        set => SetPropertyValue(nameof(TaxAmount), ref _taxAmount, value);
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
    }
}