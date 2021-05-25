using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  ///
  /// NOTE: CANNOT BE USED WITH INVOICE
  /// 
  /// See 4.3.7 Percentage discount or uplift to the partial sum for more details
  ///
  /// Format
  ///   ESC MFB F<A-discount | U-uplift> ESC MFB1<percentage> ESC MFE
  ///
  /// Description
  ///   Lowers or increases proportionally all receipt accumulators.
  ///   The amount by which each accumulator is changed is rounded to +/- 0.01 according to
  ///   standard rounding rules.The discount to the sum expressed in amounts is the difference
  ///   between the sum of the receipt before and after the discount or uplift.
  ///   Prints the lines of the partial sum and the discount or uplift.
  ///   A detailed algorithm for calculating this discount is included in the Arithmetic chapter.
  ///
  ///
  /// Arguments
  ///   • <A-discount | U-uplift> - option A causes the use of a percentage discount, option U causes the use of a percentage uplift.
  ///   • <percentage> - The percentage value referring to the current partial sum of the receipt should be entered as a numerical value with an accuracy of two decimal places and a% sign at the end, e.g. 10.00%.
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class MutatePercentageDiscountForPartialSumCommand
  : IFiscalPrinterCommand<MutatePercentageDiscountForPartialSumRequest>
  {
    public void Validate(MutatePercentageDiscountForPartialSumRequest input)
    {
      if (input.Percentage <= 0)
        throw new ArgumentOutOfRangeException(nameof(input.Percentage),
          "Value needs to be > 0");
    }

    public void BuildRequest(MutatePercentageDiscountForPartialSumRequest input, List<byte> request)
    {

      // Acceptance conditions
      //   • Receipt is open
      //   • Receipt is not in the payment phase
      //   • Command cannot be used at VAT invoice
      // Possible further operations
      //   • Sales line
      //   • Discount or uplift to the sales line
      //   • Discount or uplift to the partial sum
      //   • Footer
      //   • Settlement of the returnable packaging
      //   • Conversion or payment with foreign currency
      //   • Cancellation of the sales line
      //   • Cancellation of the transaction
      //   • Sum of the transaction

      static byte[] EncodeWithTwoDecimalPositions(decimal value)
        => EncodingHelper.Encode(value.ToString("F"));

      var encodedPercentage = EncodeWithTwoDecimalPositions(input.Percentage);
      var encodedAction = input.Action.Encode(nameof(input.Action));

      request.Add(
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.F,
        encodedAction,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb1);

      request.Add(encodedPercentage);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}