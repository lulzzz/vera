using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Vera.Poland.Contracts;
using Vera.Poland.Models;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.13.7  Internal error readout for more information
  ///
  /// Format
  ///   ESC + e
  ///
  /// Answer
  ///   ESC r MSB LSB<internal_error>
  /// where:
  /// • <internal_error> - internal error code(2 bytes).
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class ListPrinterErrorQuery: IFiscalPrinterQuery<PrinterError>
  {
    public void BuildRequest(List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.AddRange(FiscalPrinterArguments.ListErrorsArgument);
    }

    public PrinterError ReadResponse(byte[] printerRawResponse)
    {
      // We expect a printer response of the following form
      // Example
      // Passing the following request
      // ESC +e
      // will return the following response
      // ESC r 0x00 0x02 0x61 0x81

      // the first two bytes that we need to verify are: ESC r
      // we will ignore MSB LSB for now as we only care about the two other bytes that
      // represent the error code

      // we need to check that we have two bytes ESC r, then ignore the next two (MSB LSB)
      // then compose the remaining two bytes into an integer 

      if (printerRawResponse.Length != 6)
      {
        throw new InvalidDataException("Expecting response of form: ESC r MSB LSB ErrorByte1 ErrorByte2");
      }

      // check the first two bytes

      var hasMsb = printerRawResponse[0] == FiscalPrinterCommands.Esc;
      var hasResponse = printerRawResponse[1] == FiscalPrinterResponses.ResponseArgument;

      var responseIsValid = hasMsb && hasResponse;

      if (!responseIsValid)
      {
        return new PrinterError { ResponseMalformed = true, Success = false };
      }

      var lastTwoBytes = printerRawResponse[^2..];

      var error = FiscalPrinterErrors.GetErrorFor(lastTwoBytes);
      var printerDecodedRawResponse = ProduceHumanReadableError(lastTwoBytes);

      var errorResponse = new PrinterError
      {
        Code = error?.Code ?? -1,
        ErrorDescription = error?.Description ?? "Could not get error description from the printer",
        RawErrorDescription = printerDecodedRawResponse,
        Success = true
      };

      return errorResponse;
    }

    private static string ProduceHumanReadableError(byte[] bytes)
    {
      if (bytes.Length > 2)
      {
        throw new ArgumentOutOfRangeException(nameof(bytes));
      }

      var builder = new StringBuilder();
      builder.Append("ESC r MSB LSB ");

      foreach (var @byte in bytes)
      {
        builder.AppendFormat("0x{0:x2} ", @byte);
      }

      return builder.ToString();
    }
  }
}
