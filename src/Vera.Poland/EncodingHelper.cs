using System;
using System.Globalization;
using System.Text;

namespace Vera.Poland
{
  /// <summary>
  /// All Printer commands should use the helper methods from here, so in case of integration issues
  /// the encoding can be easily changed
  /// </summary>
  public static class EncodingHelper
  {
    private static readonly Encoding _encoding;

    static EncodingHelper()
    {
      _encoding = Encoding.ASCII;
    }

    public static byte[] Encode(string value)
    {
      return _encoding.GetBytes(value);
    }

    public static byte[] Encode(bool value)
    {
      return Encode(value ? 1 : 0);
    }

    public static byte[] Encode(int value)
    {
      return _encoding.GetBytes(value.ToString(CultureInfo.InvariantCulture));
    }

    // "0.00" => returns 0.00 when decimal is 0
    public static byte[] Encode(decimal value, string format = "0.00")
    {
      return _encoding.GetBytes(value.ToString(format, CultureInfo.InvariantCulture));
    }

    public static byte[] Encode(uint value)
    {
      return _encoding.GetBytes(value.ToString(CultureInfo.InvariantCulture));
    }

    public static byte[] ConvertDateToBytes(DateTime date, string format)
    {
      var formattedDate = date.ToString(format);

      return _encoding.GetBytes(formattedDate);
    }

    public static string Decode(byte[] value)
    {
      return _encoding.GetString(value);
    }

    public static decimal DecodeDecimal(byte[] value)
    {
      var asString = Decode(value);
      var result = decimal.Parse(asString);

      return result;
    }

    public static byte[] EncodeQuantity(decimal quantity)
    {
      var integer = decimal.ToInt32(quantity);
      var unencodedValue = quantity - integer > 0
        ? quantity.ToString(CultureInfo.InvariantCulture)
        : integer.ToString();

      return Encode(unencodedValue);
    }
  }
}