namespace erp.Module.BusinessObjects.Common
{
    public static class MoneyMath
    {
        public static decimal RoundMoney(decimal value, int digits = 2)
            => Math.Round(value, digits, MidpointRounding.AwayFromZero);
        
        public static decimal RoundMoney(decimal value, int digits, MidpointRounding mode)
            => Math.Round(value, digits, mode);
    }
}