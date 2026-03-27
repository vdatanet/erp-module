using System.Globalization;

namespace erp.Module.Helpers.Comun;

public static class DiscountParser
{
    public static (decimal d1, decimal d2, decimal d3) Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (0, 0, 0);

        var parts = text.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        decimal d1 = parts.Length > 0 ? ParseDecimal(parts[0]) : 0;
        decimal d2 = parts.Length > 1 ? ParseDecimal(parts[1]) : 0;
        decimal d3 = parts.Length > 2 ? ParseDecimal(parts[2]) : 0;

        return (d1, d2, d3);
    }

    public static string Format(decimal d1, decimal d2, decimal d3)
    {
        if (d1 == 0 && d2 == 0 && d3 == 0) return string.Empty;
        if (d3 != 0) return $"{d1:G29}+{d2:G29}+{d3:G29}";
        if (d2 != 0) return $"{d1:G29}+{d2:G29}";
        return $"{d1:G29}";
    }

    private static decimal ParseDecimal(string value)
    {
        if (decimal.TryParse(value.Replace(',', '.'), CultureInfo.InvariantCulture, out var result))
            return result;
        return 0;
    }
}
