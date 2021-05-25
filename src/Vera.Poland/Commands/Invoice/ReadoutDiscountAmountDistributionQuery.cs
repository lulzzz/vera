using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Models.Responses;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// See 4.3.8 Readout of the distribution of the amount of the discount / uplift to the sum for more information
  ///
  /// Format
  ///   ESC MFB Ld<A-discount/U-uplift> <amount> ESC MFE
  ///
  /// Description
  ///   It read out the distribution of the amount of the discount / uplift
  ///   to the sum that will be calculated by the fiscal module.
  ///   The command is sent before sending the discount.
  ///
  /// Answer
  ///   ESC r MSB LSB<amountA> LF<amountB> LF ... LF<amountG> LF
  ///
  /// Arguments
  ///   • < A-discount /U- uplift>
  ///   • <amountA…G> - amount of discount / uplift to calculate
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class ReadoutDiscountAmountDistributionQuery
  : IFiscalPrinterQuery<ReadoutDiscountAmountDistributionRequest, ReadoutDiscountAmountDistributionResponse>
  {
    private static List<decimal> ReadRates(IEnumerable<byte> response)
    {
      var ratesOrdered = new List<decimal>();
      var rateBytes = new List<byte>();

      foreach (var @byte in response)
      {
        if (@byte != FiscalPrinterDividers.Lf)
        {
          rateBytes.Add(@byte);
        }
        else
        {
          var rate = EncodingHelper.DecodeDecimal(rateBytes.ToArray());
          ratesOrdered.Add(rate);
          rateBytes.Clear();
        }
      }

      return ratesOrdered;
    }

    public void Validate(ReadoutDiscountAmountDistributionRequest input)
    {
      // Check that enums have values
      input.GuardNotNull(x => x.Action);

      if (input.Amount <= 0)
      {
        throw new ArgumentOutOfRangeException(
          nameof(ReadoutDiscountAmountDistributionRequest.Amount),
          $"Value needs to be > 0");
      }
    }

    public void BuildRequest(ReadoutDiscountAmountDistributionRequest input, List<byte> request)
    {
      // Acceptance conditions:
      //   • Sales phase

      var encodedDiscountAction = input.Action.Encode(nameof(input.Action));
      var encodedAmount = EncodingHelper.Encode(input.Amount.ToString("F"));

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.LD);
      request.Add(encodedDiscountAction);
      request.Add(encodedAmount);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }

    public ReadoutDiscountAmountDistributionResponse ReadResponse(byte[] printerRawResponse)
    {
      // check the first two bytes
      var hasEsc = printerRawResponse[0] == FiscalPrinterCommands.Esc;
      var hasResponse = printerRawResponse[1] == FiscalPrinterResponses.ResponseArgument;

      var responseIsValid = hasResponse && hasEsc;

      if (!responseIsValid)
      {
        return new ReadoutDiscountAmountDistributionResponse
        {
          Success = false,
          ResponseMalformed = true
        };
      }

      var responseWithoutFirstBytes = printerRawResponse.Skip(4).ToList();
      var rateList = ReadRates(responseWithoutFirstBytes);

      // rates from A to G
      var expectedCount = Enum.GetNames(typeof(VatClass)).Length - 1;
      if (rateList.Count != expectedCount)
      {
        return new ReadoutDiscountAmountDistributionResponse
        {
          Success = false,
          ResponseMalformed = true
        };
      }

      var amounts = new VatAmounts();
      for (var i = 0; i < expectedCount; i++)
      {
        amounts.AmountsInRates.Single(amount => amount.Vat == (VatClass)i + 1).Amount = rateList[i];
      }

      return new ReadoutDiscountAmountDistributionResponse
      {
        Amounts = amounts,
        Success = true
      };
    }
  }
}