using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests.TotalizersAndCountersReadout;
using Vera.Poland.Models.Responses.TotalizersAndCountersReadout;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.TotalizersAndCountersReadout
{
  /// <summary>
  /// See 4.9.11 Counters and totalizers readout
  ///
  /// Only handles the Counters
  ///
  /// Format:
  ///   ESC MFB L T<name> ESC MFE
  /// 
  /// Argument:
  ///   <name> - The name of the counter or totalizer from the table
  ///
  /// Answer:
  ///   ESC r MSB LSB <type> <value0> .. <valuen-1>
  ///   ... and a lot of rules ...
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class GetCounterQuery : IFiscalPrinterQuery<GetCounterRequest, GetCounterResponse>
  {
    public void Validate(GetCounterRequest input)
    {

    }

    public void BuildRequest(GetCounterRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.L);
      request.Add(FiscalPrinterDividers.T);

      var sanitizedEnumName = TemporaryMagicKeyRemover.RemoveMagicKey(input.Counter.ToString());

      request.AddRange(EncodingHelper.Encode(sanitizedEnumName));

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }

    public GetCounterResponse ReadResponse(byte[] printerRawResponse)
    {
      var hasMsb = printerRawResponse[0] == FiscalPrinterCommands.Esc;
      var hasResponse = printerRawResponse[1] == FiscalPrinterResponses.ResponseArgument;
      var responseIsValid = hasMsb && hasResponse;
      if (!responseIsValid)
      {
        return new GetCounterResponse
        {
          Success = false,
          ResponseMalformed = true
        };
      }

      // Skipping the first 4 bytes containing the response
      // format: ESC r MSB LSB <type> <value0> .. <value n-1>
      // we are skipping ESC r MSB LSB as these values aren't of interest to us
      //

      var responseData = printerRawResponse.Skip(4).ToArray();
      var printerStringDataResponse = EncodingHelper.Decode(responseData);
      var printerCounterResponse = ReadTotalizerOrQueryResponseHelper.ReadCounter(printerStringDataResponse);

      return new GetCounterResponse
      {
        Success = true,
        ResponseMalformed = false,
        CounterType = printerCounterResponse.Item1,
        CounterValue = printerCounterResponse.Item2,
      };
    }
  }
}