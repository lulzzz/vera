using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.4.3 Periodic fiscal report
  ///
  /// Description
  ///   Prints a periodic fiscal report.
  ///   The report is generated on the basis of entered data range and based on daily reports saved in the fiscal memory. A summary labeled
  ///   "Total Periodic Report" is printed at the end of each report. The report is a data reading and it is not saved by the printer in electronic form.
  /// The range that the report covers can be defined in one of the following ways:
  ///   • From date to date
  ///   • From the daily report number to the daily report number
  ///   • Monthly report
  ///   • Report for the entire period - the report contains all daily records stored in fiscal memory
  /// 
  /// Arguments:
  /// • report_type - selects the type of report:
  ///   o i – from date to date
  ///   o n – from number to number
  ///   o m – monthly (with label „Monthly fiscal report”
  ///   o a – entire memory
  /// 
  /// • from - to - gives the range of report. Data is interpreted in the context of the report type:
  ///   o i - (from date to date) from - to = dd-mm-yy, the beginning and end of the date to be included in the report,
  ///   o n - (from number to number of the fiscal daily reports) from - to = numbers of the fiscal daily reports,
  ///   o m - (monthly report) from = mm-yy, defines month and a year, to; can be ignored
  ///   o a - (a report from the whole range of memory) Arguments from and to; can be ignored, but if given, they are ignored anyway.
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class PrintPeriodicFiscalReportCommand : IFiscalPrinterCommand<PrintPeriodicFiscalReportRequest>
  {
    private const string FullDateFormat = "dd-MM-yy";
    private const string MonthlyDateFormat = "MM-yy";


    public void Validate(PrintPeriodicFiscalReportRequest input)
    {
      switch (input.PeriodicReportType)
      {
        case PeriodicReportType.FromDateToDate:
        {
          if (!input.FromDate.HasValue)
          {
            throw new ArgumentNullException(nameof(input.FromDate));
          }

          if (!input.ToDate.HasValue)
          {
            throw new ArgumentNullException(nameof(input.ToDate));
          }

          break;
        }
        case PeriodicReportType.FromNumberToNumber:
        {
          if (!input.FromNumber.HasValue)
          {
            throw new ArgumentNullException(nameof(input.FromNumber));
          }

          if (!input.ToNumber.HasValue)
          {
            throw new ArgumentNullException(nameof(input.ToNumber));
          }

          break;
        }
        case PeriodicReportType.TotalMonthlyFiscalReport:
        {
          if (input.MonthlyReport == null)
          {
            throw new ArgumentNullException(nameof(input.MonthlyReport));
          }

          var requestedForThisMonth = DateTime.Today.Month == input.MonthlyReport.Value.Month
                                      && DateTime.Today.Year == input.MonthlyReport.Value.Year;

          if (requestedForThisMonth)
          {
            throw new ArgumentOutOfRangeException(nameof(input.MonthlyReport), "Cannot generate report for current month");
          }

          break;
        }
        case PeriodicReportType.EntireMemory:
        {
          break;
        }
        default:
          throw new ArgumentOutOfRangeException(nameof(input.PeriodicReportType));
      }
    }

    public void BuildRequest(PrintPeriodicFiscalReportRequest input, List<byte> request)
    {
      // Acceptance conditions
      //  • Neutral state
      //  • open sales period
      //  • read - only mode

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.H);

      switch (input.PeriodicReportType)
      {
        case PeriodicReportType.FromDateToDate:
          {
            request.Add(FiscalPrinterDividers.i);

            Debug.Assert(input.FromDate != null, "input.FromDate != null");
            request.AddRange(EncodingHelper.ConvertDateToBytes(input.FromDate.Value, FullDateFormat));

            request.Add(FiscalPrinterCommands.Esc);
            request.Add(FiscalPrinterCommands.Mfb1);

            Debug.Assert(input.ToDate != null, "input.ToDate != null");
            request.AddRange(EncodingHelper.ConvertDateToBytes(input.ToDate.Value, FullDateFormat));


            break;
          }
        case PeriodicReportType.FromNumberToNumber:
          {
            request.Add(FiscalPrinterDividers.n);

            Debug.Assert(input.FromNumber != null, "input.FromNumber != null");
            request.AddRange(EncodingHelper.Encode(input.FromNumber.Value));

            request.Add(FiscalPrinterCommands.Esc);
            request.Add(FiscalPrinterCommands.Mfb1);

            Debug.Assert(input.ToNumber != null, "input.ToNumber != null");
            request.AddRange(EncodingHelper.Encode(input.ToNumber.Value));

            break;
          }
        case PeriodicReportType.TotalMonthlyFiscalReport:
          {
            request.Add(FiscalPrinterDividers.m);
            Debug.Assert(input.MonthlyReport != null, "input.MonthlyReport != null");
            request.AddRange(EncodingHelper.ConvertDateToBytes(input.MonthlyReport.Value, MonthlyDateFormat));

            break;
          }
        case PeriodicReportType.EntireMemory:
          {
            request.Add(FiscalPrinterDividers.a);

            break;
          }
        // Validated
        //default:
        //  throw new ArgumentOutOfRangeException(nameof(input.PeriodicReportType));
      }

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}