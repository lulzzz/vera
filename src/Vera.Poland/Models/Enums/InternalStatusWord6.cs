using System;

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// 1.11.5 Fiscal Module internal status readout
  /// </summary>
  [Flags]
  public enum InternalStatusWord6 : long
  {
    //
    // Word 6
    //
    None = 0,
    FiscalDayIsOpened = 1 << 0,
    ReceiptHeaderIsPrintedBeforeFirstSale = 1 << 1,
    RamResetJumperOn = 1 << 13,
    BatteryVoltageHoldingTheRamFiscalMemoryBelowTheLowerLimit = 1 << 14,  
    FiscalModuleStartupProcedureHasBeenCompleted = 1 << 15
  }
}