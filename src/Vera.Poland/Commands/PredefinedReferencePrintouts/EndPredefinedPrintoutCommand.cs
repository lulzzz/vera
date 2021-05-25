using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.PredefinedReferencePrintouts
{
  /// <summary>
  /// See 1.6.4 The end of the predefined printout
  /// </summary>
  public class EndPredefinedPrintoutCommand : IFiscalPrinterCommand
  {
    public void BuildRequest(List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.N);
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}