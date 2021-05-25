using System;
using System.Collections.Generic;
using System.Diagnostics;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.HandleGraphics;
using Vera.Poland.Models.Responses.HandleGraphics;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.HandleGraphics
{
  /// <summary>
  /// See 4.5.7 Readout of the graphic’s checksum for more details
  /// 
  /// Format
  ///   ESC MFB(R number ESC MFE
  /// 
  /// Description
  ///   After loading the graphic image, its checksum CRC32 is calculated and then stored in the printer.
  /// The above instruction allows you to read this checksum.After removing the graphic, its checksum is also deleted.
  /// 
  /// Arguments
  ///   • number - from 1 to 8 – the number of the graphic
  /// </summary>
  public class ReadGraphicChecksumQuery : IFiscalPrinterQuery<ReadGraphicChecksumRequest, ReadGraphicChecksumResponse>
  {

    public void Validate(ReadGraphicChecksumRequest input)
    {
      if (input.Graphic == null)
      {
        throw new ArgumentNullException(nameof(input.Graphic), $"{input.Graphic} cannot be null");
      }
    }

    public void BuildRequest(ReadGraphicChecksumRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.OpenParenthesis);
      request.Add(FiscalPrinterDividers.R);

      Debug.Assert(input.Graphic != null, "input.Graphic != null");
      request.Add(EncodingHelper.Encode((int)input.Graphic));

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }

    public ReadGraphicChecksumResponse ReadResponse(byte[] printerRawResponse)
    {
      // check the first two bytes
      var hasMsb = printerRawResponse[0] == FiscalPrinterCommands.Esc;
      var hasResponse = printerRawResponse[1] == FiscalPrinterResponses.ResponseArgument;

      var responseIsValid = hasMsb && hasResponse;

      if (!responseIsValid)
      {
        return new ReadGraphicChecksumResponse
        {
          Success = false,
          ResponseMalformed = true
        };
      }

      var lastEightBytes = printerRawResponse[^8..];
      var checkSum = EncodingHelper.Decode(lastEightBytes);

      var result = new ReadGraphicChecksumResponse
      {
        Checksum = checkSum,
        Success = true
      };

      return result;
    }
  }
}