using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// Check 4.3.4   Discount, uplift or reduction to the sales line for more details
  ///
  /// Format
  ///   ESC MFB d<product_name> 0x00 <a-give | c-withdraw> [p-percentage, k-amount]
  ///   ESC MFB1<A-discount | U – uplift | P - reduction> <value> ESC MFB2 <VAT> [LF<value in percentage>] ESC MFE
  ///
  /// Description
  ///   Grants and prints the discount, reduction or uplift to the transaction line.
  ///   Changes the value of sales by a fixed amount.
  ///   Displays the size of the discount or uplift and the sum of the receipt.Updates the receipt accumulators
  ///   corresponding to the given VAT rate.
  ///
  ///   None of the receipt accumulators can be made negative due to this order.
  ///   In the case of an invoice, the order "A-discount" and "P-discount" are handled.
  ///
  /// Arguments
  ///   • <product_name> - specifies the name of the item, which must be shortened to a maximum of 40 characters.
  ///   • <a-give | c- withdraw> - the 'a' option specifies that the discount, reduction or uplift will be granted.
  ///     The 'c' option allows you to modify or cancel the previous operation of granting the discount, reduction or uplift.
  ///   • [p-percentage, k-amount] - specification of the type of the discount: percentage or amount.Using the above parameter,
  ///     it is not necessary to specify the<product_name> in the command,
  ///     provided that the discount is printed directly under the sales line, which it concerns.
  ///     On the receipt, after the word "OPUST", the remaining information about the discount is printed in the same line.
  ///   • <value> - the amount by which sales will be reduced or increased. It must be a positive number.
  ///   • <VAT> - defines the VAT category assigned to a given item name. Range: A…G.
  ///   • [<value in percentage>] - the value given in percent.The addition of this parameter changes the principle of printing and allows the use of a percentage discount.
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  [SuppressMessage("ReSharper", "ParameterOnlyUsedForPreconditionCheck.Local")]
  public class MutateDiscountTransactionLineCommand: IFiscalPrinterCommand<MutateDiscountTransactionLineRequest>
  {
    private const int MaxProductNameCharacters = 40;
    
    public void Validate(MutateDiscountTransactionLineRequest input)
    {
      if (input.ProductName.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(nameof(MutateDiscountTransactionLineRequest.ProductName));
      }

      if (input.ProductName.Length > MaxProductNameCharacters)
      {
        throw new ArgumentOutOfRangeException(
          nameof(MutateDiscountTransactionLineRequest.ProductName),
          $"Must have less than {MaxProductNameCharacters} characters");
      }

      // Check that enums have values
      input.GuardNotNull(x => x.Action);
      input.GuardNotNull(x => x.ActionType);
      input.GuardNotNull(x => x.Type);
      input.GuardNotNull(x => x.Vat);

      if (input.Value <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(input.Value), "Value needs to be > 0");
      }
    }

    public void BuildRequest(MutateDiscountTransactionLineRequest input, List<byte> request)
    {
      // Acceptance conditions:
      //   • Receipt is open
      //   • Receipt is not in the payment phase
      //   • Command is not accepted fully in the case of invoice
      // Possible further operations:
      //   • Sales line
      //   • Discount or uplift to the sales line
      //   • Discount or uplift to the partial sum
      //   • Footer
      //   • Settlement of the returnable packaging
      //   • Conversion or payment with foreign currency
      //   • Cancellation of the sales line
      //   • Cancellation of the transaction
      //   • Sum of the transaction

      var encodedProductName = EncodingHelper.Encode(input.ProductName);
      var encodedVatClass = input.Vat.EncodeVatClass();
      var encodedDiscountAction = input.Action.Encode();
      var encodedDiscountType = input.Type.Encode();
      var encodedDiscountActionType = input.ActionType.Encode();
      var encodedValue = EncodingHelper.Encode(input.Value);

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.d);
      request.Add(encodedProductName);
      request.Add(
        FiscalPrinterDividers.Nul,
        encodedDiscountActionType,
        encodedDiscountType,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb1,
        encodedDiscountAction);

      request.Add(encodedValue);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb2, encodedVatClass);

      if (input.ValueInPercentage.HasValue)
      {
        var encodedValueInPercentage = EncodingHelper.Encode(input.ValueInPercentage.Value);
        request.Add(encodedValueInPercentage);
      }

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);
    }
  }
}
