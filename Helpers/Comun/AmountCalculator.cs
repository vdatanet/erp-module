namespace erp.Module.Helpers.Comun;

public static class AmountCalculator
{
    public static decimal GetTaxableAmount(decimal quantity, decimal unitPrice, decimal discountPercent)
    {
        return MoneyMath.RoundMoney(quantity * unitPrice * (1 - discountPercent / 100m));
    }

    public static decimal GetTaxableAmountCascading(decimal quantity, decimal unitPrice, decimal d1, decimal d2, decimal d3)
    {
        var total = quantity * unitPrice;
        total *= (1 - d1 / 100m);
        total *= (1 - d2 / 100m);
        total *= (1 - d3 / 100m);
        return MoneyMath.RoundMoney(total);
    }

    public static decimal GetTaxAmount(decimal taxableAmount, decimal rate, bool isWithHolding)
    {
        var sign = isWithHolding ? -1m : 1m;
        return MoneyMath.RoundMoney(taxableAmount * (rate / 100m) * sign);
    }
}