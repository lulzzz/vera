namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// The range that the report covers can be defined in one of the following ways:
  /// • From date to date
  /// • From the daily report number to the daily report number
  /// • Monthly report
  /// • Report for the entire period - the report contains all daily records stored in fiscal memory
  /// </summary>
  public enum PeriodicReportType
  {
    FromDateToDate = 1,
    FromNumberToNumber = 2,
    TotalMonthlyFiscalReport = 3,
    EntireMemory = 4
  }
}