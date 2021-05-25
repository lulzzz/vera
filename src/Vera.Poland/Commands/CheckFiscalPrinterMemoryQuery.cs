using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Responses;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// Check if the if the fiscal printer memory is full by querying the <see cref="ReadExtendedStatusQuery">extended status</see>
  /// </summary>
  public class CheckFiscalPrinterMemoryQuery: IFiscalPrinterQuery<FiscalPrinterMemoryResponse>
  {
    private readonly ReadExtendedStatusQuery _readExtendedStatusQuery;

    public CheckFiscalPrinterMemoryQuery()
    {
      _readExtendedStatusQuery = new ReadExtendedStatusQuery();
    }

    public void BuildRequest(List<byte> request)
    {
      _readExtendedStatusQuery.BuildRequest(request);
    }

    public FiscalPrinterMemoryResponse ReadResponse(byte[] printerRawResponse)
    {
      var intermediaryResponse = _readExtendedStatusQuery.ReadResponse(printerRawResponse);

      if (!intermediaryResponse.Success)
      {
        return new FiscalPrinterMemoryResponse
        {
          Success = false,
          ResponseMalformed = intermediaryResponse.ResponseMalformed
        };
      }

      var response = new FiscalPrinterMemoryResponse
      {
        FiscalMemoryFull = intermediaryResponse.FiscalStatus.IsFiscalMemoryFull(),
        FiscalMemoryAlmostFull = intermediaryResponse.FiscalStatus.IsFiscalMemoryAlmostFull(),
        Success = true
      };

      return response;
    }
  }
}
