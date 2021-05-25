using System;

namespace Vera.Poland.Models.Enums
{
  [Flags]
  public enum LongStatus
  {
    FiscalDeviceInReadonlyMode = 1 << 0,
    FiscalLevel = 1 << 1,
    SalePeriod = 1 << 2,
    Receipt = 1 << 3,
    NotUsed1 = 1 << 4,
    NotUsed2 = 1 << 5,
    ReceiptSummarized = 1 << 6,
    NonFiscalDocumentPrintoutOngoing = 1 << 7
  }
}