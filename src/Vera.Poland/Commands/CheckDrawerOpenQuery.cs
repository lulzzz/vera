using System.Collections.Generic;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Responses;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// Check if the drawer is open by querying the <see cref="ReadInternalStatusQuery">internal status</see>
  /// </summary>
  public class CheckDrawerOpenQuery: IFiscalPrinterQuery<DrawerOpenResponse>
  {
    private readonly ReadInternalStatusQuery _readInternalStatusQuery;

    public CheckDrawerOpenQuery()
    {
      _readInternalStatusQuery = new ReadInternalStatusQuery();
    }

    public DrawerOpenResponse ReadResponse(byte[] printerRawResponse)
    {
      var intermediaryResponse = _readInternalStatusQuery.ReadResponse(printerRawResponse);

      if (!intermediaryResponse.Success)
      {
        return new DrawerOpenResponse
        {
          Success = false,
          ResponseMalformed = intermediaryResponse.ResponseMalformed
        };
      }

      var drawerOpen = intermediaryResponse.Word1.HasFlag(InternalStatusWord1.DrawerOneOpened)
                       || intermediaryResponse.Word1.HasFlag(InternalStatusWord1.DrawerTwoOpened);

      return new DrawerOpenResponse
      {
        Success = true,
        DrawerOpen = drawerOpen
      };
    }

    public void BuildRequest(List<byte> request)
    {
      _readInternalStatusQuery.BuildRequest(request);
    }
  }
}