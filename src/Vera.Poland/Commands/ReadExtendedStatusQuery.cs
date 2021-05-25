using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Responses;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.13.4 Extended status for more info
  ///
  /// Format:
  ///   ESC + 0x0C
  ///
  /// Answer
  ///   ESC r MSB LSB<fiscal_status [4 bytes]> < printer_mechanism_status[4 bytes]>
  ///
  /// Please see answer details in the documentation as it's pretty long
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class ReadExtendedStatusQuery : IFiscalPrinterQuery<ExtendedStatusResponse>
  {
    public void BuildRequest(List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.AddRange(FiscalPrinterArguments.ReadExtendedStatusArgument);
    }

    public ExtendedStatusResponse ReadResponse(byte[] printerRawResponse)
    {
      // We expect a printer response of the following form
      // Example
      // Passing the following request
      // ESC + 0x0C
      // will return the following response
      // ESC r NUL 0x08 NUL 0x1F 0x21 0x1A 0x10 NUL NUL NUL

      // the first two bytes that we need to verify are: ESC r
      // we will ignore MSB LSB for now as we only care about the 8 bytes that
      // represent the fiscal status and printer mechanism status


      // we need to check that we have two bytes ESC r, then ignore the next two (MSB LSB)

      if (printerRawResponse.Length != 12)
      {
        throw new InvalidOperationException("Expecting response of form: ESC r MSB LSB<fiscal_status [4 bytes]> < printer_mechanism_status[4 bytes]>");
      }

      // check the first two bytes

      var hasMsb = printerRawResponse[0] == FiscalPrinterCommands.Esc;
      var hasResponse = printerRawResponse[1] == FiscalPrinterResponses.ResponseArgument;

      var responseIsValid = hasMsb && hasResponse;

      if (!responseIsValid)
      {
        return new ExtendedStatusResponse
        {
          Success = false,
          ResponseMalformed = true
        };
      }

      var fiscalStatusBits = new BitArray(printerRawResponse[^8..^4]);
      var printerMechanismBits = new BitArray(printerRawResponse[^4..]);

      var fiscalStatus = fiscalStatusBits.GetEnum<FiscalStatus>();
      var printerMechanismStatus = printerMechanismBits.GetEnum<PrinterMechanismStatus>();

      var extendedStatusResponse = new ExtendedStatusResponse
      {
        PrinterMechanismStatus = printerMechanismStatus,
        FiscalStatus = fiscalStatus,
        Success = true
      };

      return extendedStatusResponse;
    }
  }
}
  
