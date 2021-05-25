using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.4.1 Fiscal daily report
  /// 
  /// Format
  ///   ESC MFB B [LF c] ESC MFE 
  ///
  /// Description
  ///   Closes the sales period, saves the daily record to the fiscal memory and generates a daily report printout.
  ///
  /// Arguments
  ///   • [LF c] – adding this parameter will create a fiscal daily report in digital form without printing a paper version. 
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class PrintFiscalDailyReportCommand : IFiscalPrinterCommand<PrintFiscalDailyReportRequest>
  {
    public void Validate(PrintFiscalDailyReportRequest input)
    {
    }

    public void BuildRequest(PrintFiscalDailyReportRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.B);

      if (input.DigitalReportOnly)
      {
        // • [LF c] – adding this parameter will create a fiscal daily report in digital form without printing a paper version.
        request.Add(FiscalPrinterDividers.Lf);
        request.Add(FiscalPrinterDividers.c);
      }

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}