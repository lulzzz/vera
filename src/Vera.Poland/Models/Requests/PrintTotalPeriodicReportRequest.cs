using System;
using Vera.Poland.Models.Enums;

namespace Vera.Poland.Models.Requests
{
  public class PrintTotalPeriodicReportRequest : PrinterRequest
  {
    public PeriodicReportType PeriodicReportType { get; set; }
    public DateTime? FromDate { get; set; }  // Normal DateTime?, the printer will handle dd-MM-yy
    public DateTime? ToDate { get; set; } // Normal DateTime?, the printer will handle dd-MM-yy
    public DateTime? TotalMonthlyReport { get; set; } // Normal DateTime?, the printer will handle mm-yy
    public uint? FromNumber { get; set; } // fiscal daily reports from x to y 
    public uint? ToNumber { get; set; } // fiscal daily reports from x to y
  }
}