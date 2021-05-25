using System;
using System.Collections.Generic;
using Vera.Extensions;
using Vera.Poland.Models.Requests;

namespace Vera.Poland.Helpers
{
  public class PosAddressHelper
  {
    public static List<byte[]> GetPosAddressEncoded(SetPosAddressRequest request, Func<string, byte[]> encode)
    {
      var encodedPosAddressLines = new List<byte[]>();

      void MaybeAdd(string item)
      {
        if (!item.IsNullOrWhiteSpace())
        {
          encodedPosAddressLines.Add(encode(item));
        }
      }

      MaybeAdd(request.Place);
      MaybeAdd(request.TaxOffice);
      MaybeAdd(request.Street);

      MaybeAdd(request.PostalCode);
      MaybeAdd(request.HouseNumber);
      MaybeAdd(request.ApartmentNumber);

      return encodedPosAddressLines;
    }
  }
}