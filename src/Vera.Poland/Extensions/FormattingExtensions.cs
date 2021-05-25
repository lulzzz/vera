using System.Collections.Generic;

namespace Vera.Poland.Extensions
{
  public static class FormattingExtensions
  {
    public static IEnumerable<byte> ToFixedPointByteArray(this decimal value)
    {
      var formattedFixedPointValue = value.ToFixedPointFormatting();

      return EncodingHelper.Encode(formattedFixedPointValue);
    }

    public static string ToFixedPointFormatting(this decimal value)
    {
      return value.ToString("F");
    }
  }
}
