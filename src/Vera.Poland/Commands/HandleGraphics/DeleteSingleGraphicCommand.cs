using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests.HandleGraphics;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.HandleGraphics
{
  /// <summary>
  /// See 4.5.6 Deleting graphics
  /// 
  /// Format
  ///   ESC MFB(E number ESC MFE
  /// 
  /// Arguments
  ///   • number = 0 – removes all images
  ///   • number = 1 to 8 – removes given images
  /// </summary>
  /// <example>
  ///   ESC MFB (E1 ESC MFE 
  ///     ACK
  ///</example>
  public class DeleteSingleGraphicCommand : IFiscalPrinterCommand<DeleteSingleGraphicRequest>
  {
    public void Validate(DeleteSingleGraphicRequest input)
    {
      if (input.Graphic == null)
      {
        throw new ArgumentNullException(nameof(input.Graphic), $"{input.Graphic} cannot be null");
      }
    }

    public void BuildRequest(DeleteSingleGraphicRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);

      request.Add(FiscalPrinterDividers.OpenParenthesis);
      request.Add(FiscalPrinterDividers.E);

      Debug.Assert(input.Graphic != null, "input.Graphic != null");
      request.AddRange(EncodingHelper.Encode((int)input.Graphic));

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}