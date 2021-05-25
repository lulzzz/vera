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
  /// See 4.4.4 Total periodic report
  /// 
  /// Format
  ///   ESC MFB H <report_type> [<from > ESC MFB1 <to> ] ESC MFE
  /// 
  /// Description
  ///   Prints total periodic report.The range that report covers can be defined by:
  ///   • From date to date
  ///   • From the daily report number to the daily report number
  ///   • Monthly
  ///   • Report for the entire period - the report contains all daily records stored in fiscal memory
  /// 
  /// Arguments
  ///   • <report_type> - selects the type of report:
  ///     o k – from date to date
  ///     o j – from number to number
  ///     o m s – total monthly fiscal report
  ///     o b - entire memory
  ///   • <from>, <to> - gives the range of report.Data is interpreted in the context of the report type:
  ///       o k - (from date to date) <from>, <to> = dd-mm-yy, the beginning and end of the date to be included in the report,
  ///       o j - (from number to number of the fiscal daily reports) <from>, <to> - numbers of the fiscal daily reports,
  ///       o m s – total monthly report <from> = mm-rr, specifies month and year, <to> can be ignored
  ///       o b - (a report from the whole range of memory) Arguments <from> and<to> can be ignored, but if given, they are ignored anyway.
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class PrintTotalPeriodicReportCommand : IFiscalPrinterCommand<PrintTotalPeriodicReportRequest>
  {
    private const string FullDateFormat = "dd-MM-yy";
    private const string MonthlyDateFormat = "MM-yy";

    public void Validate(PrintTotalPeriodicReportRequest input)
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
          if (input.TotalMonthlyReport == null)
          {
            throw new ArgumentNullException(nameof(input.TotalMonthlyReport));
          }

          var requestedForThisMonth = DateTime.Today.Month == input.TotalMonthlyReport.Value.Month
                                      && DateTime.Today.Year == input.TotalMonthlyReport.Value.Year;

          if (requestedForThisMonth)
          {
            throw new ArgumentOutOfRangeException(nameof(input.TotalMonthlyReport), "Cannot generate report for current month");
          }

          break;
        }
        case PeriodicReportType.EntireMemory:
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(input.PeriodicReportType));
      }
    }

    public void BuildRequest(PrintTotalPeriodicReportRequest input, List<byte> request)
    {
      // Acceptance conditions
      //  • Neutral state
      //  • Sales period open
      //  • read - only state

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.H);

      switch (input.PeriodicReportType)
      {
        case PeriodicReportType.FromDateToDate:
          {
            request.Add(FiscalPrinterDividers.k);
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
            request.Add(FiscalPrinterDividers.j);
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
            request.Add(FiscalPrinterDividers.s);

            Debug.Assert(input.TotalMonthlyReport != null, "input.TotalMonthlyReport != null");
            request.AddRange(EncodingHelper.ConvertDateToBytes(input.TotalMonthlyReport.Value, MonthlyDateFormat));

            break;
          }
        case PeriodicReportType.EntireMemory:
          {
            request.Add(FiscalPrinterDividers.b);

            break;
          }
      }

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}
