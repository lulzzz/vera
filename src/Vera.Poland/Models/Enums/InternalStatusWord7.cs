using System;

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// 1.11.5 Fiscal Module internal status readout
  /// </summary>
  [Flags]
  public enum InternalStatusWord7 : long
  {
    //
    // Word 7
    //
    None = 0,
    ReceiptHeaderIsSet = 1 << 0,
    OperatorIdIsSet = 1 << 4,
    TerminalIdIsSet = 1 << 5,
    DefinedStaticFooter = 1 << 6,
    VatRateTableIsSet = 1 << 7,
    ActivationRequired = 1 << 12,
    PrinterBlockedActivationRequired = 1 << 13,
    SummerTime = 1 << 14,
    EjIsOff = 1 << 15
  }
}