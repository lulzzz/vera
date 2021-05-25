using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests.HandleGraphics;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.HandleGraphics
{
  /// <summary>
  /// See 4.5.1 Initialization of the image loading for more details
  /// 
  /// The printer allows you to print monochrome graphics. In addition, it is possible to load any graphic and print them in front of the header
  /// or in the footer of document. For non-fiscal documents it is possible to print them before the header and in the content. To use graphic
  /// user should set parameters by service program. The graphic is automatic printed on the documents by set the parameters: for fiscal receipt
  /// by parameter:
  ///   - graph.bhead_fiscal, for fiscal invoice by parameter
  ///   - graph.bhead_inv, and for the nonfiscal document by parameter
  ///   - graph.bhead_nonfiscal.
  /// A maximum of 8 graphics can be defined. Selecting one of the parameters from 1 to 8 determines the number of the defined graphic to be printed.
  /// If user uses all numbers of parameters for the defined graphics, then it can delete them individually or all and replace them with new ones.
  ///
  /// Format
  ///   ESC MFB ( L B number; LF colour LF X LF Y [ LF name] ESC MFE
  /// 
  /// Arguments
  ///   • X – width (pixels) – max 360
  ///   • Y– height (pixels) - max 256
  ///   • number - the number of the defined image (from 1 to 8)
  ///   • color - from 1 to 99 where:
  ///     1 – black and white image printed from inverted bitmap data
  ///     2...99 – reserved for future use
  ///   • [ LF name] – optional parameter allowing to define the name of the graphic (max 16 characters)
  /// 
  /// The size of the graphic must be a multiple of 8.
  /// The graphics must be saved as a monochrome bitmap (bmp).
  /// Graphics must be sent to the printer in the form of an array of hexadecimal pixel values.
  /// </summary>
  public class InitializeGraphicLoadingCommand : IFiscalPrinterCommand<InitializeGraphicLoadingRequest>
  {
    public void Validate(InitializeGraphicLoadingRequest input)
    {
      if (input.Graphic == null)
      {
        throw new ArgumentNullException(nameof(input.Graphic), $"{input.Graphic} cannot be null");
      }

      if (input.Width == 0 || input.Width > 360)
      {
        throw new ArgumentOutOfRangeException(nameof(input.Width), $"Width cannot be 0 or higher than 360 pixels");
      }

      if (input.Height == 0 || input.Height > 256)
      {
        throw new ArgumentOutOfRangeException(nameof(input.Height), $"Height cannot be 0 or higher than 256 pixels");
      }

      if (!input.Name.IsNullOrWhiteSpace() && input.Name.Length > 16)
      {
        throw new ArgumentOutOfRangeException(nameof(input.Name), $"The name of the graphic cannot exceed 16 characters");
      }
    }

    public void BuildRequest(InitializeGraphicLoadingRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);

      request.Add(FiscalPrinterDividers.OpenParenthesis);
      request.AddRange(FiscalPrinterDividers.LB);

      Debug.Assert(input.Graphic != null, "input.Graphic != null");
      request.AddRange(EncodingHelper.Encode((int)input.Graphic.Value));
      request.Add(FiscalPrinterDividers.Lf);
      request.AddRange(EncodingHelper.Encode(input.Colour));
      request.Add(FiscalPrinterDividers.Lf);

      request.AddRange(EncodingHelper.Encode(input.Width));
      request.Add(FiscalPrinterDividers.Lf);
      request.AddRange(EncodingHelper.Encode(input.Height));
      if (!input.Name.IsNullOrWhiteSpace())
      {
        request.Add(FiscalPrinterDividers.Lf);
        request.AddRange(EncodingHelper.Encode(input.Name));
      }

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }
  }
}