using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.4.7 Printout of X report
  ///
  /// Description
  ///   Prints a non-fiscal report that does not close sales and does not save data in fiscal memory.
  /// 
  /// Format
  ///   ESC MFB O ESC MFE
  /// </summary>
  public class PrintXReportCommand : IFiscalPrinterCommand
  {
    public void BuildRequest(List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.O);
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}