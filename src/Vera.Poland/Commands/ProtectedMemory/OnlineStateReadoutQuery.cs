using System;
using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Responses.ProtectedMemory;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.ProtectedMemory
{
  /// <summary>
  /// 1.8.22 Online state readout
  ///
  /// Format:
  ///   ESC MFB Lx ESC MFE
  ///
  /// Description
  ///   Allows for reads the following data of the printer:
  ///   • Printer installation address,
  ///   • The last JPKID of document, sent to CRK server with date and time ZULU,
  ///   • Counters of Fiscal Memory limits:
  ///     o Daily Reports,
  ///     o General Events,
  ///     o Changes in PTU rates,
  ///     o RAM reset,
  ///     o Exchanges of the public key for cash register,
  ///     o Extended Events,
  ///   • The number of the last receipt in the day,
  ///   • The number of the last printout in the day,
  ///   • Total number of printouts, registered on the printer,
  ///   • Date of last service review or fiscal date.
  /// </summary>
  public class OnlineStateReadoutQuery : IFiscalPrinterQuery<OnlineStateReadoutResponse>
  {
    private const string JpkidPrefix = "#LJPK#";
    private const int ExpectedJpkidLength = 6;

    public void BuildRequest(List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.L);
      request.Add(FiscalPrinterDividers.x);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }

    /// <summary>
    /// Example:
    ///   Request:           ESC MFB Lx ESC MFE
    ///   Printer Response:  ESC 'r' NUL 0xD8 ';#IADR#Gliwice,Gliwice,Bojkowska,44-100,35,4; #LJPK#000067' SP '2019-06-03T14:06:52;
    ///                      #FMLT #6/2100,9/25000,0/30,0/200,0/30,3/1000;#LRCP#000001/0007; #LPRN#000004/0007;#TPRN#000062/0007;#LSRV#2019-06-03
    /// </summary>
    /// <param name="printerRawResponse"></param>
    /// <returns></returns>
    public OnlineStateReadoutResponse ReadResponse(byte[] printerRawResponse)
    {
      // check the first two bytes
      var hasMsb = printerRawResponse[0] == FiscalPrinterCommands.Esc;
      var hasResponse = printerRawResponse[1] == FiscalPrinterResponses.ResponseArgument;

      var responseIsValid = hasMsb && hasResponse;

      if (!responseIsValid)
      {
        return new OnlineStateReadoutResponse
        {
          Success = false,
          ResponseMalformed = true
        };
      }

      var printerStringResponse = EncodingHelper.Decode(printerRawResponse);
      var indexOfJpkidPrefix = printerStringResponse.IndexOf(JpkidPrefix, StringComparison.Ordinal);
      if (indexOfJpkidPrefix == -1)
      {
        return new OnlineStateReadoutResponse
        {
          Success = false,
          ResponseMalformed = true
        };
      }

      // at this point we should have the JPKID guaranteed
      var jpkid = printerStringResponse.Substring(indexOfJpkidPrefix + JpkidPrefix.Length, ExpectedJpkidLength);

      
      var result = new OnlineStateReadoutResponse
      {
        JPKID = jpkid,
        Success = true
      };

      if (uint.TryParse(jpkid, out var parsedJpkId))
      {
        result.ParsedKpkID = parsedJpkId;
      }

      return result;
    }
  }
}