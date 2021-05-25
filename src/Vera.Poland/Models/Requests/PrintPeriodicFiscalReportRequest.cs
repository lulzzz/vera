using System;
using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests
{
  /// <summary>
  /// See PrintPeriodicFiscalReportCommand for details
  /// </summary>
  public class PrintPeriodicFiscalReportRequest : PrinterRequest
  {
    public PeriodicReportType PeriodicReportType { get; set; }
    public DateTime? FromDate { get; set; }  // Normal DateTime?, the printer will handle dd-MM-yy
    public DateTime? ToDate { get; set; } // Normal DateTime?, the printer will handle dd-MM-yy
    public DateTime? MonthlyReport { get; set; } // Normal DateTime?, the printer will handle dd-MM
    public uint? FromNumber { get; set; } // fiscal daily reports from x to y 
    public uint? ToNumber { get; set; } // fiscal daily reports from x to y
  }
}