using System.Collections.Generic;
using System.Globalization;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Responses;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// For more information see 1.8.12 Counters and totalizers readout
  ///
  /// This is needed for when we want to do a partial sum on the printer
  /// </summary>
  public class RequestAutoTotalQuery: IFiscalPrinterQuery<AutoTotalResponse>
  {
    /// <summary>
    /// This is the parameter signifying that we want a readout of the total accumulators on the invoice
    /// </summary>
    private const string ReadoutParameter = "tra.tot";
    
    public AutoTotalResponse ReadResponse(byte[] printerRawResponse)
    {

      // check the first two bytes

      var hasMsb = printerRawResponse[0] == FiscalPrinterCommands.Esc;
      var hasResponse = printerRawResponse[1] == FiscalPrinterResponses.ResponseArgument;

      var responseIsValid = hasMsb && hasResponse;

      if (!responseIsValid)
      {
        return new AutoTotalResponse
        {
          Success = false,
          ResponseMalformed = true
        };
      }

      // Answer
      //   ESC r MSB LSB<type> < value0 > .. < value n - 1 >
      //   where:
      //   < type > -counter / accumulator type:
      // 1 - non - index
      // 2 - indexed by tax category
      // 3,4 – not used
      // 5 - indexed by payment method
      //   <value> number with character.
      //   Totalizers are written on 19 bytes by. Format: "snnnnnnnnnnnnnnn, dd " where: s - "+" or "-"(1 character) nnn... – total part(15 characters) ',' - separator(1 character) dd – fractional part(2 characters)
      // Counters are written on 11 bytes in the format "snnnnnnnnnnn” where: s - " + " or " - " (1 character) nnn... – total part (10 characters)

      // we need to read the bytes following the 5th byte

      var value = printerRawResponse[5..];

      // this would hold a value like: +000000000000021,69

      var totalAmountString = EncodingHelper.Decode(value);
      var couldParseDecimal = decimal.TryParse(
        EncodingHelper.Decode(value).Replace(',', '.'),
        NumberStyles.Any,
        CultureInfo.InvariantCulture, out var totalAmount);

      var result = new AutoTotalResponse
      {
        Success = true,
        ParsedOk = couldParseDecimal,
        Raw = totalAmountString,
        Total = totalAmount
      };

      return result;
    }

    public void BuildRequest(List<byte> request)
    {
      var readoutParameterEncoded = EncodingHelper.Encode(ReadoutParameter);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.LT);
      request.Add(readoutParameterEncoded);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
} 
