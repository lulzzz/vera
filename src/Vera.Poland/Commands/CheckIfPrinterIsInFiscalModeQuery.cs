using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Responses;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// Check if the if the fiscal printer is in fiscal mode by querying the <see cref="ReadExtendedStatusQuery">extended status</see>
  /// </summary>
  public class CheckIfPrinterIsInFiscalModeQuery : IFiscalPrinterQuery<CheckIfPrinterIsInFiscalModeResponse>
  {
    private readonly ReadExtendedStatusQuery _readExtendedStatusQuery;

    public CheckIfPrinterIsInFiscalModeQuery()
    {
      _readExtendedStatusQuery = new ReadExtendedStatusQuery();
    }

    public void BuildRequest(List<byte> request)
    {
      _readExtendedStatusQuery.BuildRequest(request);
    }

    public CheckIfPrinterIsInFiscalModeResponse ReadResponse(byte[] printerRawResponse)
    {
      var intermediaryResponse = _readExtendedStatusQuery.ReadResponse(printerRawResponse);

      if (!intermediaryResponse.Success)
      {
        return new CheckIfPrinterIsInFiscalModeResponse
        {
          Success = false,
          ResponseMalformed = intermediaryResponse.ResponseMalformed
        };
      }

      return new CheckIfPrinterIsInFiscalModeResponse
      {
        Success = true,
        PrinterIsInFiscalMode = intermediaryResponse.FiscalStatus.HasFlag(FiscalStatus.PrinterInFiscalMode)
      };
    }
  }
}