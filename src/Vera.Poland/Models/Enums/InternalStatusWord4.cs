using System;

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// 1.11.5 Fiscal Module internal status readout
  /// </summary>
  [Flags]
  public enum InternalStatusWord4 : long
  {
    //
    // Word 4
    //
    None = 0,
    RollPaperNotLoaded = 1 << 0,
    RollPaperNearEndSensor = 1 << 4,
    CoverOpen = 1 << 6,
    PaperJam = 1 << 7,
    OverheatingOfThermalHead = 1 << 8,
    AutoCutterError = 1 << 9,
    FatalError = 1 << 10,
    PrinterIsOnLine = 1 << 11,
    PrinterSwitchedOffLineByFiscalModule = 1 << 12,
    PrinterWithoutAnswerToStatusQuery = 1 << 13,
    PrinterThermalHeadIsOpened = 1 << 14,
    PrinterNotConnectedToFiscalModule = 1 << 15
  }
}