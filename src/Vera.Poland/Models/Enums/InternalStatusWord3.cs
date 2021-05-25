using System;

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// 1.11.5 Fiscal Module internal status readout
  /// </summary>
  [Flags]
  public enum InternalStatusWord3 : long
  {
    //
    // Word 3
    //
    None = 0,
    LessThan30DaysRecordEntries = 1 << 0,
    MemoryIsFull = 1 << 1,
    LessThan5FreeTaxRecordsAvailable = 1 << 2,
    NoTaxRecordEntriesAvailable = 1 << 3,
    LessThan20SecondsOfServiceInterventionRecords = 1 << 4,
    NoServiceEntryRecordsAvailable = 1 << 5,
    DamagedDayRecords = 1 << 12,
    CorruptDataInitiatingFiscalMemory = 1 << 13,
    MoreThan50PercentMemoryIsUsed = 1 << 14,
    MoreThan75PercentMemoryIsUsed = 1 << 15
  }
}