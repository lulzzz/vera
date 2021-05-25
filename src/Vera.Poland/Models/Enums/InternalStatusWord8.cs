using System;

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// 1.11.5 Fiscal Module internal status readout
  /// </summary>
  [Flags]
  public enum InternalStatusWord8 : long
  {
    //
    // Word 8
    //
    None = 0,
    PrinterConnectedToEj = 1 << 0,
    DataIsBeingTransferredToEjServer = 1 << 1,
    DataTransferredToEj = 1 << 2,
    DataTransferError = 1 << 3,
    InitializationErrorOfTransferDataToEj = 1 << 4,
    SavingToEjAllowed = 1 << 5,
    FlashMemoryFullIn50Percent = 1 << 6,
    FlashMemoryAlmostFull = 1 << 7,
    FlashMemoryFull = 1 << 8,
    FlashMemoryDataDamaged = 1 << 9,
    NoFlashMemoryDetected = 1 << 10,
    EjIsEmpty = 1 << 11,
    OneOfTheEjBlocksIsOpenForWriting = 1 << 12,
    AtLeastOneOfTheJsBlocksWaitingToBeDumpOrErased = 1 << 13,
    NoEjBlockCanBeOpenedButItCanBeDropped = 1 << 14,
    EjIsReady = 1 << 15
  }
}