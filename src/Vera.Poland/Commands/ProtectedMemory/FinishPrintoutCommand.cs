using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.ProtectedMemory
{
  /// <summary>
  /// 1.10.3 Finish printout from Protected Memory
  /// 
  /// Format:
  ///   ESC MFB HEE ESC MFE
  /// 
  /// Description
  ///   Finishes printing documents read from Protected Memory.
  /// </summary>
  public class FinishPrintoutCommand : IFiscalPrinterCommand
  {
    public void BuildRequest(List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);

      request.Add(FiscalPrinterDividers.H);
      request.Add(FiscalPrinterDividers.E);
      request.Add(FiscalPrinterDividers.E);

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}