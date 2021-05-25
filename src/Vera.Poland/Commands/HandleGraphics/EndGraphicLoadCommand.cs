using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.HandleGraphics
{
  /// <summary>
  /// See 4.5.3 The end of loading graphics
  ///
  /// Format
  ///   ESC MFB ( L A ESC MFE
  ///
  /// Description
  ///   The command starts the process of sending image data to the mechanism of the printer.
  ///   A reply response is sent when all data has been stored in the printer's RAM
  /// </summary>
  /// <example>
  ///   ESC MFB (LA ESC MFE
  ///     ACK
  /// </example>
  public class EndGraphicLoadCommand : IFiscalPrinterCommand
  {
    public void BuildRequest(List<byte> request)
    {
      request.Add(
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.OpenParenthesis);

      request.Add(FiscalPrinterDividers.L, FiscalPrinterDividers.A);

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}