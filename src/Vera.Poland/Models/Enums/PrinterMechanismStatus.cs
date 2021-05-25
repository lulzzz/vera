using System;

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// 1.11.4 Extended status
  /// </summary>
  [Flags]
  public enum PrinterMechanismStatus : long
  {
    None = 0,

    //
    // Byte 1
    //

    DrawerOpenSignal = 1 << 2,
    PrinterMechanismIsReady = 1 << 3,
    IsPrinterCoverClosed = 1 << 5,
    PaperFeedButtonPressed = 1 << 6,

    //
    // Byte 2
    //

    CutterError = 1 << 11,
    PrintMechanismFirmwareCrcError = 1 << 13,

    //
    // Byte 3
    //

    NearPaperEndSensor = 1 << 17,
    PaperEndSensor = 1 << 19,
  }
}