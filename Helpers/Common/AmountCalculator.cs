namespace erp.Module.Helpers.Common;

public static class AmountCalculator
{
    public static decimal GetTaxableAmount(decimal quantity, decimal unitPrice, decimal discountPercent)
    {
        return MoneyMath.RoundMoney(quantity * unitPrice * (1 - discountPercent / 100m), 2);
    }
    public static decimal GetTaxAmount(decimal taxableAmount, decimal rate, bool isWithHolding)
    {
        var sign = isWithHolding ? -1m : 1m;
        return MoneyMath.RoundMoney(taxableAmount * (rate / 100m) * sign);
    }
}