using System;
using Vera.Poland.Models;

namespace Vera.Poland.Helpers
{
  public class VatRateHelper
  {
    private const int RateLowerBound = 0;
    private const int RateUpperBound = 100;

    public const string NullFormatting = "????";
    public const string ExemptFromTaxFormatting = "====";

    public static string GetVatValue(VatItem item)
    {
      if (item.IsNotSet)
      {
        return NullFormatting;
      }

      if (item.IsExemptFromTax)
      {
        return ExemptFromTaxFormatting;
      }

      if (item.RatePercentage < RateLowerBound || item.RatePercentage >= RateUpperBound)
      {
        throw new ArgumentOutOfRangeException(
          nameof(VatItem),
          $"Vat item rate needs to be between {RateLowerBound} and {RateUpperBound}");
      }

      return PadRate(item.RatePercentage);
    }

    private static string PadRate(decimal rate)
    {
      var rateTimes100 = rate * 100;

      return rateTimes100.ToString("0000");
    }
  }
}
