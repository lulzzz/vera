using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.HandleGraphics
{
  /// <summary>
  /// 4.5.4 Canceling graphics loading
  /// 
  /// Description
  ///   The command interrupts the process of sending image data to the mechanism of the printer.
  ///
  /// Format
  ///   ESC MFB(L C ESC MFE
  /// </summary>
  /// <example>
  ///
  /// Example
  ///  ESC MFB(L C ESC MFE
  ///   ACK
  /// </example>
  public class CancelGraphicLoadingCommand : IFiscalPrinterCommand
  {
    public void BuildRequest(List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);

      request.Add(FiscalPrinterDividers.OpenParenthesis);
      request.Add(FiscalPrinterDividers.L);
      request.Add(FiscalPrinterDividers.C);

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}