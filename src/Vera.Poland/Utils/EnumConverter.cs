using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Vera.Poland.Utils
{
  public static class EnumConverter<TEnum> where TEnum : struct, IConvertible
  {
    /// <summary>
    /// 4 bytes
    /// </summary>
    private const int Size = 32;

    /// <summary>
    /// Converts an enum in the long number representation
    /// </summary>
    public static readonly Func<TEnum, long> ConvertEnumToNumber = GenerateEnumToNumberConverter();

    /// <summary>
    /// Converts a long number into an enum representation
    /// </summary>
    public static readonly Func<long, TEnum> ConvertNumberToEnum = GenerateNumberToEnumConverter();

    /// <summary>
    /// Converts an enum to a byte array
    /// </summary>
    public static byte[] GetByteArrayRepresentation(TEnum @enum)
    {
      var bitArray = GetBitArray(@enum);

      return BitArrayToByteArray(bitArray);
    }

    /// <summary>
    /// Gets a BitArray from a flagged enum
    /// </summary>
    private static BitArray GetBitArray(TEnum @enum)
    {
      // Checks if a flag is present in a value
      //
      static bool IsEnumFlagPresent(long value, long lookingForFlag)
      => (value & lookingForFlag) == lookingForFlag;

      var number = ConvertEnumToNumber(@enum);
      var enumType = typeof(TEnum);

      // Get all possible values of that enum as long numbers
      var values = Enum.GetValues(enumType).Cast<long>().Except(new List<long> { 0 });

      // Get the flags 
      var foundFlags =
        values
          .Where(value => IsEnumFlagPresent(number, value))
          .ToList();

      var listOfBooleans = ProvideList();

      // foreach flag value found we will set the item in the list (defined by power of two index) to true
      foreach (var powerOfTwo in foundFlags.Select(FindPowerOfTwo))
      {
        listOfBooleans[powerOfTwo] = true;
      }

      return new BitArray(listOfBooleans.ToArray());
    }

    /// <summary>
    /// Transform a BitArray to byte array
    /// </summary>
    private static byte[] BitArrayToByteArray(BitArray bits)
    {
      var ret = new byte[(bits.Length - 1) / 8 + 1];
      bits.CopyTo(ret, 0);
      return ret;
    }

    /// <summary>
    /// Provides a list spanning 4 bytes
    /// </summary>
    private static List<bool> ProvideList()
    {
      var list = new List<bool>(Size);
      for (var i = 0; i < Size; i++)
      {
        list.Add(false);
      }

      return list;
    }

    /// <summary>
    /// Finds the power of two from a number
    ///
    /// E.g. 128 will return 7
    /// </summary>
    private static int FindPowerOfTwo(long number)
    {
      var pow = 0;

      if (number == 0)
        return 0;

      while (number != 1)
      {
        if (number % 2 != 0)
        {
          throw new InvalidOperationException("This number is not pow of 2");
        }

        number /= 2;
        pow += 1;
      }

      return pow;
    }

    private static Func<TEnum, long> GenerateEnumToNumberConverter()
    {
      var parameter = Expression.Parameter(typeof(TEnum));
      var dynamicMethod = Expression.Lambda<Func<TEnum, long>>(
        Expression.Convert(parameter, typeof(long)),
        parameter);
      return dynamicMethod.Compile();
    }

    private static Func<long, TEnum> GenerateNumberToEnumConverter()
    {
      var parameter = Expression.Parameter(typeof(long));
      var dynamicMethod = Expression.Lambda<Func<long, TEnum>>(
        Expression.Convert(parameter, typeof(TEnum)),
        parameter);
      return dynamicMethod.Compile();
    }
  }
}