using System;
using System.Globalization;

namespace Vera.Extensions
{
    public static class DecimalExtensions
    {
        public static string FormatCurrency(this decimal d, CultureInfo culture) => Math.Abs(d).ToString("C", culture);
        public static string FormatTaxRate(this decimal r, CultureInfo culture) => (r - 1).ToString("P0", culture);
        public static decimal Round(this decimal d, int decimals) => Math.Round(Math.Abs(d), decimals);
        public static decimal RoundKeepSign(this decimal d, int decimals) => d < 0 ? -d.Round(decimals) : d.Round(decimals); 
        public static decimal RoundAwayFromZero(this decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        public static decimal Absolute(this decimal amount) => Math.Abs(amount);
    }
}
