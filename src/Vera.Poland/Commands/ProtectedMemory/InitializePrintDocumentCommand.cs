using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.ProtectedMemory
{
  /// <summary>
  /// 1.10.1 Initialize printout from Protected Memory
  ///
  /// Format
  ///   ESC MFB HEB ESC MFE
  /// Description
  ///   Starts the process of printing data from protected memory.
  ///   Header lines are printed on top of the printout.
  /// </summary>
  public class InitializePrintDocumentCommand : IFiscalPrinterCommand
  {
    public void BuildRequest(List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);

      request.Add(FiscalPrinterDividers.H);
      request.Add(FiscalPrinterDividers.E);
      request.Add(FiscalPrinterDividers.B);

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}