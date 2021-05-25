using System;

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// 1.11.5 Fiscal Module internal status readout
  /// </summary>
  [Flags]
  public enum InternalStatusWord9 : long
  {
    //
    // Word 9
    //
    None = 0,
    ClrAllJumperActivated = 1 << 0,
    Reserved = 1 << 1,
    JumperConfigOption = 1 << 2,
    NotAvailable = 1 << 3,
    BootSourceError = 1 << 4,
    NoFlashMemoryDetectedOrNoFlashMemoryOfRequiredCapacity = 1 << 5,
    EjIsAvailable = 1 << 6,
    EjIsOn = 1 << 7,
    SdCardInserted = 1 << 8,
    SdCardInitialized = 1 << 9,
    SdCardFromDifferentPrinter = 1 << 10,
    InternalEjDriveVerificationOngoing = 1 << 11,
    RecordingToInternalEjDriveOngoing = 1 << 12,
    SdCardAlmostFull = 1 << 13,
  }
}