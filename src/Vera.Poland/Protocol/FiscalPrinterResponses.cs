using System;

namespace Vera.Poland.Protocol
{
  public class FiscalPrinterResponses
  {
    /// <summary>
    /// Confirmation of the correct execution of the command
    /// </summary>
    public const byte Ack = 0x06;

    /// <summary>
    /// Confirmation of the incorrect execution of the command
    /// </summary>
    public const byte Nak = 0x15;

    public const byte Lda = 0x23;

    /// <summary>
    /// Returned by the printer responses
    ///
    /// Most of printer responses begin with ESC r
    ///
    /// So we use this to verify a correct response from the printer
    /// </summary>
    public static readonly byte ResponseArgument = Convert.ToByte('r');
  }
}