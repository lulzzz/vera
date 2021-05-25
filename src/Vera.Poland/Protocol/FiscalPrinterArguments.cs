using System;

namespace Vera.Poland.Protocol
{
  public class FiscalPrinterArguments
  {
    /// <summary>
    /// Used as arguments for listing printer errors
    /// </summary>
    public static readonly byte[] ListErrorsArgument =
    {
      Convert.ToByte('+'),
      Convert.ToByte('e')
    };

    /// <summary>
    /// Used as arguments for fetching the printer's long status
    /// </summary>
    public static readonly byte[] LongStatusArgument =
    {
      Convert.ToByte('+'),
      0x0A
    };

    /// <summary>
    /// Used as argument for reading the printer extended status
    /// </summary>
    public static readonly byte[] ReadExtendedStatusArgument =
    {
      Convert.ToByte('+'),
      0x0C
    };

    /// <summary>
    /// Used as argument for reading the printer internal status
    /// </summary>
    public static readonly byte[] ReadInternalStatusArgument =
    {
      Convert.ToByte('+'),
      Convert.ToByte('f')
    };
  }
}