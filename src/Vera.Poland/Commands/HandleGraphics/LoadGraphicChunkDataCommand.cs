using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.HandleGraphics;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.HandleGraphics
{
  /// <summary>
  /// See 4.5.2 Loading graphic data
  /// 
  /// Format
  ///   ESC MFB(L D MSB LSB D0...D(N-1) ESC MFE
  /// 
  /// Arguments
  ///   • MSB LSB - determining the ordering of bytes in a packet of data sent to the printer
  ///   • N – amount of data bytes, N = MSB * 256 + LSB(max 128 bytes)
  ///   • D0...D(N-1) – bytes of image data
  ///
  /// 1.5.3 Also Handles the end of loading graphics
  /// Format
  ///   ESC MFB ( L A ESC MFE
  /// </summary>
  public class LoadGraphicChunkDataCommand : IFiscalPrinterCommand<LoadGraphicChunkDataRequest>
  {
    public void Validate(LoadGraphicChunkDataRequest input)
    {
    }

    public void BuildRequest(LoadGraphicChunkDataRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.OpenParenthesis);
      request.Add(FiscalPrinterDividers.L);
      request.Add(FiscalPrinterDividers.D);
      request.Add(FiscalPrinterDividers.Nul);
      request.Add(FiscalPrinterCommands.Mfb);

      request.Add(input.ImageChunkBytes.ToArray());
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}