using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Vera.Poland.Contracts;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Enums;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  ///
  /// NOTE: CANNOT BE USED WITH INVOICE
  /// 
  /// See 1.3.10 Percentage/amount discount/uplift to total of the one tax rate for more details
  ///
  /// Format
  ///   ESC MFB<V – percentage/ v - amount> <A – discount/ U - uplift > ESC MFB1 <value> ESC MFB2 <VAT> ESC MFE
  ///
  /// Description
  ///   The command allows for give amount or percentage discount/uplift for the selected tax rate.
  ///
  /// Arguments
  ///   • <V – percentage discount/uplift, v – amount discount/uplift> – specifies command type
  ///   • <A - discount/U-uplift> - specifies use of the command
  ///   • <value> - discount/uplift value to tax rate – the percentage type requires mark %
  ///   • <VAT> - VAT tax rate of the discount/uplift
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class MutatePercentageDiscountForOneTaxRateCommand: IFiscalPrinterCommand<MutatePercentageDiscountForOneTaxRateRequest>
  {
    public void Validate(MutatePercentageDiscountForOneTaxRateRequest input)
    {
      if (input.Value <= 0)
        throw new ArgumentOutOfRangeException(nameof(input.Value),
          "Value needs to be > 0");

      // check that the enums have values

      input.GuardNotNull(x => x.Vat);
      input.GuardNotNull(x => x.Type);
      input.GuardNotNull(x => x.Action);
    }

    public void BuildRequest(MutatePercentageDiscountForOneTaxRateRequest input, List<byte> request)
    {
      static byte EncodeDiscountType(DiscountType discountType)
      {
        return discountType switch
        {
          DiscountType.Amount => FiscalPrinterDividers.v,
          DiscountType.Percentage => FiscalPrinterDividers.V,
          _ => throw new ArgumentOutOfRangeException(nameof(discountType), discountType, null)
        };
      }

      var encodedDiscountType = EncodeDiscountType(input.Type);
      var encodedDiscountAction = input.Action.Encode(nameof(input.Action));
      var encodedVat = input.Vat.EncodeVatClass();
      var encodedValue = input.Value.ToFixedPointByteArray();

      request.Add(
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb,
        encodedDiscountType,
        encodedDiscountAction,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb1);

      request.Add(encodedValue.ToArray());
      request.Add(
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfb2,
        encodedVat,
        FiscalPrinterCommands.Esc,
        FiscalPrinterCommands.Mfe);

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
    }
  }
}