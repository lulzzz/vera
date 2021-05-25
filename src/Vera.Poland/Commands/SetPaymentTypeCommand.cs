using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Extensions;
using Vera.Poland.Contracts;
using Vera.Poland.Models.Requests;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands
{
  /// <summary>
  /// See 4.7.5 Payment type setup for more info
  /// 
  /// Description 
  ///   Defines payment method. A maximum of 15 different payment methods can be defined during each fiscal day.
  /// This command is accepted only at the closed fiscal day.Payment is made on the receipt using the footer
  /// Q(1.3.12 Payments and footers of receipts and invoices).
  /// 
  /// Format
  ///   ESC MFB P<payment_type> ESC MFE
  ///
  /// Arguments
  ///   • <payment_type> - specifies the name of the payment method, which will be printed on the receipt.
  ///   The parameter length is maximum 18 characters.
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  public class SetPaymentTypeCommand : IFiscalPrinterCommand<SetPaymentTypeRequest>
  {
    private const int PaymentTypeMaxLength = 18;


    public void BuildRequest(SetPaymentTypeRequest input, List<byte> request)
    {
      var paymentType = EncodingHelper.Encode(input.PaymentType);

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfb);
      request.Add(FiscalPrinterDividers.P);

      request.AddRange(paymentType);

      request.Add(FiscalPrinterCommands.Esc);
      request.Add(FiscalPrinterCommands.Mfe);
      // Acceptance conditions
      //   • Neutral
    }

    public void Validate(SetPaymentTypeRequest input)
    {
      if (input.PaymentType.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(nameof(SetPaymentTypeRequest.PaymentType));
      }

      if (input.PaymentType.Length > PaymentTypeMaxLength)
      {
        throw new ArgumentOutOfRangeException(
          nameof(SetPaymentTypeRequest.PaymentType),
          $"Must have less than {PaymentTypeMaxLength} characters");
      }
    }
  }
}
