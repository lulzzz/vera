using System;
using System.Collections;
using Vera.Poland.Utils;

namespace Vera.Poland.Extensions
{
  public static class BitArrayExtensions
  {
    public static TK GetEnum<TK>(this BitArray bitArray) where TK : struct, IConvertible
    {
      var powerOfTwo = 0;
      long number = 0;
      foreach (bool bit in bitArray)
      {
        var intBitValue = bit ? 1 : 0;
        number += (intBitValue * 1 << powerOfTwo);
        powerOfTwo++;
      }

      return EnumConverter<TK>.ConvertNumberToEnum(number);
    }
  }
}