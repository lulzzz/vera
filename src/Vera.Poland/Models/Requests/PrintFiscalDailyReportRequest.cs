namespace Vera.Poland.Models.Requests
{
  public class PrintFiscalDailyReportRequest : PrinterRequest
  {
    /// <summary>
    /// If true, will create a fiscal daily report in digital form without printing a paper version.
    /// </summary>
    public bool DigitalReportOnly { get; set; }
  }
}