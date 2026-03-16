namespace erp.Module.Helpers.Comun;

public static class MoneyMath
{
    public static decimal RoundMoney(decimal value, int digits = 2)
    {
        return Math.Round(value, digits, MidpointRounding.AwayFromZero);
    }

    public static decimal RoundMoney(decimal value, int digits, MidpointRounding mode)
    {
        return Math.Round(value, digits, mode);
    }
}