using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.HandleGraphics;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.HandleGraphics
{
  /// <summary>
  /// See 4.5.5 Graphic printout for more details
  ///
  /// Format
  ///   ESC MFB(P number LF B LF C LF 0 ESC MFE
  /// 
  /// Description
  ///   The command to print the graphics. The command remembers the given number of the graphic and sets
  /// it on the next fiscal or non-fiscal printout before the header (except for error printout or warning printout).
  /// In the case of non-fiscal document, the printout takes place immediately.
  /// 
  /// </summary>
  public class PrintGraphicCommand : IFiscalPrinterCommand<PrintGraphicRequest>
  {
    public void Validate(PrintGraphicRequest input)
    {
      if (input.Graphic == null)
      {
        throw new ArgumentNullException(nameof(input.Graphic), $"{input.Graphic} cannot be null");
      }
    }

    public void BuildRequest(PrintGraphicRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.OpenParenthesis);
      request.Add(FiscalPrinterDividers.P);

      Debug.Assert(input.Graphic != null, "input.Graphic != null");
      request.Add(EncodingHelper.Encode((int)input.Graphic));

      request.Add(FiscalPrinterDividers.Lf);
      request.Add(FiscalPrinterDividers.B);
      request.Add(FiscalPrinterDividers.Lf);
      request.Add(FiscalPrinterDividers.C);
      request.Add(FiscalPrinterDividers.Lf);
      request.Add(FiscalPrinterDividers.Zero);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}