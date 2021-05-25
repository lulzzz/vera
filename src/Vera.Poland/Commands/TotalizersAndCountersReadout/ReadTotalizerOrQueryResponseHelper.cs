using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vera.Extensions;
using Vera.Poland.Models.Enums;

namespace Vera.Poland.Commands.TotalizersAndCountersReadout
{
  /// <summary>
  /// See 4.9.11. Totalizers and counters readout
  ///
  /// Printer Answer:
  ///   ESC r MSB LSB<type> <value0> .. <valuen-1>
  ///     where:
  ///       <type> - counter/accumulator type: 
  ///           1-non-index
  ///           2-indexed by tax category
  ///           3,4 – not used
  ///           5-indexed by payment method
  /// 
  ///       <value> number with character.
  ///
  ///   Totalizers are written on 19 bytes by. Format: "snnnnnnnnnnnnnnn, dd "
  ///           where:
  ///           s - "+" or "-" (1 character)
  ///           nnn... – total part(15 characters)
  ///           ',' - separator(1 character)
  ///           dd – fractional part(2 characters)
  ///
  ///   Counters are written on 11 bytes in the format "snnnnnnnnnnn”
  ///           where:
  ///           s - "+" or "-" (1 character)
  ///           nnn... – total part(10 characters)
  ///
  /// </summary>
  public static class ReadTotalizerOrQueryResponseHelper
  {
    private static (TotalizerOrCounterType, string) GetTypeAndValue(string rawDataResponse)
    {
      var containsMinus = rawDataResponse.Contains('-');
      var containsPlus = rawDataResponse.Contains('+');

      if (containsPlus && containsMinus || !containsPlus && !containsMinus)
      {
        throw new ArgumentOutOfRangeException(rawDataResponse, "Malformed argument");
      }

      var dataResponseSplit = rawDataResponse.Split(containsMinus ? '-' : '+');

      if (!dataResponseSplit.Any() || dataResponseSplit.Length > 2)
      {
        throw new ArgumentOutOfRangeException(rawDataResponse, "Malformed argument");
      }

      var characters = dataResponseSplit[0].ToCharArray();
      var isControlCharacter = characters.Length == 1 && char.IsControl(characters[0]);
      string sanitizedType = "";
      if (isControlCharacter)
      {
        switch (characters[0])
        {
          case '\u0001':
          {
            sanitizedType = "1";
            break;
          }
          case '\u0002':
          {
            sanitizedType = "2";
            break;
          }
          case '\u0003':
          {
            sanitizedType = "3";
            break;
          }
          case '\u0004':
          {
            sanitizedType = "4";
            break;
          }
          case '\u0005':
          {
            sanitizedType = "5";
            break;
          }
        }
      }
      else
      {
        sanitizedType = dataResponseSplit[0].Last().ToString();
      }

      var type = (TotalizerOrCounterType)Convert.ToInt32(sanitizedType);

      var number = string.Concat(
        new List<string?>
        {
          containsMinus ? "-" : "",

          //
          // Only interested in the last character of the type

          dataResponseSplit[1]
        }).Replace(",", ".");


      return (type, number);
    }

    public static Tuple<TotalizerOrCounterType, double> ReadTotalizer(string printerDataResponse)
    {
      if (printerDataResponse.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(printerDataResponse, "printerDataResponse cannot be null or white space");
      }

      var (type, number) = GetTypeAndValue(printerDataResponse);
      var castToDouble = Convert.ToDouble(number, CultureInfo.InvariantCulture);

      return new Tuple<TotalizerOrCounterType, double>(type, castToDouble);
    }

    public static Tuple<TotalizerOrCounterType, long> ReadCounter(string printerDataResponse)
    {
      if (printerDataResponse.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(printerDataResponse, "printerDataResponse cannot be null or white space");
      }

      var (type, number) = GetTypeAndValue(printerDataResponse);

      var castToInt = Convert.ToInt64(number, CultureInfo.InvariantCulture);

      return new Tuple<TotalizerOrCounterType, long>(type, castToInt);
    }
  }
}