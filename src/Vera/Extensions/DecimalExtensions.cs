using System;
using System.Globalization;

namespace Vera.Extensions
{
    public static class DecimalExtensions
    {
        public static string FormatCurrency(this decimal d, CultureInfo culture) => Math.Abs(d).ToString("C", culture);
        public static string FormatTaxRate(this decimal r, CultureInfo culture) => (r - 1).ToString("P0", culture);
    }
}
