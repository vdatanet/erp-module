namespace erp.Module.BusinessObjects.Helpers.Common;

public static class AmountCalculator
{
    public static decimal GetTaxableAmount(decimal quantity, decimal unitPrice, decimal discountPercent)
    {
        return Math.Round(quantity * unitPrice * (1 - discountPercent / 100m), 2);
    }
}