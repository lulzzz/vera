using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Vera.Extensions;
using Vera.Poland.Commands.Invoice.Contract;
using Vera.Poland.Extensions;
using Vera.Poland.Models.Requests.Invoice;
using Vera.Poland.Protocol;

namespace Vera.Poland.Commands.Invoice
{
  /// <summary>
  /// See 1.3.3 Advance payment and settlement of the advance for more details
  ///
  /// Format for settlement of the advance line
  /// ESC MFB D<product_name> LF<quantity> * <price> LF CR settlement of the advance <value> LF CR <to_document> LF CR <advance_amount> SP<VAT> ESC MFB1 a<supplement_amount> ESC MFB2 <VAT> ESC MFE
  ///
  /// Arguments
  ///   Explanation is extended - please check the manual
  /// </summary>
  [SuppressMessage("ReSharper", "InvalidXmlDocComment")]
  [SuppressMessage("ReSharper", "EntityNameCapturedOnly.Local")]
  public class SettleAdvanceCommand : IFiscalOrderLineInvoiceCommand<SettleAdvanceRequest>
  {
    public void Validate(SettleAdvanceRequest input)
    {
      this.ValidateBaseRequest(input);

      if (input.SettlementOfTheAdvanceComment.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(nameof(SettleAdvanceRequest.SettlementOfTheAdvanceComment));
      }

      if (input.ToDocument.IsNullOrWhiteSpace())
      {
        throw new ArgumentNullException(nameof(SettleAdvanceRequest.ToDocument));
      }

      if (input.AdvanceAmount == default)
      {
        throw new ArgumentNullException(nameof(SettleAdvanceRequest.AdvanceAmount));
      }
    }

    public void BuildRequest(SettleAdvanceRequest input, List<byte> request)
    {
      var encodedProductName = EncodingHelper.Encode(input.ProductName);
      var encodedQuantity = EncodingHelper.EncodeQuantity(input.Quantity);
      var encodedPrice = EncodingHelper.Encode(input.Price);
      var encodedValue = EncodingHelper.Encode(input.Value);
      var encodedVatClass = input.Vat.EncodeVatClass();
      var encodedToDocument = EncodingHelper.Encode(input.ToDocument);
      var encodedAdvanceAmount = EncodingHelper.Encode(input.AdvanceAmount);
      var encodedSupplementAmount = EncodingHelper.Encode(input.SupplementAmount);

      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb, FiscalPrinterDividers.D);
      request.Add(encodedProductName);
      request.Add(FiscalPrinterDividers.Lf);
      request.Add(encodedQuantity);
      request.Add(FiscalPrinterDividers.Star);
      request.Add(encodedPrice);
      request.Add(FiscalPrinterDividers.Lf, FiscalPrinterDividers.Cr);
      request.Add(EncodingHelper.Encode(input.SettlementOfTheAdvanceComment));
      request.Add(encodedValue);
      request.Add(FiscalPrinterDividers.Lf, FiscalPrinterDividers.Cr);
      request.Add(encodedToDocument);
      request.Add(FiscalPrinterDividers.Lf, FiscalPrinterDividers.Cr);
      request.Add(encodedAdvanceAmount);
      request.Add(FiscalPrinterDividers.Sp);
      request.Add(encodedVatClass);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb1, FiscalPrinterDividers.a);
      request.Add(encodedSupplementAmount);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfb2);
      request.Add(encodedVatClass);
      request.Add(FiscalPrinterCommands.Esc, FiscalPrinterCommands.Mfe);

      // Acceptance conditions
      //   • Receipt is open
      //   • Receipt is not in the payment phase
    }
  }
}
