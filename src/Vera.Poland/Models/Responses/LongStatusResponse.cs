using System;

namespace Vera.Poland.Models.Responses
{
  public class LongStatusResponse : PrinterResponse
  {
    public bool IsFiscalDeviceInReadonlyMode { get; set; }

    public bool IsPreFiscal { get; set; }

    public bool IsFiscalDayOpen { get; set; }

    public bool IsReceiptOpen { get; set; }

    public bool IsReceiptSummarized { get; set; }

    public bool IsNonFiscalDocumentPrintoutOngoing { get; set; }

    public bool IsDisplayTypeAlphanumeric { get; set; }

    public bool IsXonXoffProtocolOn { get; set; }

    public DateTime PrinterTime { get; set; }

    public string ReceiptCounter { get; set; }

    public DateTime LastDailyReportDate { get; set; }

    public string LastDailyReportNumber { get; set; }
  }
}