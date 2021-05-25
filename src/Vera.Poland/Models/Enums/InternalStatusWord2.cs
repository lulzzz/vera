using System;

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// 1.11.5 Fiscal Module internal status readout
  /// </summary>
  [Flags]
  public enum InternalStatusWord2 : long
  {
    //
    // Word 2
    //
    None = 0,
    DisconnectedMemory = 1 << 0,
    CorruptOrNotEmptyMemory = 1 << 1,
    ImproperlyFormattedMemory = 1 << 2,
    MemoryDamagedOrInitializedForOtherCountry = 1 << 3,
    MemoryReplaced = 1 << 4,
    MemoryFormatted = 1 << 5,
    MemoryDoesNotContainFiscalDailyRecords = 1 << 6,
    DataWriteOperationIsAvailable = 1 << 7,
    MemoryInReadonlyMode = 1 << 8,
    UnlockedMemoryAccess = 1 << 9,
    ProducerDataSavedInFiscalMemory = 1 << 10,
    FiscalMemoryInitialized = 1 << 11,
    SerialNumberOfTerminalSavedInMemory = 1 << 12,
    FiscalMemoryNumberStoredInMemory = 1 << 13,
    MemoryIsEmpty = 1 << 14,
    MemoryContainsUnknownData = 1 << 15,
  }
}