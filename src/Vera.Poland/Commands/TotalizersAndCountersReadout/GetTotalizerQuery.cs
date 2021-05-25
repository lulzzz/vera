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
  /// See 4.9.11 Totalizers and counters readout
  ///
  /// Only handles the Totalizers
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
  public class GetTotalizerQuery : IFiscalPrinterQuery<GetTotalizerRequest, GetTotalizerResponse>
  {

    public void Validate(GetTotalizerRequest input)
    {

    }

    public void BuildRequest(GetTotalizerRequest input, List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.L);
      request.Add(FiscalPrinterDividers.T);

      var sanitizedEnumName = TemporaryMagicKeyRemover.RemoveMagicKey(input.Totalizer.ToString());

      request.AddRange(EncodingHelper.Encode(sanitizedEnumName));

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
    }

    public GetTotalizerResponse ReadResponse(byte[] printerRawResponse)
    {
      var hasMsb = printerRawResponse[0] == FiscalPrinterCommands.Esc;
      var hasResponse = printerRawResponse[1] == FiscalPrinterResponses.ResponseArgument;
      var responseIsValid = hasMsb && hasResponse;
      if (!responseIsValid)
      {
        return new GetTotalizerResponse
        {
          Success = false,
          ResponseMalformed = true
        };
      }

      var responseData = printerRawResponse.Skip(4).ToArray();
      var printerStringDataResponse = EncodingHelper.Decode(responseData);
      var printerTotalizerResponse = ReadTotalizerOrQueryResponseHelper.ReadTotalizer(printerStringDataResponse);

      return new GetTotalizerResponse
      {
        Success = true,
        ResponseMalformed = false,
        TotalizerType = printerTotalizerResponse.Item1,
        TotalizerValue = printerTotalizerResponse.Item2,
      };
    }
  }
}