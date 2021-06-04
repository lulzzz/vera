using System.Globalization;

namespace Vera.Sweden.Utils
{
  public static class MiscUtils
  {
    private static readonly CultureInfo SwedenCultureInfo = new("sv-SE");

    public static string FormatDecimalWithTwoPlaces(decimal? number)
    {
      if (number == null)
      {
        return "0,00";
      }

      var formattedForSweden = string.Format(SwedenCultureInfo, "{0:0.00,}", number);
      // Funky; Note that for negative values, the MINUS "-" sign is "−" for SwedenCulture
      // However, Infrasec does not like that, we get a generic ServiceUnavailable error response,
      // so change the SwedenCulture minus sign with the standard/default one
      return formattedForSweden.Replace("−", "-");
    }
  }
}