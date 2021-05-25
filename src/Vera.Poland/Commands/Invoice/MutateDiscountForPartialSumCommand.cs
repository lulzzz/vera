using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  ///
  /// NOTE: CANNOT BE USED WITH INVOICE
  /// 
  /// See 4.3.6 Discount or uplift amount to the partial sum for more details
  ///
  /// Format
  ///   ESC MFB f<A-discount | U-uplift> ESC MFB1[< amount in a rate A > LF... < amount in a rate G > ESC MFB2] <a-give | c-withdraw> <value> ESC MFE
  ///   Lub
  ///   ESC MFB f<a-discount | u-uplift> ESC MFB1 <a-give | c-withdraw> <value> ESC MFE
  ///
  ///   Provides and prints a discount or a mark-up to the current total.
  ///   A discount or mark-up is a fixed amount subtracted or added to the accumulators of VAT rates,
  ///   in proportion to their current value.
  ///   A detailed algorithm for calculating this discount can be found in the Arithmetic chapter.
  ///
  /// Arguments
  ///   • <A-discount | U-withdraw> - the 'A' option determines the application of the discount,
  ///     the 'U' option selects the uplift operation.
  ///   • [<amount in a rate A>, …, < amount in a rate G>] - determines the values
  ///     that will be subtracted from or added to the accumulator VAT rates category A...G.
  ///     If the tax rate for a given VAT category is not defined then the value for this category
  ///     must be equal to zero (0.00).
  ///     The correctness of the separation of the total value of the discount or uplift on the
  ///     VAT rate accumulators is verified.
  ///     The transmitted value may not differ more than +/- 0.01
  ///     from the value calculated by the fiscal module.
  ///     The distribution of the amount of the discount / uplift must be made in
  ///     accordance with the rules described in the chapter Arithmetic.
  ///   • <value> - determines the total value to be subtracted from or added to the current sum of the receipt.
  ///   • <a-discount |u-uplift> - the 'a' option determines the application of the discount,
  ///     the 'u' option selects the uplift operation.
  ///     For these values, the separation of values is calculated by the fiscal module and is not verified.
  ///   • <a-give |c-withdraw> - the option 'a' specifies that the discount / uplift is to be
  ///     granted and the value of the accumulator will be reduced / increased accordingly.
  ///     The 'c' option allows you to modify or cancel the previous operation of discount / uplift.
  ///     For a command with the 'c' option, the sign of the arithmetic operation performed
  ///     on the accumulator is changed to the opposite one.
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class MutateDiscountForPartialSumCommand: IFiscalPrinterCommand<MutateDiscountForPartialSumRequest>
  {


    public void BuildRequest(MutateDiscountForPartialSumRequest input, List<byte> request)
    {

      //
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

      var encodedDiscountAction = input.Action.Encode(nameof(input.Action));
      var encodedDiscountActionType = input.ActionType.Encode();
      var encodedValue = EncodingHelper.Encode(input.Value);

      request.Add(
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        FiscalPrinterDividers.f,
        encodedDiscountAction,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb1);

      if (input.Amounts != null)
      {
        var encodedAmounts = EncodeAmounts(input.Amounts);
        request.Add(encodedAmounts);
        request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb2);
      }

      request.Add(encodedDiscountActionType);
      request.Add(encodedValue);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }

    private static byte[] EncodeAmounts(VatAmounts amounts)
    {
      // ensure it is ordered
      var orderedAmounts = amounts.AmountsInRates
        .OrderBy(amount => amount.Vat).ToList();

      var bytes = new List<byte>();

      foreach (var amountStringRepresentation in
        orderedAmounts.Select(orderedAmount => orderedAmount.Amount.ToString("F")))
      {
        bytes.AddRange(EncodingHelper.Encode(amountStringRepresentation));
        bytes.Add(FiscalPrinterDividers.Lf);
      }

      return bytes.ToArray();
    }

    private static void ValidateAmounts(VatAmounts amounts)
    {
      if (amounts.AmountsInRates == null || !amounts.AmountsInRates.Any())
      {
        throw new ArgumentNullException(
          nameof(MutateDiscountForPartialSumRequest.Amounts.AmountsInRates));
      }

      ValidateVatClassEntry(amounts, VatClass.A);
      ValidateVatClassEntry(amounts, VatClass.B);
      ValidateVatClassEntry(amounts, VatClass.C);
      ValidateVatClassEntry(amounts, VatClass.D);
      ValidateVatClassEntry(amounts, VatClass.E);
      ValidateVatClassEntry(amounts, VatClass.F);
      ValidateVatClassEntry(amounts, VatClass.G);
    }

    private static void ValidateVatClassEntry(VatAmounts amounts, VatClass vat)
    {
      var vatClass = amounts.AmountsInRates.SingleOrDefault(amount => amount.Vat == vat);

      if (vatClass == null)
      {
        throw new ArgumentNullException(
          nameof(MutateDiscountForPartialSumRequest.Amounts.AmountsInRates),
          $"Entry missing for VAT class {vat}");
      }

      if (vatClass.Amount < 0)
      {
        throw new ArgumentOutOfRangeException(
          nameof(MutateDiscountForPartialSumRequest.Amounts.AmountsInRates),
          $"VAT class {vat} amount needs to be > 0");
      }
    }

    public void Validate(MutateDiscountForPartialSumRequest input)
    {
      // Check that enums have values
      input.GuardNotNull(x => x.Action);
      input.GuardNotNull(x => x.ActionType);

      if (input.Value < 0)
      {
        throw new ArgumentOutOfRangeException(
          nameof(MutateDiscountForPartialSumRequest.Value),
          $"Value needs to be > 0");
      }

      if (input.Amounts != null)
      {
        ValidateAmounts(input.Amounts);
      }
    }
  }
}