using System;

namespace Vera.Poland.Protocol
{
  /// <summary>
  /// Commands defined within the F210 protocol for communication with fiscal printers.
  ///
  /// See 1.2 Basic information for more details
  /// </summary>
  public class FiscalPrinterCommands
  {
    /// <summary>
    /// Beginning of the command
    /// </summary>
    public const byte Esc = 0x1B;

    /// <summary>
    /// ID of the beginning of the command
    /// </summary>
    public const byte Mfb = 0x80;

    /// <summary>
    /// ID of the end of the order
    /// </summary>
    public const byte Mfe = 0x83;

    /// <summary>
    /// ID of the sequence start
    /// </summary>
    public const byte Mfb1 = 0x81;

    /// <summary>
    /// ID of the sequence start
    /// </summary>
    public const byte Mfb2 = 0x82;

    /// <summary>
    /// 
    /// </summary>
    public const byte Dle = 0x10;

    public static readonly byte[] SetSettingCommand =
    {
      Convert.ToByte('%'),
      Convert.ToByte('s')
    };

    public static readonly byte[] SaveSettingCommand =
    {
      Convert.ToByte('%'),
      Convert.ToByte('c')
    };
  }
}