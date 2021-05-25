using System;

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// 1.11.5 Fiscal Module internal status readout
  /// </summary>
  [Flags]
  public enum InternalStatusWord5 : long
  {
    //
    // Word 5
    //
    None = 0,
    PrinterIsNotInitialized = 1 << 0,
    PrinterIsInitialized = 1 << 1,
    SetPreFiscalMode = 1 << 2,
    SetFiscalModeButNotYetActive = 1 << 4,
    FiscalModeIsActivated = 1 << 5,
    SecurityModeWasSetWhenAFatalErrorOccurred = 1 << 6,
    TechnologicalMode = 1 << 9,
    PrinterInEuroMode = 1 << 12,
    PrinterWithScheduledDataOfTransitionToEuro = 1 << 13,
    InternalTestMode = 1 << 14,
    CertificationMode = 1 << 15
  }
}