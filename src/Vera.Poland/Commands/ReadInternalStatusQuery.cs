using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Responses;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// 1.11.5 Fiscal Module internal status readout
  /// Format
  ///   ESC + f
  /// Note:
  ///   Emulation
  /// Answer
  ///   ESC r MSB LSB<word 1> .. <word 10>
  /// where
  /// • <word 1> .. <word 10> - information contained in status is determined by the bits of returned data words. First, the most significant byte of word is returned
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class ReadInternalStatusQuery : IFiscalPrinterQuery<InternalStatusResponse>
  {
    
    public void BuildRequest(List<byte> request)
    {
      request.Add(FiscalPrinterCommands.Esc);
      request.AddRange(FiscalPrinterArguments.ReadInternalStatusArgument);
    }

    public InternalStatusResponse ReadResponse(byte[] printerRawResponse)
    {
      // 1 word is 2 bytes.
      // 24 bytes answer required

      if (printerRawResponse.Length != 24)
      {
        throw new InvalidOperationException("Expecting response of form: ESC r MSB LSB <word 1> .. <word 10>");
      }

      // check the first two bytes

      var hasMsb = printerRawResponse[0] == FiscalPrinterCommands.Esc;
      var hasResponse = printerRawResponse[1] == FiscalPrinterResponses.ResponseArgument;

      var responseIsValid = hasMsb && hasResponse;

      if (!responseIsValid)
      {
        return new InternalStatusResponse
        {
          Success = false,
          ResponseMalformed = true
        };
      }

      var statusResponseBytes = printerRawResponse[4..];


      var word1 = GetEnum<InternalStatusWord1>(statusResponseBytes[^20..^18]);
      var word2 = GetEnum<InternalStatusWord2>(statusResponseBytes[^18..^16]);
      var word3 = GetEnum<InternalStatusWord3>(statusResponseBytes[^16..^14]);
      var word4 = GetEnum<InternalStatusWord4>(statusResponseBytes[^14..^12]);
      var word5 = GetEnum<InternalStatusWord5>(statusResponseBytes[^12..^10]);
      var word6 = GetEnum<InternalStatusWord6>(statusResponseBytes[^10..^8]);
      var word7 = GetEnum<InternalStatusWord7>(statusResponseBytes[^8..^6]);
      var word8 = GetEnum<InternalStatusWord8>(statusResponseBytes[^6..^4]);
      var word9 = GetEnum<InternalStatusWord9>(statusResponseBytes[^4..^2]);

      return new InternalStatusResponse
      {
        Success = true,
        Word1 = word1,
        Word2 = word2,
        Word3 = word3,
        Word4 = word4,
        Word5 = word5,
        Word6 = word6,
        Word7 = word7,
        Word8 = word8,
        Word9 = word9,
      };
    }

    private static TEnum GetEnum<TEnum>(IList<byte> array) where TEnum : struct, IConvertible
    {
      //
      // We need to reverse this because First, the most significant byte of the word is returned
      //
      var bitArray = new BitArray(new[]
      {
        array[1],
        array[0]
      });

      var result = bitArray.GetEnum<TEnum>();

      return result;
    }
  }
}